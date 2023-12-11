using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace Ekkam {
    public class UpgradeManager : MonoBehaviour
    {
        public bool waitingForUpgrade = false;
        public GameObject upgradeMenu;
        UIStateMachine uiStateMachine;

        public List<Card> player1Cards;
        public List<Card> player2Cards;

        public GameObject player1Upgrades;
        public GameObject player2Upgrades;
        
        [SerializeField] ParticleSystem upgradeParticles;
        public Color dullingColor;

        public List<Upgrade> upgrades;

        void Start()
        {
            uiStateMachine = FindObjectOfType<UIStateMachine>();
        }

        // Assign non repeating random upgrades to cards
        void AssignRandomUpgrades(List<Card> cards)
        {
            List<int> randomNumbers = new List<int>();
            for (int i = 0; i < cards.Count; i++)
            {
                int randomNumber = Random.Range(0, upgrades.Count);
                while (randomNumbers.Contains(randomNumber))
                {
                    randomNumber = Random.Range(0, upgrades.Count);
                }
                randomNumbers.Add(randomNumber);
                cards[i].upgradeName.text = upgrades[randomNumber].upgradeName;
                cards[i].upgradeDescription.text = upgrades[randomNumber].upgradeDescription;
                cards[i].SetCardColors(upgrades[randomNumber].upgradeBGColor, upgrades[randomNumber].upgradeBorderColor);
                cards[i].SetUpgradeIcon(upgrades[randomNumber].upgradeIcon);
            }
        }

        async public void ShowUpgrades()
        {
            upgradeParticles.Play();
            upgradeMenu.SetActive(false);
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
            Time.timeScale = 0.5f;
            await Task.Delay(1000);
            Time.timeScale = 0;
            AssignRandomUpgrades(player1Cards);
            AssignRandomUpgrades(player2Cards);
            uiStateMachine.OpenUpgradeMenu();
        }

        async public void HideUpgrades()
        {
            upgradeParticles.Stop();
            uiStateMachine.CloseUpgradeMenu();
            await Task.Delay(500);
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

            waitingForUpgrade = false;
            Time.timeScale = 1;
        }

        public void UpgradePlayer(Player player, Card card, RawImage icon, Color iconColor)
        {
            GameObject newIcon = new GameObject();
            newIcon.AddComponent<RawImage>();
            newIcon.GetComponent<RawImage>().texture = icon.texture;
            newIcon.GetComponent<RawImage>().color = iconColor;
            newIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            if (player.playerNumber == 1)
            {
                foreach (Card playerCard in player1Cards)
                {
                    newIcon.transform.SetParent(player1Upgrades.transform);
                    Destroy(playerCard.gameObject.transform.parent.gameObject);
                }
            }
            else
            {
                foreach (Card playerCard in player2Cards)
                {
                    newIcon.transform.SetParent(player2Upgrades.transform);
                    Destroy(playerCard.gameObject.transform.parent.gameObject);
                }
            }
            
            newIcon.GetComponent<RectTransform>().localScale = new Vector3(2, 2, 2);
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
