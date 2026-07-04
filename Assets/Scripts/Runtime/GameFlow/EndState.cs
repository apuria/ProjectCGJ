using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Event;

public class EndState : BaseState
{

// 结局分支系统
/*
1. 接收 EndSetting 数据
2. 按顺序遍历 EndSetting 中的 endBranches
3. 对每个 endBranch：
   - 如果 branches 为空，直接执行对应的 DialogueSetting，切换到 DialogueState
   - 如果 branches 不为空，检查 PlayerData.branchList 是否包含全部条件：
     - EndBranchSlot.index 作为 key，EndBranchSlot.chosen 作为期望值（chosen 为空则只检查 key 存在）
   - 全部满足则执行该 endBranch 的 DialogueSetting，切换到 DialogueState
4. 若没有匹配的 endBranch / EndSetting 无效 / PlayerData 为 null：
   - 清除所有挂起状态（Machine.ClearSuspendedNodes）
   - 返回主菜单（GameStart）
*/

    private EndSetting endSetting;

    public override void OnCreate(StateMachine machine, IStateData data)
    {
        base.OnCreate(machine, data);
        endSetting = data as EndSetting;
    }

    public override void OnEnter()
    {
        if (endSetting == null || endSetting.endBranches == null || endSetting.endBranches.Count == 0)
        {
            Debug.LogWarning("EndState.OnEnter: EndSetting 为空或没有配置 endBranches，返回主菜单");
            GoToMainMenu();
            return;
        }

        var playerData = GameManager.Instance.PlayerData;

        var branchList = playerData?.branchList;

        // 按顺序遍历 endBranches
        foreach (var endBranch in endSetting.endBranches)
        {
            if (endBranch == null) continue;
            if (endBranch.Setting == null)
            {
                Debug.LogWarning("EndState: endBranch 的 Setting 为空，跳过");
                continue;
            }

            // 条件检查：branches 为空则直接执行
            if (endBranch.branches == null || endBranch.branches.Count == 0)
            {
                ExecuteEndBranch(endBranch);
                return;
            }

            // 检查所有条件是否满足
            if (CheckAllBranches(endBranch.branches, branchList))
            {
                ExecuteEndBranch(endBranch);
                return;
            }
        }

        // 没有匹配的 endBranch，返回主菜单并清理所有挂起状态
        Debug.LogWarning("EndState: 没有匹配的 endBranch，返回主菜单");
        GoToMainMenu();
    }

    /// <summary>
    /// 检查 endBranch.branches 中的全部条件是否满足
    /// EndBranchSlot.index 匹配 branchList 的 key，chosen 匹配 value
    /// </summary>
    private bool CheckAllBranches(List<EndBranchSlot> conditions, Dictionary<string, string> branchList)
    {
        foreach (var condition in conditions)
        {
            if (condition == null) continue;

            // index 作为 key 在 branchList 中查找
            if (!branchList.ContainsKey(condition.index))
            {
                return false;
            }

            // 如果 chosen 不为空，还要检查值是否匹配
            if (!string.IsNullOrEmpty(condition.chosen))
            {
                if (branchList[condition.index] != condition.chosen)
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// 执行匹配的 endBranch：切换到 DialogueState 并销毁当前 EndState
    /// </summary>
    private void ExecuteEndBranch(EndBranch endBranch)
    {
        StateEventDefine.ChangeState.SendEventMessage<DialogueState>("LogState", endBranch.Setting, true);
    }

    /// <summary>
    /// 返回主菜单：清除所有挂起状态，切换到 GameStart
    /// </summary>
    private void GoToMainMenu()
    {
        Machine.ClearSuspendedNodes();
        StateEventDefine.ChangeState.SendEventMessage<GameStart>("GameStart", null, true);
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

    public override void OnHandleEventMessage(IEventMessage message)
    {

    }
}
