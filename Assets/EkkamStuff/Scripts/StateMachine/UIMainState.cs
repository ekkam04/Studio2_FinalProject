using System.Collections;
using System.Collections.Generic;
using Ekkam;
using UnityEngine;
using UnityEngine.UI;

public class UIMainState : BaseState<UIStateMachine.UIState>
{

    private RectTransform mainMenu;
    private Button startButton;

    public UIMainState(UIStateMachine.UIState key, RectTransform mainMenu, Button startButton) : base(key)
    {
        this.mainMenu = mainMenu; 
        this.startButton = startButton;
    }

    public override void EnterState()
    {
        Debug.Log("UI Entered Main State");
        mainMenu.gameObject.SetActive(true);

        startButton.Select();
    }

    public override void ExitState()
    {
        Debug.Log("UI Exited Main State");
        mainMenu.gameObject.SetActive(false);
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
