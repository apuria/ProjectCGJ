using System.Collections;
using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;

public struct PlayerBattleInfo : IStateData
{
    public RoleInfo roleInfo;
    public List<RoleInfo> roleList;
    public List<EnemyInfo> enemyList;
    /// <summary>
    /// 当前回合的完整行动队列（第一顺位为当前行动者）
    /// </summary>
    public List<BaseInfo> actionQueue;

    public PlayerBattleInfo(RoleInfo roleInfo, List<RoleInfo> roleList,
        List<EnemyInfo> enemyList, List<BaseInfo> actionQueue)
    {
        this.roleInfo = roleInfo;
        this.roleList = roleList;
        this.enemyList = enemyList;
        this.actionQueue = actionQueue;
    }
}

public struct EnemyBattleInfo : IStateData
{
    public EnemyInfo enemyInfo;
    public int round;
    public List<RoleInfo> roleList;
    public List<EnemyInfo> enemyList;
    /// <summary>
    /// 当前回合的完整行动队列（第一顺位为当前行动者）
    /// </summary>
    public List<BaseInfo> actionQueue;

    public EnemyBattleInfo(EnemyInfo enemyInfo, int round, List<RoleInfo> roleList,
        List<EnemyInfo> enemyList, List<BaseInfo> actionQueue)
    {
        this.enemyInfo = enemyInfo;
        this.round = round;
        this.roleList = roleList;
        this.enemyList = enemyList;
        this.actionQueue = actionQueue;
    }
}

public enum EnemyActionType
{
    Defence,
    Attack,
    Skill
}
