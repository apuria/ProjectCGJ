using System.Collections;
using System.Collections.Generic;
using UniFramework.Event;
using UniFramework.Machine;
using UnityEngine;

public class EnemyBattleState : BaseBattleState
{

    public EnemyBattleInfo info;
    public override void OnCreate(StateMachine machine, IStateData stateData)
    {
        base.OnCreate(machine, stateData);
        info = (EnemyBattleInfo)stateData;
    }

    public override void OnEnter()
    {
        // 1. 发送 UpdateActionSide 事件，由 BattlePanel 更新 UI
        BattleEventDefine.UpdateActionSide.SendEventMessage(
            roleList: info.roleList,
            enemyList: info.enemyList,
            actionQueue: info.actionQueue
        );

        // 2. 执行敌方 AI（决策 → 攻击 → 发送 EnemyActionDelay → EndTurn）
        EnemyAI.DoTurn(info);
    }

    public override void OnExit()
    {

    }

    public override void OnUpdate()
    {

    }

    public override void OnDispose()
    {
        base.OnDispose();
    }
}
