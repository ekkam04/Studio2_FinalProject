using System.Collections;
using System.Collections.Generic;
using Ekkam;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplayState : BaseState<UIStateMachine.UIState>
{

    private Slider xpSlider;
    private TMP_Text waveNumber;
    private RectTransform gameplayUI;
    GameManager gameManager;

    bool tweening = false;

    public UIGameplayState(UIStateMachine.UIState key, Slider xpSlider, TMP_Text waveNumber, RectTransform gameplayUI) : base(key)
    {
        this.xpSlider = xpSlider;
        this.waveNumber = waveNumber;
        gameManager = GameManager.instance;
        this.gameplayUI = gameplayUI;
    }

    public override void EnterState()
    {
        Debug.Log("UI Entered Gameplay State");
        tweening = true;

        LeanTween.scale(gameplayUI, new Vector3(1.45f, 1.45f, 1.45f), 0f).setEaseOutCubic().setIgnoreTimeScale(true).setOnComplete(() => {
            gameplayUI.gameObject.SetActive(true);
            LeanTween.scale(gameplayUI, new Vector3(0.94f, 0.94f, 0.94f), 0.5f).setEaseOutCubic().setIgnoreTimeScale(true).setOnComplete(() => {
                LeanTween.scale(gameplayUI, new Vector3(1f, 1f, 1f), 0.25f).setEaseOutCubic().setIgnoreTimeScale(true);
            });
        });
    }

    public override void ExitState()
    {
        Debug.Log("UI Exited Gameplay State");
        tweening = false;

        LeanTween.scale(gameplayUI, new Vector3(1.45f, 1.45f, 1.45f), 1f).setEaseOutCubic().setIgnoreTimeScale(true).setOnComplete(() => {
            if (!tweening) gameplayUI.gameObject.SetActive(false);
            LeanTween.scale(gameplayUI, new Vector3(1f, 1f, 1f), 0f).setEaseOutCubic().setIgnoreTimeScale(true);
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
        xpSlider.value = gameManager.playersXP;
        xpSlider.maxValue = gameManager.playersXPToNextLevel;

        waveNumber.text = "Wave: " + gameManager.waveNumber;
    }
}
