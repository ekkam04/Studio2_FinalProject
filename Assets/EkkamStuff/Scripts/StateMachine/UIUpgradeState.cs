using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIUpgradeState : BaseState<UIStateMachine.UIState>
{
    public UIUpgradeState(UIStateMachine.UIState key) : base(key)
    {

    }

    public override void EnterState()
    {
        Debug.Log("UI Entered Upgrade State");
    }

    public override void ExitState()
    {
        Debug.Log("UI Exited Upgrade State");
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
