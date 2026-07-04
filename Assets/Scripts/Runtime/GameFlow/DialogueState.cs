using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UnityEngine.XR;
using UniFramework.Event;
public class DialogueState : BaseState
{

    //进入对话状态，播放对话
    public DialogueSetting nowDialogue;

    public bool canGoNext = false;

    public int index = 0;

    public override void OnCreate(StateMachine machine, IStateData data)
    {
        base.OnCreate(machine, data);
        nowDialogue = data as DialogueSetting;
        UIMgr.Instance.ShowPanel<DialoguePanel>(isSync: true);
    }

    public override void OnEnter()
    {
        // 重置对话索引
        index = 0;
        // 显示对话面板
        DiaLogueEventDefine.ShowUI.SendEventMessage(nowDialogue.speakers, nowDialogue.hasBackground, nowDialogue.BackGround, nowDialogue.hasReturnButton);
        // 更新对话面板
        var dialogue = nowDialogue.dialogues[index];
        DiaLogueEventDefine.UpdateUI.SendEventMessage(dialogue.leftSpeakerIndex, dialogue.rightSpeakerIndex, dialogue.text, dialogue.talkingSpeaker,
            dialogue.dialogueType, dialogue.cgImage, dialogue.showDialogueBox);

        eventGroup.AddListener<DiaLogueEventDefine.Next>(OnHandleEventMessage);
    }

    public override void OnExit()
    {
        
    }

    public override void OnUpdate()
    {

    }

    public override void OnDispose()
    {
        UIMgr.Instance.HidePanel<DialoguePanel>(true);
        base.OnDispose();
    }

    public void Continue()
    {
        //先判断这个对话的下一个操作是什么，然后进行相应的操作
        switch(nowDialogue.dialogues[index].onEnd)
        {
            case EOnEnd.NextDialogue:
                // 播放下一条对话
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
                // 开始分支选项
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
            case EOnEnd.GoBackToMenu:
                // 回到主菜单，清空所有挂起状态，经过加载界面
                GoBackToMenu();
                break;
        }
    }

    private void NextDialogue()
    {
        //如果是最后一个对话，则回到上一个状态
        if(index == nowDialogue.dialogues.Count - 1)
        {
            GoBackToLastState();
        }
        else
        {
            index++;
            // 更新对话面板
            var dialogue = nowDialogue.dialogues[index];
            DiaLogueEventDefine.UpdateUI.SendEventMessage(dialogue.leftSpeakerIndex, dialogue.rightSpeakerIndex, dialogue.text, dialogue.talkingSpeaker,
                dialogue.dialogueType, dialogue.cgImage, dialogue.showDialogueBox);
        }
    }

    private void StartBattle()
    {
        //使用状态机切换到战斗状态, 而且要使用删除当前状态的切换状态
        StateEventDefine.ChangeState.SendEventMessage<BattleState>("BattleState", nowDialogue.dialogues[index].Battle);
    }

    private void GoBackToLastState()
    {
        //使用状态机切换到上一个状态
        StateEventDefine.BackToPrevState.SendEventMessage();
    }

    private void StartBranch()
    {
        //使用状态机切换到分支选项状态
        StateEventDefine.ChangeState.SendEventMessage<BranchState>("BranchState", nowDialogue.dialogues[index].Branch, false);
    }

    private void GoBackToMap()
    {
        //使用状态机切换到地图状态
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
        // 切换到 EndState，销毁当前 DialogueState
        StateEventDefine.ChangeState.SendEventMessage<EndState>("EndState", nowDialogue.dialogues[index].End, true);
    }

    private void GoBackToMenu()
    {
        // 清空所有挂起状态，经过加载界面回到主菜单
        Machine.ClearSuspendedNodes();
        StateEventDefine.ChangeState.SendEventMessage<GameStart>("GameStart");
    }

    public override void OnHandleEventMessage(IEventMessage message)
    {
        if(message is DiaLogueEventDefine.Next)
        {
            Continue();
        }
    }
}
