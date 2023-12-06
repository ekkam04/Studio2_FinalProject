using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStateMachine : StateManager<UIStateMachine.UIState>
{
    public enum UIState
    {
        Main,
        Gameplay,
        PlayerSelect,
        Pause,
        Upgrade,
        Settings,
        Synergy,
        GameOver
    }

    public GameObject mainCanvas;
    [SerializeField] Button startButton;
    [SerializeField] RectTransform upgradeMenu;
    [SerializeField] RectTransform mainMenu;
    [SerializeField] RectTransform joinMenu;
    [SerializeField] RectTransform gameplayUI;
    [SerializeField] Slider xpSlider;

    void Awake()
    {
        States.Add(UIState.Main, new UIMainState(UIState.Main, mainMenu, startButton));
        States.Add(UIState.Gameplay, new UIGameplayState(UIState.Gameplay, xpSlider, gameplayUI));
        States.Add(UIState.PlayerSelect, new UIPlayerSelectState(UIState.PlayerSelect, joinMenu));
        States.Add(UIState.Upgrade, new UIUpgradeState(UIState.Upgrade, upgradeMenu));
        CurrentState = States[UIState.Main];
    }

    public void OpenUpgradeMenu()
    {
        TransitionToState(UIState.Upgrade);
    }

    public void CloseUpgradeMenu()
    {
        TransitionToState(UIState.Gameplay);
    }

    public void OpenMainMenu()
    {
        TransitionToState(UIState.Main);
    }

    public void CloseMainMenu()
    {
        TransitionToState(UIState.PlayerSelect);
    }

    public void ShowGameplayUI()
    {
        TransitionToState(UIState.Gameplay);
    }
}
