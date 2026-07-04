using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniFramework.Event;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialoguePanel : BasePanel
{
/*
1. 显示对话内容:
    1. 背景图片设置
2. 更新对话内容:
    1. 对话内容设置
    2. 对话者设置
3. 点击发送事件

1. 角色图片
2. 对话内容
3. 背景图片
4. 角色名字
5. 退出按钮
*/

#region 属性

    public Image background; // 背景图片
    public Image leftRole; // 左侧角色图片
    public Image rightRole; // 右侧角色图片
    public Image leftRoleBg; // 左侧角色背景图片
    public Image rightRoleBg; // 右侧角色背景图片
    public Image cgImage; // CG图片（全屏CG对话时使用）
    public Image dialogueBoxBg; // 对话框背景图片（Log）
    public TextMeshProUGUI content;
    public TextMeshProUGUI leftRoleName;
    public TextMeshProUGUI rightRoleName;
    public Button quitButton;
    public Button finishButton;
    public Button continueButton;

    public List<Speaker> speakers = new List<Speaker>();

    private CancellationTokenSource typewriterCts;
    private string currentFullText;

#endregion

#region 生命周期

    protected void Awake()
    {
        eventGroup = new();
        eventGroup.AddListener<DiaLogueEventDefine.ShowUI>(OnHandleEventMessage);
        eventGroup.AddListener<DiaLogueEventDefine.UpdateUI>(OnHandleEventMessage);
    }

    protected void Start()
    {
        quitButton.onClick.AddListener(() =>
        {
            TipPanelEventDefine.ShowTip.SendEventMessage("确定要退出对话吗？", "确认", () =>
            {
                // 关闭对话面板
                UIMgr.Instance.HidePanel<DialoguePanel>();
                // 返回地图
                StateEventDefine.ChangeState.SendEventMessage<MapState>("MapState");
            }, "取消", null);
        });

        finishButton.onClick.AddListener(() =>
        {
            // 直接完成文字显示
            if(typewriterCts != null)
            {
                typewriterCts.Cancel();
                typewriterCts.Dispose();
                typewriterCts = null;
            }
            content.text = currentFullText;
            finishButton.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(true);
        });

        continueButton.onClick.AddListener(() =>
        {
            // 发送事件, 下一句对话
            DiaLogueEventDefine.Next.SendEventMessage();
        });
    }

    protected void OnDestroy()
    {
        typewriterCts?.Cancel();
        typewriterCts?.Dispose();
        typewriterCts = null;
        eventGroup.RemoveAllListener();
    }
#endregion

#region 逻辑控制

    public override void HideMe()
    {

    }

    public override void ShowMe()
    {

    }

    private async UniTaskVoid TypewriterEffect(string text, CancellationToken ct)
    {
        content.text = "";
        for(int i = 0; i < text.Length; i++)
        {
            content.text += text[i];
            try
            {
                await UniTask.Delay(50, cancellationToken: ct);
            }
            catch(System.OperationCanceledException)
            {
                return;
            }
        }
        // 打字完成
        finishButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(true);
        typewriterCts?.Dispose();
        typewriterCts = null;
    }

#endregion

#region 事件监听

    public void OnHandleEventMessage(IEventMessage message)
    {
        if(message is DiaLogueEventDefine.ShowUI showUI)
        {
            speakers = showUI.speakers;
            quitButton.gameObject.SetActive(showUI.hasReturnButton);
            background.gameObject.SetActive(showUI.hasBackground);
            if (showUI.hasBackground)
            {
                background.sprite = showUI.BackGround;
            }
        }
        else if(message is DiaLogueEventDefine.UpdateUI updateUI)
        {
            // 取消上一次的打字效果
            if(typewriterCts != null)
            {
                typewriterCts.Cancel();
                typewriterCts.Dispose();
                typewriterCts = null;
            }

            currentFullText = updateUI.content;

            // ── 根据对话类型分别处理 ──
            if (updateUI.dialogueType == EDialogueType.CGDialogue)
            {
                HandleCGDialogue(updateUI);
            }
            else
            {
                HandleCharacterDialogue(updateUI);
            }
        }
    }

    /// <summary>
    /// 处理 CG 对话渲染
    /// </summary>
    private void HandleCGDialogue(DiaLogueEventDefine.UpdateUI updateUI)
    {
        // 隐藏人物立绘（保留名字用于后续判断）
        leftRoleBg.gameObject.SetActive(false);
        leftRole.gameObject.SetActive(false);
        rightRoleBg.gameObject.SetActive(false);
        rightRole.gameObject.SetActive(false);

        // 显示 CG 图片
        if (cgImage != null)
        {
            cgImage.gameObject.SetActive(updateUI.cgImage != null);
            if (updateUI.cgImage != null)
            {
                cgImage.sprite = updateUI.cgImage;
            }
        }

        // 对话框显示控制
        bool showBox = updateUI.showDialogueBox;
        if (dialogueBoxBg != null)
            dialogueBoxBg.gameObject.SetActive(showBox);
        content.gameObject.SetActive(showBox);

        if (showBox)
        {
            // 显示角色名字（不显示立绘），高亮当前说话者
            UpdateRoleNames(updateUI);

            // 有对话框：打字效果
            continueButton.gameObject.SetActive(false);
            finishButton.gameObject.SetActive(true);
            typewriterCts = new CancellationTokenSource();
            TypewriterEffect(currentFullText, typewriterCts.Token).Forget();
        }
        else
        {
            // 隐藏角色名字
            leftRoleName.gameObject.SetActive(false);
            rightRoleName.gameObject.SetActive(false);

            // 无对话框：直接显示继续按钮
            finishButton.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 处理人物对话渲染（原有逻辑）
    /// </summary>
    private void HandleCharacterDialogue(DiaLogueEventDefine.UpdateUI updateUI)
    {
        // 隐藏 CG 图片
        if (cgImage != null)
            cgImage.gameObject.SetActive(false);

        // 显示对话框
        if (dialogueBoxBg != null)
            dialogueBoxBg.gameObject.SetActive(true);
        content.gameObject.SetActive(true);

        // 设置左右角色立绘和名字
        UpdateRoleArtAndNames(updateUI);

        // 开始打字效果
        continueButton.gameObject.SetActive(false);
        finishButton.gameObject.SetActive(true);
        typewriterCts = new CancellationTokenSource();
        TypewriterEffect(currentFullText, typewriterCts.Token).Forget();
    }

    /// <summary>
    /// 人物对话模式：设置左右角色立绘和名字（含高亮/变暗）
    /// </summary>
    private void UpdateRoleArtAndNames(DiaLogueEventDefine.UpdateUI updateUI)
    {
        // 设置左侧角色
        if(updateUI.leftRoleIndex >= 0 && updateUI.leftRoleIndex < speakers.Count)
        {
            var leftSpeaker = speakers[updateUI.leftRoleIndex];
            bool hasArt = leftSpeaker?.CharaArtwork != null;
            leftRoleBg.gameObject.SetActive(hasArt);
            leftRole.gameObject.SetActive(hasArt);
            leftRoleName.gameObject.SetActive(leftSpeaker != null);
            if (hasArt)
                leftRole.sprite = leftSpeaker.CharaArtwork;
            if (leftSpeaker != null)
                leftRoleName.text = leftSpeaker.name;
            leftRole.color = updateUI.speaker == TalkingSpeaker.Left ? Color.white : Color.gray;
            leftRoleName.color = updateUI.speaker == TalkingSpeaker.Left ? Color.white : Color.gray;
        }
        else
        {
            leftRoleBg.gameObject.SetActive(false);
            leftRole.gameObject.SetActive(false);
            leftRoleName.gameObject.SetActive(false);
        }

        // 设置右侧角色
        if(updateUI.rightRoleIndex >= 0 && updateUI.rightRoleIndex < speakers.Count)
        {
            var rightSpeaker = speakers[updateUI.rightRoleIndex];
            bool hasArt = rightSpeaker?.CharaArtwork != null;
            rightRoleBg.gameObject.SetActive(hasArt);
            rightRole.gameObject.SetActive(hasArt);
            rightRoleName.gameObject.SetActive(rightSpeaker != null);
            if (hasArt)
                rightRole.sprite = rightSpeaker.CharaArtwork;
            if (rightSpeaker != null)
                rightRoleName.text = rightSpeaker.name;
            rightRole.color = updateUI.speaker == TalkingSpeaker.Right ? Color.white : Color.gray;
            rightRoleName.color = updateUI.speaker == TalkingSpeaker.Right ? Color.white : Color.gray;
        }
        else
        {
            rightRoleBg.gameObject.SetActive(false);
            rightRole.gameObject.SetActive(false);
            rightRoleName.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// CG 对话模式：仅显示角色名字（不显示立绘），含高亮/变暗
    /// </summary>
    private void UpdateRoleNames(DiaLogueEventDefine.UpdateUI updateUI)
    {
        // 左侧角色名字
        if (updateUI.leftRoleIndex >= 0 && updateUI.leftRoleIndex < speakers.Count)
        {
            var leftSpeaker = speakers[updateUI.leftRoleIndex];
            leftRoleName.gameObject.SetActive(leftSpeaker != null);
            if (leftSpeaker != null)
            {
                leftRoleName.text = leftSpeaker.name;
                leftRoleName.color = updateUI.speaker == TalkingSpeaker.Left ? Color.white : Color.gray;
            }
        }
        else
        {
            leftRoleName.gameObject.SetActive(false);
        }

        // 右侧角色名字
        if (updateUI.rightRoleIndex >= 0 && updateUI.rightRoleIndex < speakers.Count)
        {
            var rightSpeaker = speakers[updateUI.rightRoleIndex];
            rightRoleName.gameObject.SetActive(rightSpeaker != null);
            if (rightSpeaker != null)
            {
                rightRoleName.text = rightSpeaker.name;
                rightRoleName.color = updateUI.speaker == TalkingSpeaker.Right ? Color.white : Color.gray;
            }
        }
        else
        {
            rightRoleName.gameObject.SetActive(false);
        }
    }

#endregion
}
