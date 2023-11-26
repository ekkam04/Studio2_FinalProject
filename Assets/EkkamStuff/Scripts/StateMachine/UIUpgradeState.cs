using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIUpgradeState : BaseState<UIStateMachine.UIState>
{
    private RectTransform upgradeMenu;

    public UIUpgradeState(UIStateMachine.UIState key, RectTransform upgradeMenu) : base(key)
    {
        this.upgradeMenu = upgradeMenu;
    }

    public override void EnterState()
    {
        Debug.Log("UI Entered Upgrade State");
        upgradeMenu.anchoredPosition = new Vector2(upgradeMenu.anchoredPosition.x, -Screen.height);
        upgradeMenu.gameObject.SetActive(true);
        LeanTween.moveY(upgradeMenu, 0, 0.5f).setEaseOutCubic().setIgnoreTimeScale(true);
    }

    public override void ExitState()
    {
        Debug.Log("UI Exited Upgrade State");
        upgradeMenu.gameObject.SetActive(false);
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
