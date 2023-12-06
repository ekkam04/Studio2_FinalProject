using System.Collections;
using System.Collections.Generic;
using Ekkam;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplayState : BaseState<UIStateMachine.UIState>
{

    private Slider xpSlider;
    private RectTransform gameplayUI;
    GameManager gameManager;

    public UIGameplayState(UIStateMachine.UIState key, Slider xpSlider, RectTransform gameplayUI) : base(key)
    {
        this.xpSlider = xpSlider;
        gameManager = GameManager.instance;
        this.gameplayUI = gameplayUI;
    }

    public override void EnterState()
    {
        Debug.Log("UI Entered Gameplay State");
        xpSlider.gameObject.SetActive(true);
        gameplayUI.gameObject.SetActive(true);
    }

    public override void ExitState()
    {
        Debug.Log("UI Exited Gameplay State");
        xpSlider.gameObject.SetActive(false);
        gameplayUI.gameObject.SetActive(false);
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
        xpSlider.value = gameManager.playersXP;
        xpSlider.maxValue = gameManager.playersXPToNextLevel;
    }
}
