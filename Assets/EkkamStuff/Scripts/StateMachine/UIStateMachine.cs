using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStateMachine : StateManager<UIStateMachine.UIState>
{
    public enum UIState
    {
        Main,
        Pause,
        Upgrade,
        Settings,
        Synergy,
        GameOver
    }

    public GameObject mainCanvas;
    [SerializeField] RectTransform upgradeMenu;
    [SerializeField] Slider xpSlider;

    void Awake()
    {
        States.Add(UIState.Main, new UIMainState(UIState.Main, xpSlider));
        States.Add(UIState.Upgrade, new UIUpgradeState(UIState.Upgrade, upgradeMenu));
        CurrentState = States[UIState.Main];
    }

    public void OpenUpgradeMenu()
    {
        TransitionToState(UIState.Upgrade);
    }

    public void CloseUpgradeMenu()
    {
        TransitionToState(UIState.Main);
    }
}
