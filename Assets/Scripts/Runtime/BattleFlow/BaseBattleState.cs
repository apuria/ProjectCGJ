using System.Collections;
using System.Collections.Generic;
using UniFramework.Event;
using UniFramework.Machine;
using UnityEngine;

public class BaseBattleState : IStateNode
{
    protected StateMachine Machine { get; private set; }

    protected EventGroup eventGroup { get; private set; }

    public bool IsStarted = false;

    public virtual void OnCreate(StateMachine machine, IStateData data)
    {
        Machine = machine;
        eventGroup = new EventGroup();
    }

    public virtual void OnDispose()
    {
        eventGroup.RemoveAllListener();
    }

    public virtual void OnEnter()
    {
       
    }

    public virtual void OnExit()
    {
        
    }

    public virtual void OnUpdate()
    {
        
    }
}
