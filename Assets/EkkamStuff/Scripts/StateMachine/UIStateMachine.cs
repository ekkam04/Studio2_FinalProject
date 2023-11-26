using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Awake()
    {
        States.Add(UIState.Main, new UIMainState(UIState.Main));
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
