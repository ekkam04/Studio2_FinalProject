using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class UIManager : MonoBehaviour
    {
        public GameObject mainCanvas;
        [SerializeField] RectTransform upgradeMenu;

        public void OpenUpgradeMenu()
        {
            GetComponent<UIStateMachine>().OpenUpgradeMenu();
            upgradeMenu.anchoredPosition = new Vector2(upgradeMenu.anchoredPosition.x, -Screen.height);
            upgradeMenu.gameObject.SetActive(true);
            LeanTween.moveY(upgradeMenu, 0, 0.5f).setEaseOutCubic().setIgnoreTimeScale(true);
        }

        public void CloseUpgradeMenu()
        {
            GetComponent<UIStateMachine>().CloseUpgradeMenu();
            upgradeMenu.gameObject.SetActive(false);
        }  
    }
}