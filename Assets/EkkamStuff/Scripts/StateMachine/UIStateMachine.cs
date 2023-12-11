using System.Collections;
using System.Collections.Generic;
using Ekkam;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

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
    [SerializeField] RectTransform pauseMenu;
    [SerializeField] Button resumeButton;
    [SerializeField] Slider xpSlider;
    [SerializeField] TMP_Text player1Kills;
    [SerializeField] TMP_Text player2Kills;

    void Awake()
    {
        States.Add(UIState.Main, new UIMainState(UIState.Main, mainMenu, startButton));
        States.Add(UIState.Gameplay, new UIGameplayState(UIState.Gameplay, xpSlider, gameplayUI));
        States.Add(UIState.PlayerSelect, new UIPlayerSelectState(UIState.PlayerSelect, joinMenu));
        States.Add(UIState.Pause, new UIPauseState(UIState.Pause, pauseMenu, resumeButton));
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

    public void TogglePauseMenu(int playerNumber = 0)
    {
        print("Player " + playerNumber + " paused the game");
        if (CurrentState.StateKey == UIState.Pause)
        {
            TransitionToState(UIState.Gameplay);
            foreach (PlayerInput playerInput in PlayerInput.all)
            {
                Debug.Log("Clearing Pause Menu Controls");
                playerInput.GetComponent<Player>().AssignMenuControls(null, null, playerNumber);
            }
        }
        else if (CurrentState.StateKey == UIState.Gameplay)
        {
            TransitionToState(UIState.Pause);
            foreach (PlayerInput playerInput in PlayerInput.all)
            {
                Debug.Log("Assigning Pause Menu Controls");
                playerInput.GetComponent<Player>().AssignMenuControls(resumeButton, pauseMenu.gameObject, playerNumber);
            }
        }
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
