using System;
using System.Collections;
using System.Collections.Generic;
using UniFramework.Event;
using UniFramework.Machine;
using UnityEngine;

public class LoadingState : BaseState
{
    private float remainingTime;

    private Type targetStateType;
    private string targetTag;
    private IStateData targetData;

    private bool loadCompleteSent = false;
    private bool switchPending = false;

    public override void OnCreate(StateMachine machine, IStateData data)
    {
        base.OnCreate(machine, data);
        var loadingInfo = data as LoadingInfo;
        if (loadingInfo != null)
        {
            targetStateType = loadingInfo.targetStateType;
            targetTag = loadingInfo.targetTag;
            targetData = loadingInfo.targetData;
        }
    }

    public override void OnEnter()
    {
        // 根据目标状态区分随机加载时间
        if (targetStateType == typeof(BattleState))
            remainingTime = UnityEngine.Random.Range(3, 4);
        else if (targetStateType == typeof(MapState))
            remainingTime = UnityEngine.Random.Range(1, 2);
        loadCompleteSent = false;
        switchPending = false;

        // 从 GameManager 获取加载文本并随机一条
        var texts = GameManager.Instance.loadingTexts;
        int index = UnityEngine.Random.Range(0, texts.texts.Count);
        string loadText = texts.texts[index].text;

        UIMgr.Instance.ShowPanel<LoadingPanel>(isSync: true);

        LoadingEventDefine.ShowLoadingText.SendEventMessage(loadText);

        eventGroup.AddListener<LoadingEventDefine.AnimationEnd>(OnHandleEventMessage);
    }

    public override void OnExit()
    {

    }

    public override void OnUpdate()
    {
        if (switchPending)
        {
            switchPending = false;
            Machine.SwitchTo(targetStateType, targetTag, targetData);
            return;
        }

        if (loadCompleteSent)
            return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            loadCompleteSent = true;
            LoadingEventDefine.LoadComplete.SendEventMessage();
        }
    }

    public override void OnDispose()
    {
        UIMgr.Instance.HidePanel<LoadingPanel>(true);
        base.OnDispose();
    }

    public override void OnHandleEventMessage(IEventMessage message)
    {
        if (message is LoadingEventDefine.AnimationEnd)
        {
            // 延迟到下一帧 OnUpdate 中再切换状态，
            // 避免在 LoadingPanel.Update() 中同步销毁自身导致 Editor Inspector 报错
            switchPending = true;
        }
    }
}
