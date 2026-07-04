using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Event;

public class MapState : BaseState
{
// 地图状态
/*
1. 监听地图UI的开启按钮，点击后发送消息，跳转节点事件
*/
    PlayerData playerData;
    public override void OnCreate(StateMachine machine, IStateData data)
    {
        base.OnCreate(machine, data);
        UIMgr.Instance.ShowPanel<MapPanel>(isSync: true);
    }

    public override void OnEnter()
    {
        MusicEventDefine.PlayBGM.SendEventMessage("DefaultBGM");

        // 回到地图时清除所有挂起的状态节点，确保状态机处于干净状态
        Machine.ClearSuspendedNodes();

        playerData = GameManager.Instance.PlayerData;
        var gm = GameManager.Instance;
        switch(playerData.NowFlow)
        {
            case PlayerData.Flow.Start:
                if (gm.HasStartEvent)
                {
                    SceneEventDefine.StartGame.SendEventMessage();
                }
                else
                {
                    // 没有开始事件，直接跳到下一个节点，避免卡住
                    gm.NextNode();
                }
                GameEventDefine.SaveProgress.SendEventMessage();
                break;
            case PlayerData.Flow.End:
                if (gm.HasEndEvent)
                {
                    SceneEventDefine.EndGame.SendEventMessage();
                }
                else
                {
                    // 没有结束事件，直接跳到下一个
                    gm.NextNode();
                }
                GameEventDefine.SaveProgress.SendEventMessage();
                break;
            default:
                // 通关后（End 事件已完成，NowFlow > End），回退到最后一个节点并返回主菜单
                if (playerData.NowFlow > PlayerData.Flow.End)
                {
                    playerData.NowFlow = PlayerData.Flow.Node6;
                    GameEventDefine.SaveProgress.SendEventMessage();
                    StateEventDefine.ChangeState.SendEventMessage<GameStart>("GameStart");
                    return;
                }
                GameEventDefine.SaveProgress.SendEventMessage();
                break;
        }
    }

    public override void OnExit()
    {
        
    }

    public override void OnUpdate()
    {

    }

    public override void OnDispose()
    {
        UIMgr.Instance.HidePanel<MapPanel>(true);
        base.OnDispose();
    }

    public override void OnHandleEventMessage(IEventMessage message)
    {
        
    }

}
