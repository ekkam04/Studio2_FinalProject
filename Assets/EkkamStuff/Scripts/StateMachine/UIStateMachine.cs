using System.Collections;
using System.Collections.Generic;
using Ekkam;
using TMPro;
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
    [SerializeField] TMP_Text player1Kills;
    [SerializeField] TMP_Text player2Kills;

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

    public void UpdatePlayerKillCount(Player killer)
    {
        if (killer.playerNumber == 1)
        {
            player1Kills.text = killer.killCount.ToString();
        }
        else if (killer.playerNumber == 2)
        {
            player2Kills.text = killer.killCount.ToString();
        }
    }
}
