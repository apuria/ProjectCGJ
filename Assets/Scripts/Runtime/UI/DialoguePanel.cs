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
        // 隐藏人物立绘（名字背景板和名字由 UpdateRoleNames 统一管理）
        leftRole.gameObject.SetActive(false);
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
            // 隐藏角色名字和名字背景板
            leftRoleBg.gameObject.SetActive(false);
            leftRoleName.gameObject.SetActive(false);
            rightRoleBg.gameObject.SetActive(false);
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
    /// 规则：
    /// 1. 当角色索引为 -1，或当前索引没有角色时，不显示名字和立绘
    /// 2. 当角色没有立绘但有名字时，只显示名字
    /// </summary>
    private void UpdateRoleArtAndNames(DiaLogueEventDefine.UpdateUI updateUI)
    {
        if (speakers == null)
        {
            HideAllRoleUI();
            return;
        }

        // 设置左侧角色
        ApplyRoleArtAndName(
            updateUI.leftRoleIndex,
            updateUI.speaker == TalkingSpeaker.Left,
            leftRoleBg, leftRole, leftRoleName);

        // 设置右侧角色
        ApplyRoleArtAndName(
            updateUI.rightRoleIndex,
            updateUI.speaker == TalkingSpeaker.Right,
            rightRoleBg, rightRole, rightRoleName);
    }

    /// <summary>
    /// 为单侧角色设置立绘和名字
    /// </summary>
    /// <param name="roleIndex">角色在 speakers 列表中的索引，-1 表示无角色</param>
    /// <param name="isTalking">该侧是否为当前说话方，用于高亮判断</param>
    /// <param name="roleBg">立绘背景 Image</param>
    /// <param name="roleArt">立绘 Image</param>
    /// <param name="roleName">名字 TextMeshProUGUI</param>
    private void ApplyRoleArtAndName(int roleIndex, bool isTalking, Image roleBg, Image roleArt, TextMeshProUGUI roleName)
    {
        // 规则1：索引为 -1 或 speakers 中无此角色 → 隐藏名字和立绘
        Speaker speaker = GetSpeaker(roleIndex);
        if (speaker == null)
        {
            roleBg.gameObject.SetActive(false);
            roleArt.gameObject.SetActive(false);
            roleName.gameObject.SetActive(false);
            return;
        }

        // 规则2：有角色 → 根据是否有立绘分别处理
        bool hasArt = speaker.CharaArtwork != null;
        roleArt.gameObject.SetActive(hasArt);
        if (hasArt)
        {
            roleArt.sprite = speaker.CharaArtwork;
        }

        // 名字：有名字则显示
        bool hasName = !string.IsNullOrEmpty(speaker.name);
        roleName.gameObject.SetActive(hasName);
        if (hasName)
        {
            roleName.text = speaker.name;
        }

        // 名字背景板（SpeakerNameBackGround）：
        // 注意：roleName 是 roleBg 的子对象，所以必须确保
        // roleBg 在有名字或有立绘时都处于激活状态
        roleBg.gameObject.SetActive(hasArt || hasName);

        // 高亮/变暗：当前说话侧高亮，另一侧变暗
        Color tint = isTalking ? Color.white : Color.gray;
        roleArt.color = tint;
        roleName.color = isTalking ? Color.black : Color.gray;
    }

    /// <summary>
    /// 根据索引安全获取 Speaker，索引为 -1 或越界返回 null
    /// </summary>
    private Speaker GetSpeaker(int index)
    {
        if (speakers == null || index < 0 || index >= speakers.Count)
            return null;
        return speakers[index];
    }

    /// <summary>
    /// CG 对话模式：仅显示角色名字（不显示立绘），含高亮/变暗
    /// 规则：
    /// 1. 当角色索引为 -1，或当前索引没有角色时，不显示名字
    /// 2. 当角色没有立绘但有名字时，只显示名字（CG 模式下立绘始终隐藏）
    /// </summary>
    private void UpdateRoleNames(DiaLogueEventDefine.UpdateUI updateUI)
    {
        if (speakers == null)
        {
            leftRoleBg.gameObject.SetActive(false);
            leftRoleName.gameObject.SetActive(false);
            rightRoleBg.gameObject.SetActive(false);
            rightRoleName.gameObject.SetActive(false);
            return;
        }

        // 左侧角色名字
        ApplyRoleNameOnly(updateUI.leftRoleIndex, updateUI.speaker == TalkingSpeaker.Left,
            leftRoleBg, leftRoleName);

        // 右侧角色名字
        ApplyRoleNameOnly(updateUI.rightRoleIndex, updateUI.speaker == TalkingSpeaker.Right,
            rightRoleBg, rightRoleName);
    }

    /// <summary>
    /// 为单侧角色仅设置名字（CG 模式用，不显示立绘）
    /// </summary>
    private void ApplyRoleNameOnly(int roleIndex, bool isTalking, Image roleBg, TextMeshProUGUI roleName)
    {
        // 规则1：索引为 -1 或 speakers 中无此角色 → 隐藏名字和名字背景板
        Speaker speaker = GetSpeaker(roleIndex);
        if (speaker == null || string.IsNullOrEmpty(speaker.name))
        {
            roleBg.gameObject.SetActive(false);
            roleName.gameObject.SetActive(false);
            return;
        }

        // 规则2：有名字 → 显示名字和名字背景板（CG 模式下立绘已在 HandleCGDialogue 中统一隐藏）
        roleBg.gameObject.SetActive(true);
        roleName.gameObject.SetActive(true);
        roleName.text = speaker.name;
        roleName.color = isTalking ? Color.black : Color.gray;
    }

    /// <summary>
    /// 隐藏所有角色 UI（立绘背景、立绘、名字）
    /// </summary>
    private void HideAllRoleUI()
    {
        leftRoleBg.gameObject.SetActive(false);
        leftRole.gameObject.SetActive(false);
        leftRoleName.gameObject.SetActive(false);
        rightRoleBg.gameObject.SetActive(false);
        rightRole.gameObject.SetActive(false);
        rightRoleName.gameObject.SetActive(false);
    }

#endregion
}
