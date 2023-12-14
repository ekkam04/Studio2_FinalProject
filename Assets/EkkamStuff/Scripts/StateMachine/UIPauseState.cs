using System.Collections;
using System.Collections.Generic;
using Ekkam;
using UnityEngine;
using UnityEngine.UI;

public class UIPauseState : BaseState<UIStateMachine.UIState>
{
    private RectTransform pauseMenu;
    private Button resumeButton;

    public UIPauseState(UIStateMachine.UIState key, RectTransform pauseMenu, Button resumeButton) : base(key)
    {
        this.pauseMenu = pauseMenu;
        this.resumeButton = resumeButton;
    }

    public override void EnterState()
    {
        Debug.Log("UI Entered Pause State");
        pauseMenu.gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public override void ExitState()
    {
        Debug.Log("UI Exited Pause State");
        pauseMenu.gameObject.SetActive(false);
        Time.timeScale = 1f;
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
