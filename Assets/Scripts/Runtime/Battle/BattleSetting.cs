using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New Battle Setting", menuName = "Battle/New Battle Setting")]
public class BattleSetting : ScriptableObject, IStateData
{
    /*
    1. 战斗场景
    2. 敌人列表 (最多三个)
    3. 开始事件, 开始做什么的枚举
    4. 胜利事件, 战斗胜利时做什么的枚举
    5. 失败事件, 战斗失败时做什么的枚举
    6. 中途满足特定条件的事件, 做什么的枚举
    */

    [Header("Music Settings")]
    [Tooltip("背景音乐名称（留空则播放 DefaultBGM）")]
    public string bgmName;

    public Sprite Background;

    [Tooltip("敌人列表（最多三个）")]
    public List<EnemyInfo> enemies = new();

#region 

    [Header("道具分支")]
    [Tooltip("道具分支选项列表")]
    public List<MoreItemBranch> moreItemBranches = new();

#endregion

#region 多角色分支

    [Header("多角色分支")]
    [Tooltip("多角色分支选项列表")]
    public List<MoreRoleBranch> moreRoleBranches = new();

#endregion

#region 开始事件
    [Header("开始事件")]
    [Tooltip("战斗开始时做什么")]
    public EBattleStartEvent startEvent = EBattleStartEvent.None;

    [Tooltip("开始事件为 PlayDialogue 时使用的对话配置")]
    public DialogueSetting startDialogue;

    [Tooltip("开始事件为 StartBranch 时使用的分支配置")]
    public BranchSetting startBranch;
#endregion

#region 胜利事件

    [Header("胜利事件")]
    [Tooltip("战斗胜利时做什么")]
    public EBattleEndEvent winEvent = EBattleEndEvent.None;

    [Tooltip("胜利事件为 NextDialogue 时使用的对话配置")]
    public DialogueSetting winDialogue;

    [Tooltip("胜利事件为 StartBranch 时使用的分支配置")]
    public BranchSetting winBranch;

#endregion

#region 失败事件

    [Header("失败事件")]
    [Tooltip("战斗失败时做什么")]
    public EBattleEndEvent loseEvent = EBattleEndEvent.None;

    [Tooltip("失败事件为 NextDialogue 时使用的对话配置")]
    public DialogueSetting loseDialogue;

    [Tooltip("失败事件为 StartBranch 时使用的分支配置")]
    public BranchSetting loseBranch;

    [Tooltip("胜利事件为 End 时使用的 EndSetting 配置")]
    public EndSetting winEndSetting;

    [Tooltip("失败事件为 End 时使用的 EndSetting 配置")]
    public EndSetting loseEndSetting;

#endregion

#region 中途事件

    [Header("中途事件")]
    [Tooltip("战斗中满足特定条件时触发的事件列表")]
    public List<MidBattleEvent> midEvents = new();

#endregion
}
