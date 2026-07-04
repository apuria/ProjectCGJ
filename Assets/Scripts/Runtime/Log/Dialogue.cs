using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话类型
/// </summary>
public enum EDialogueType
{
    /// <summary>
    /// 人物对话（保留原有功能）
    /// </summary>
    CharacterDialogue,
    /// <summary>
    /// CG对话（全屏图片）
    /// </summary>
    CGDialogue,
}

public enum EOnEnd
{
    /// <summary>
    /// 下一句话
    /// </summary>
    NextDialogue,
    /// <summary>
    /// 开启战斗
    /// </summary>
    StartBattle,
    /// <summary>
    /// 回到上一个状态
    /// </summary>
    GoBackToLastState,
    ///　<summary>
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
    /// 下一个对话
    /// </summary>
    NextLog,
    /// <summary>
    /// 进入 EndState，根据 EndSetting 中的分支条件决定后续流程
    /// </summary>
    End,
    /// <summary>
    /// 回到主菜单，清除所有挂起状态，经过加载界面
    /// </summary>
    GoBackToMenu
}

public enum TalkingSpeaker
{
    Left,
    Right,
}


[Serializable]
public class Dialogue
{
    [Tooltip("对话类型：人物对话 或 CG对话")]
    public EDialogueType dialogueType = EDialogueType.CharacterDialogue;

    [Tooltip("CG图片（dialogueType = CGDialogue 时使用）")]
    public Sprite cgImage;

    [Tooltip("是否显示对话框（dialogueType = CGDialogue 时使用）")]
    public bool showDialogueBox = true;

    [Tooltip("对话角色ID, -1则没有, 不要超过角色数量")]
    public int leftSpeakerIndex = -1;
    [Tooltip("对话角色ID, -1则没有, 不要超过角色数量")]
    public int rightSpeakerIndex = -1;
    public TalkingSpeaker talkingSpeaker = TalkingSpeaker.Left;

    [Tooltip("说话文本")]
    [TextArea(5, 10)]
    public string text;



    public EOnEnd onEnd = EOnEnd.NextDialogue;

    [Tooltip("结束后开启的战斗配置（onEnd = StartBattle 时使用）")]
    public BattleSetting Battle;

    [Tooltip("结束后开启的分支配置（onEnd = StartBranch 时使用）")]
    public BranchSetting Branch;

    [Tooltip("结束后的下一个对话（onEnd = NextLog 时使用）")]
    public DialogueSetting NextLog;

    [Tooltip("结束后进入的 EndState 配置（onEnd = End 时使用）")]
    public EndSetting End;

}
