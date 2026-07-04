using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;

[CreateAssetMenu(fileName = "Branch", menuName = "Setting/Branch")]
public class BranchSetting : ScriptableObject, IStateData
{
    [Tooltip("当前分支选项对应的选择配置")]
    public string id;

    [Tooltip("分支选项列表")]
    public List<Branch> branches;

    [Tooltip("选择结束后让状态机做什么")]
    public EOnEnd onEnd;

    [Tooltip("结束后的下一个对话（onEnd = NextDialogue 时使用）")]
    public DialogueSetting nextDialogue;

    [Tooltip("结束后开启的战斗配置（onEnd = StartBattle 时使用）")]
    public BattleSetting battleSetting;

    [Tooltip("结束后开启的分支配置（onEnd = StartBranch 时使用）")]
    public BranchSetting nextBranch;

    [Tooltip("结束后进入的 EndState 配置（onEnd = End 时使用）")]
    public EndSetting endSetting;
}
