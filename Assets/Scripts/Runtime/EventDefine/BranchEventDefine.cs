using System.Collections;
using System.Collections.Generic;
using UniFramework.Event;
using UnityEngine;

public class BranchEventDefine
{
    /// <summary>
    /// 显示/更新分支选项UI
    /// </summary>
    public class ShowUI : IEventMessage
    {
        public List<Branch> branches;
        public static void SendEventMessage(List<Branch> branches)
        {
            var msg = new ShowUI();
            msg.branches = branches;
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 玩家选择了某个分支
    /// </summary>
    public class ChooseBranch : IEventMessage
    {
        public int branchId;
        public static void SendEventMessage(int branchId)
        {
            var msg = new ChooseBranch();
            msg.branchId = branchId;
            UniEvent.SendMessage(msg);
        }
    }
}
