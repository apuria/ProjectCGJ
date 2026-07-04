using System.Collections.Generic;
using UnityEngine;

public class ActionList : MonoBehaviour
{
    public List<ActionSlot> actions;

    /// <summary>
    /// 更新行动队列UI：跳过第一顺位（当前行动者），其余按顺序显示，空位隐藏
    /// </summary>
    public void UpdateUI(List<BaseInfo> actionQueue)
    {
        for (int i = 0; i < actions.Count; i++)
        {
            // actionQueue[0] 是当前行动者，从 index 1 开始显示
            int queueIndex = i + 1;
            if (actionQueue != null && queueIndex < actionQueue.Count)
            {
                actions[i].gameObject.SetActive(true);
                actions[i].UpdateUI(actionQueue[queueIndex].avatar);
            }
            else
            {
                actions[i].gameObject.SetActive(false);
            }
        }
    }
}
