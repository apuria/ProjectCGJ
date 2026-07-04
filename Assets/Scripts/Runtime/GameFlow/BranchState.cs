using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Event;

public class BranchState : BaseState
{

//分支系统
/*
先初始化UI界面
随后由UI发送消息, 让这个状态判断玩家的选择
然后根据配置, 让状态机继续运行
*/

    public BranchSetting branchSetting;
    
    public override void OnCreate(StateMachine machine, IStateData data)
    {
        base.OnCreate(machine, data);
        branchSetting = data as BranchSetting;
        UIMgr.Instance.ShowPanel<BranchPanel>(isSync: true);
    }

    public override void OnEnter()
    {
        // 向 BranchPanel 发送分支列表
        BranchEventDefine.ShowUI.SendEventMessage(branchSetting.branches);

        eventGroup.AddListener<BranchEventDefine.ChooseBranch>(OnHandleEventMessage);
    }

    public override void OnExit()
    {
        
    }

    public override void OnUpdate()
    {

    }

    public override void OnDispose()
    {
        UIMgr.Instance.HidePanel<BranchPanel>(true);
        base.OnDispose();
    }

    public override void OnHandleEventMessage(IEventMessage message)
    {
        if(message is BranchEventDefine.ChooseBranch chooseBranch)
        {
            // 边界检查
            if (chooseBranch.branchId < 0 || chooseBranch.branchId >= branchSetting.branches.Count) return;
            // 记录玩家的分支选择
            GameManager.Instance.AddBranchChoose(branchSetting.id, branchSetting.branches[chooseBranch.branchId].chosen);
            // 继续后续流程
            Continue();
        }
    }

    public void Continue()
    {
        // 先判断分支选择结束后应该做什么
        switch(branchSetting.onEnd)
        {
            case EOnEnd.NextDialogue:
                // 进入下一个对话
                NextDialogue();
                break;
            case EOnEnd.StartBattle:
                // 开启战斗
                StartBattle();
                break;
            case EOnEnd.GoBackToLastState:
                // 回到上一个状态
                GoBackToLastState();
                break;
            case EOnEnd.StartBranch:
                // 开始新的分支选项
                StartBranch();
                break;
            case EOnEnd.GoBackToMap:
                // 回到地图
                GoBackToMap();
                break;
            case EOnEnd.EndBranch:
                // 结束分支，回到地图界面，玩家数据进入下一个节点
                EndBranch();
                break;
            case EOnEnd.End:
                // 进入 EndState，根据 EndSetting 中的分支条件决定后续流程
                StartEnd();
                break;
        }
    }

    private void NextDialogue()
    {
        // 使用状态机切换到对话状态
        StateEventDefine.ChangeState.SendEventMessage<DialogueState>("LogState", branchSetting.nextDialogue);
    }

    private void StartBattle()
    {
        // 使用状态机切换到战斗状态
        StateEventDefine.ChangeState.SendEventMessage<BattleState>("BattleState", branchSetting.battleSetting);
    }

    private void GoBackToLastState()
    {
        // 使用状态机切换到上一个状态
        StateEventDefine.BackToPrevState.SendEventMessage();
    }

    private void StartBranch()
    {
        // 使用状态机切换到分支选项状态
        StateEventDefine.ChangeState.SendEventMessage<BranchState>("BranchState", branchSetting.nextBranch);
    }

    private void GoBackToMap()
    {
        // 使用状态机切换到地图状态
        StateEventDefine.ChangeState.SendEventMessage<MapState>("MapState");
    }

    private void EndBranch()
    {
        // 结束当前一段剧情，玩家数据进入下一个节点，然后回到地图界面
        // 调用UI显示剧情结束过渡效果
        GameManager.Instance.NextNode();
        GoBackToMap();
    }

    private void StartEnd()
    {
        // 切换到 EndState，销毁当前 BranchState
        StateEventDefine.ChangeState.SendEventMessage<EndState>("EndState", branchSetting.endSetting, true);
    }
}
