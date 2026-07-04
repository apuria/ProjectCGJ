using System.Collections;
using System.Collections.Generic;
using UniFramework.Event;
using UnityEngine;
using UnityEngine.UI;

public class MapPanel : BasePanel
{
//
/*
1. 6个节点的图标
2. 退出按钮
3. 继续按钮
4. 根据玩家所处节点设置玩家图标所在位置
*/

#region 属性

    public Button exitButton;
    public Button continueButton;

    public Image node1;
    public Image node2;
    public Image node3;
    public Image node4;
    public Image node5;
    public Image node6;
    public Image nowNode;
    private List<Image> nodeImages;

#endregion
#region 生命周期

    protected void Awake()
    {
        eventGroup = new();
        nodeImages = new List<Image> { node1, node2, node3, node4, node5, node6 };
    }

    protected void Start()
    {
        eventGroup.AddListener<SceneEventDefine.UpdateMapUI>(OnHandleEventMessage);
        UpdateNodes();

        exitButton.onClick.AddListener(() =>
        {
            TipPanelEventDefine.ShowTip.SendEventMessage("确定要退出到主菜单吗？", "确认", () =>
            {
                GameEventDefine.ReturnToMainMenu.SendEventMessage();
            }, "取消", null);
        });

        continueButton.onClick.AddListener(() =>
        {
            SceneEventDefine.NodeGame.SendEventMessage();
        });
    }

    protected void OnDestroy()
    {
        eventGroup.RemoveAllListener();
    }
#endregion

#region 逻辑控制

    public override void HideMe()
    {

    }

    public override void ShowMe()
    {
        UpdateNodes();
    }

    /// <summary>
    /// 更新六个节点的图标状态
    /// </summary>
    private void UpdateNodes()
    {
        var gameNodes = GameManager.Instance.GameNodes;
        var nowFlow = GameManager.Instance.PlayerData.NowFlow;

        for (int i = 0; i < nodeImages.Count && i < gameNodes.Count; i++)
        {
            // 设置节点图标
            if (gameNodes[i].icon != null)
            {
                nodeImages[i].sprite = gameNodes[i].icon;
                nodeImages[i].enabled = true;
            }
            else
            {
                nodeImages[i].enabled = false;
            }
        }

        // 设置当前节点指示器位置
        int flowIndex = (int)nowFlow;
        if (flowIndex >= 0 && flowIndex < nodeImages.Count && nowNode != null)
        {
            var target = nodeImages[flowIndex].transform as RectTransform;
            nowNode.enabled = true;
            nowNode.transform.SetAsLastSibling();
            nowNode.rectTransform.position = target.position;
        }
        else if (nowNode != null)
        {
            nowNode.enabled = false;
        }
    }

#endregion

#region 事件监听

    public void OnHandleEventMessage(IEventMessage message)
    {
        if (message is SceneEventDefine.UpdateMapUI)
        {
            UpdateNodes();
        }
    }

#endregion
}
