using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Event;
public class GameStart : BaseState
{
//TODO：游戏开始
/*
1. 监听按钮
    1. 开始游戏:    发送消息，切换到游戏状态
    2. 新游戏:      发送消息，切换到游戏状态
    3. 退出游戏:    发送消息，退出游戏
*/
    
    public override void OnCreate(StateMachine machine, IStateData data)
    {
        base.OnCreate(machine, data);
        UIMgr.Instance.ShowPanel<StartPanel>(isSync: true);
    }

    public override void OnEnter()
    {
        MusicEventDefine.PlayBGM.SendEventMessage("DefaultBGM");
    }

    public override void OnExit()
    {
        
    }

    public override void OnUpdate()
    {

    }

    public override void OnDispose()
    {
        UIMgr.Instance.HidePanel<StartPanel>(true);
        base.OnDispose();
    }

    public override void OnHandleEventMessage(IEventMessage message)
    {
        
    }
}