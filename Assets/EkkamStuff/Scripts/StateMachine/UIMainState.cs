using System.Collections;
using System.Collections.Generic;
using Ekkam;
using UnityEngine;
using UnityEngine.UI;

public class UIMainState : BaseState<UIStateMachine.UIState>
{
    private Slider xpSlider;
    GameManager gameManager;

    public UIMainState(UIStateMachine.UIState key, Slider xpSlider) : base(key)
    {
        this.xpSlider = xpSlider;
        gameManager = GameManager.instance;
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
        xpSlider.value = gameManager.playersXP;
        xpSlider.maxValue = gameManager.playersXPToNextLevel;
    }
}
