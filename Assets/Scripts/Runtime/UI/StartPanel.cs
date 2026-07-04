using System.Collections;
using System.Collections.Generic;
using UniFramework.Event;
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : BasePanel
{
/*
1. 开始游戏
2. 新游戏
3. 退出游戏
*/

#region 属性

    public Button btnQuit;
    public Button btnStart;
    public Button btnNewGame;

#endregion

#region 生命周期

    protected void Awake()
    {
        eventGroup = new();
    }

    protected void Start()
    {
        btnQuit.onClick.AddListener(() =>
        {
            GameEventDefine.QuitGame.SendEventMessage();
        });

        btnStart.onClick.AddListener(() =>
        {
            GameEventDefine.ContinueGame.SendEventMessage();
        });

        btnNewGame.onClick.AddListener(() =>
        {
            TipPanelEventDefine.ShowTip.SendEventMessage("确定要开始新游戏吗？当前存档将被覆盖。", "确认", () =>
            {
                GameEventDefine.NewGame.SendEventMessage();
            }, "取消", null);
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

    }

#endregion

#region 事件监听

    public void OnHandleEventMessage(IEventMessage message)
    {
        
    }

#endregion  
}
