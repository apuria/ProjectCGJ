using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
/*
1. 玩家所在游戏节点
2. 记录玩家关键选择
3. 可能还要记录当前玩家的状态
*/
    /// <summary>
    /// 玩家所在游戏节点
    /// 完成当前节点后，才会进入下一个节点
    /// </summary>
    public enum Flow
    {
        Start = -1,
        Node1,
        Node2,
        Node3,
        Node4,
        Node5,
        Node6,
        End
    }

    public Flow NowFlow = Flow.Start;

    public void NextFlow()
    {
        NowFlow++;
    }

    public Dictionary<string, string> branchList = new();
}
