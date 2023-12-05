using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Ekkam {
    public class Card : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public Player ownerPlayer;

        public TMP_Text upgradeName;
        public TMP_Text upgradeDescription;
        public Button upgradeButton;

        public Image[] CardBGs;
        public Image CardBorder;

        Color bgColor;
        Color borderColor;

        UpgradeManager upgradeManager;

        void Awake()
        {
            upgradeManager = FindObjectOfType<UpgradeManager>();
        }

        public void OnCardSelected(TMP_Text waitingText)
        {
            print("Player " + ownerPlayer.playerNumber + " selected " + upgradeName.text);
            ownerPlayer.cardPicked = true;
            
            if (ownerPlayer.playerNumber == 1)
            {
                waitingText.text = "Waiting for Player 2 to select an upgrade";
            }
            else
            {
                waitingText.text = "Waiting for Player 1 to select an upgrade";
            }
            waitingText.gameObject.SetActive(true);
            
            upgradeManager.UpgradePlayer(ownerPlayer, this);
        }

        public void SetCardColors(Color bgColor, Color borderColor)
        {
            this.bgColor = bgColor;
            this.borderColor = borderColor;
            foreach (Image bg in CardBGs)
            {
                bg.color = bgColor;
            }
            CardBorder.color = borderColor;
            // set upgradebutton's normal color to the border color
            ColorBlock cb = upgradeButton.colors;
            cb.normalColor = borderColor;
            upgradeButton.colors = cb;
        }

        void dullCardColors()
        {
            Color dullBgColor = bgColor * 0.5f;
            Color dullBorderColor = borderColor * 0.5f;
            foreach (Image bg in CardBGs)
            {
                bg.color = dullBgColor;
            }
            CardBorder.color = dullBorderColor;
            // set upgradebutton's normal color to the border color
            ColorBlock cb = upgradeButton.colors;
            cb.normalColor = dullBorderColor;
            upgradeButton.colors = cb;
        }

        void brightenCardColors()
        {
            foreach (Image bg in CardBGs)
            {
                bg.color = bgColor;
            }
            CardBorder.color = borderColor;
            // set upgradebutton's normal color to the border color
            ColorBlock cb = upgradeButton.colors;
            cb.normalColor = borderColor;
            upgradeButton.colors = cb;
        }

        public void OnSelect(BaseEventData eventData)
        {
            print("Selected " + upgradeName.text);
            brightenCardColors();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            print("Deselected " + upgradeName.text);
            dullCardColors();
        }
    }
}
