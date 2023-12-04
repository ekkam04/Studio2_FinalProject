using System.Collections;
using System.Collections.Generic;
using Ekkam;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerSelectState : BaseState<UIStateMachine.UIState>
{

    private RectTransform joinMenu;

    public UIPlayerSelectState(UIStateMachine.UIState key, RectTransform joinMenu) : base(key)
    {
        this.joinMenu = joinMenu;
    }

    public override void EnterState()
    {
        Debug.Log("UI Entered PlayerSelect State");
        joinMenu.gameObject.SetActive(true);
    }

    public override void ExitState()
    {
        Debug.Log("UI Exited PlayerSelect State");
        joinMenu.gameObject.SetActive(false);
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
