using System;
using UnityEngine;

/// <summary>
/// 战斗开始时做什么的枚举
/// </summary>
public enum EBattleStartEvent
{
    /// <summary>
    /// 无特殊事件，直接开始战斗
    /// </summary>
    None,
    /// <summary>
    /// 播放对话
    /// </summary>
    PlayDialogue,
    /// <summary>
    /// 开始分支选项
    /// </summary>
    StartBranch,
}

/// <summary>
/// 战斗结束时做什么的枚举
/// </summary>
public enum EBattleEndEvent
{
    /// <summary>
    /// 无特殊事件
    /// </summary>
    None,
    /// <summary>
    /// 下一个对话
    /// </summary>
    NextDialogue,
    /// <summary>
    /// 回到上一个状态
    /// </summary>
    GoBackToLastState,
    /// <summary>
    /// 开始分支选项
    /// </summary>
    StartBranch,
    /// <summary>
    /// 回到地图
    /// </summary>
    GoBackToMap,
    /// <summary>
    /// 结束分支，回到地图界面，玩家数据进入下一个节点
    /// </summary>
    EndBranch,
    /// <summary>
    /// 进入 EndState，根据 EndSetting 中的分支条件决定后续流程
    /// </summary>
    End,
}

/// <summary>
/// 中途事件触发条件类型
/// </summary>
public enum EMidEventTriggerType
{
    /// <summary>
    /// 角色血量跌到指定百分比时触发
    /// </summary>
    CharacterHpThreshold,
    /// <summary>
    /// 敌人血量跌到指定百分比时触发
    /// </summary>
    EnemyHpThreshold,
    /// <summary>
    /// 到达指定回合时触发
    /// </summary>
    RoundCount,
}

/// <summary>
/// 中途事件触发后做什么
/// </summary>
public enum EMidEventAction
{
    /// <summary>
    /// 播放对话
    /// </summary>
    PlayDialogue,
    /// <summary>
    /// 开始分支选项
    /// </summary>
    StartBranch,
}

/// <summary>
/// 中途事件配置
/// </summary>
[Serializable]
public class MidBattleEvent
{
    [Tooltip("触发条件类型")]
    public EMidEventTriggerType triggerType = EMidEventTriggerType.CharacterHpThreshold;

    [Tooltip("目标索引\n- CharacterHpThreshold: 角色索引\n- EnemyHpThreshold: 敌人索引\n- TurnCount: 不使用此字段")]
    public int targetIndex;

    [Tooltip("血量百分比阈值（0~100），仅在 CharacterHpThreshold / EnemyHpThreshold 时使用")]
    [Range(0, 100)]
    public int hpPercentage = 50;

    [Tooltip("第几回合触发，仅在 TurnCount 时使用")]
    public int roundCount = 1;

    [Tooltip("触发后做什么")]
    public EMidEventAction action = EMidEventAction.PlayDialogue;

    [Tooltip("触发后播放的对话（action = PlayDialogue 时使用）")]
    public DialogueSetting dialogue;

    [Tooltip("触发后开启的分支（action = StartBranch 时使用）")]
    public BranchSetting branch;
}

[Serializable]
public class MoreRoleBranch
{
    public int roleIndex;
    public string branchId;
    public string choose;
}
