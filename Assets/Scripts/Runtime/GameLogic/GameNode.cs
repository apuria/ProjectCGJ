using System;
using UnityEngine;

/// <summary>
/// 游戏节点配置：在 Inspector 中选择节点类型后，拖入对应的 Setting。
/// </summary>
[Serializable]
public class GameNode
{
    [Tooltip("节点名称（用于识别）")]
    public string nodeName;

    [Tooltip("节点类型")]
    public GameState stateType;

    [Tooltip("节点图标")]
    public Sprite icon;

    [Tooltip("拖入 BattleSetting")]
    public BattleSetting battleSetting;

    [Tooltip("拖入 DialogueSettings")]
    public DialogueSetting dialogueSetting;

    [Tooltip("拖入 BranchSetting")]
    public BranchSetting branchSetting;
}
