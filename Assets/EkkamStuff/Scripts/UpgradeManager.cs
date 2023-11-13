using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class UpgradeManager : MonoBehaviour
    {
        public bool waitingForUpgrade = false;
        [SerializeField] GameObject player1UpgradePanel;
        [SerializeField] Card[] player1Cards;
        [SerializeField] GameObject player2UpgradePanel;
        [SerializeField] Card[] player2Cards;

        [HideInInspector]
        public List<string> upgradeNames;

        [HideInInspector]
        public List<string> upgradeDescriptions;

        void Start()
        {
            HideUpgrades();
        }

        // Assign non repeating random upgrades to cards
        void AssignRandomUpgrades(Card[] cards)
        {
            List<int> randomNumbers = new List<int>();
            for (int i = 0; i < cards.Length; i++)
            {
                int randomNumber = Random.Range(0, upgradeNames.Count);
                while (randomNumbers.Contains(randomNumber))
                {
                    randomNumber = Random.Range(0, upgradeNames.Count);
                }
                randomNumbers.Add(randomNumber);
                cards[i].upgradeName.text = upgradeNames[randomNumber];
                cards[i].upgradeDescription.text = upgradeDescriptions[randomNumber];
            }
        }

        public void ShowUpgrades()
        {
            waitingForUpgrade = true;
            Time.timeScale = 0;
            AssignRandomUpgrades(player1Cards);
            AssignRandomUpgrades(player2Cards);
            player1UpgradePanel.SetActive(true);
            player2UpgradePanel.SetActive(true);
        }

        public void HideUpgrades()
        {
            player1UpgradePanel.SetActive(false);
            player2UpgradePanel.SetActive(false);
            waitingForUpgrade = false;
            Time.timeScale = 1;
        }
    }
}
