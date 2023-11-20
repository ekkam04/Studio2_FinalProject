using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainState : BaseState<UIStateMachine.UIState>
{
    public UIMainState(UIStateMachine.UIState key) : base(key)
    {

    }

    public override void EnterState()
    {
        Debug.Log("UI Entered Main State");
    }

    public override void ExitState()
    {
        Debug.Log("UI Exited Main State");
    }

    public override UIStateMachine.UIState GetNextState()
    {
        return StateKey; 
    }

    public override void OnTriggerEnter(Collider other)
    {

    }

    public override void OnTriggerExit(Collider other)
    {

    }

    public override void OnTriggerStay(Collider other)
    {

    }

    public override void UpdateState()
    {

    }
}
