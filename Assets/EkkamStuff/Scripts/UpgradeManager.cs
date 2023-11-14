using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class UpgradeManager : MonoBehaviour
    {
        public bool waitingForUpgrade = false;
        public GameObject upgradeMenu;
        public List<Card> player1Cards;
        public List<Card> player2Cards;

        [HideInInspector]
        public List<string> upgradeNames;

        [HideInInspector]
        public List<string> upgradeDescriptions;

        void Start()
        {
            HideUpgrades();
        }

        // Assign non repeating random upgrades to cards
        void AssignRandomUpgrades(List<Card> cards)
        {
            List<int> randomNumbers = new List<int>();
            for (int i = 0; i < cards.Count; i++)
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
            if (player1Cards.Count == 0 || player2Cards.Count == 0)
            {
                foreach (Player p in FindObjectsOfType<Player>())
                {
                    p.AddCardsToUpgradeMenu();
                }
            }

            foreach (Player p in FindObjectsOfType<Player>())
            {
                p.allowMovement = false;
            }

            waitingForUpgrade = true;
            Time.timeScale = 0;
            AssignRandomUpgrades(player1Cards);
            AssignRandomUpgrades(player2Cards);
            upgradeMenu.SetActive(true);
        }

        public void HideUpgrades()
        {
            foreach (Player player in FindObjectsOfType<Player>())
            {
                player.allowMovement = true;
                player.cardPicked = false;
            }

            foreach (Transform playerCards in upgradeMenu.transform)
            {
                Destroy(playerCards.gameObject);
            }

            player1Cards.Clear();
            player2Cards.Clear();

            upgradeMenu.SetActive(false);
            waitingForUpgrade = false;
            Time.timeScale = 1;
        }

        public void UpgradePlayer(Player player, Card card)
        {
            if (player.playerNumber == 1)
            {
                foreach (Card playerCard in player1Cards)
                {
                    Destroy(playerCard.gameObject);
                }
            }
            else
            {
                foreach (Card playerCard in player2Cards)
                {
                    Destroy(playerCard.gameObject);
                }
            }
            player.Upgrade(card.upgradeName.text);

            bool allPlayersPickedUpgrades = true;
            foreach (Player p in FindObjectsOfType<Player>())
            {
                if (!p.cardPicked)
                {
                    allPlayersPickedUpgrades = false;
                }
            }
            if (allPlayersPickedUpgrades)
            {
                HideUpgrades();
            }
        }
    }
}