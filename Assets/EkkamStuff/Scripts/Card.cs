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

        UpgradeManager upgradeManager;

        void Awake()
        {
            upgradeButton = GetComponent<Button>();
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
    }
}
