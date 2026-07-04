using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UniFramework.Machine;

[Serializable]
public class EndBranchSlot
{
    [Tooltip("匹配 PlayerData.branchList 的 key")]
    [FormerlySerializedAs("text")]
    public string index;
    [Tooltip("匹配 PlayerData.branchList 的 value（为空则只检查 key 是否存在）")]
    public string chosen;
}


[Serializable]
public class EndBranch
{
    [Tooltip("分支条件列表：用 index 匹配 branchList 的 key，chosen 匹配 value，全部满足才执行")]
    public List<EndBranchSlot> branches = new List<EndBranchSlot>();

    [Tooltip("条件满足后执行的对话配置")]
    public DialogueSetting Setting;
}

[CreateAssetMenu(fileName = "EndSetting", menuName = "Settings/EndSetting", order = 1)]
public class EndSetting : ScriptableObject, IStateData
{
    [SerializeField]
    public List<EndBranch> endBranches = new List<EndBranch>();
}
