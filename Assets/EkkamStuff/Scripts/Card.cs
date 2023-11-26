using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace Ekkam {
    public class Card : MonoBehaviour
    {
        public Player ownerPlayer;

        public TMP_Text upgradeName;
        public TMP_Text upgradeDescription;
        public Button upgradeButton;

        public Image[] CardBGs;
        public Image CardBorder;

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
    }
}
