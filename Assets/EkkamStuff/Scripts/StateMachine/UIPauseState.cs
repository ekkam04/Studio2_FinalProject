using System.Collections;
using System.Collections.Generic;
using Ekkam;
using UnityEngine;
using UnityEngine.UI;

public class UIPauseState : BaseState<UIStateMachine.UIState>
{
    private RectTransform pauseMenu;
    private Button resumeButton;

    bool tweening = false;

    public UIPauseState(UIStateMachine.UIState key, RectTransform pauseMenu, Button resumeButton) : base(key)
    {
        this.pauseMenu = pauseMenu;
        this.resumeButton = resumeButton;
    }

    public override void EnterState()
    {
        Debug.Log("UI Entered Pause State");
        tweening = true;
        
        Time.timeScale = 0f;
        LeanTween.moveY(pauseMenu, -1200f, 0f).setEaseOutCubic().setIgnoreTimeScale(true).setOnComplete(() => {
            pauseMenu.gameObject.SetActive(true);
            LeanTween.moveY(pauseMenu, 0f, 0.75f).setEaseOutCubic().setIgnoreTimeScale(true);
        });
    }

    public override void ExitState()
    {
        Debug.Log("UI Exited Pause State");
        tweening = false;

        Time.timeScale = 1f;
        LeanTween.moveY(pauseMenu, -1200f, 0.5f).setEaseOutCubic().setIgnoreTimeScale(true).setOnComplete(() => {
            if (!tweening) pauseMenu.gameObject.SetActive(false);
        });
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
