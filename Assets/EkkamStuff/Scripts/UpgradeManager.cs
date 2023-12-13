using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

namespace Ekkam {
    public class UpgradeManager : MonoBehaviour
    {
        public bool waitingForUpgrade = false;
        public GameObject upgradeMenu;
        UIStateMachine uiStateMachine;

        public List<Card> player1Cards;
        public List<Card> player2Cards;

        public GameObject upgradeIconPrefab;
        public GameObject player1Upgrades;
        public GameObject player2Upgrades;
        
        [SerializeField] ParticleSystem upgradeParticles;
        public Color dullingColor;

        public List<Upgrade> upgrades;

        public List<UpgradeData> player1UpgradeData;
        public List<UpgradeData> player2UpgradeData;

        void Start()
        {
            uiStateMachine = FindObjectOfType<UIStateMachine>();

            player1UpgradeData = new List<UpgradeData>();
            player2UpgradeData = new List<UpgradeData>();

            LoadUpgradesFromJson();
            ClearAllPlayerUpgradeSaveData();
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

        public void UpgradePlayer(Player player, Card card, Texture2D icon, Color iconColor)
        {
            bool saveUpgrade = upgrades.Find(x => x.upgradeName == card.upgradeName.text).saveUpgrade;
            GameObject newIcon = null;
            if (player.playerNumber == 1)
            {
                bool upgradeAlreadyExists = false;
                GameObject existingIcon = null;
                foreach (RawImage image in player1Upgrades.GetComponentsInChildren<RawImage>())
                {
                    if (image.texture == icon)
                    {
                        upgradeAlreadyExists = true;
                        existingIcon = image.gameObject;
                    }
                }

                if (!upgradeAlreadyExists)
                {
                    newIcon = SpawnUpgradeIcon(card.upgradeName.text, icon, iconColor);
                    newIcon.transform.SetParent(player1Upgrades.transform);

                    if (saveUpgrade) UpdatePlayerUpgradeData(player, card.upgradeName.text, 1);
                }
                else
                {
                    TMP_Text existingIconText = existingIcon.GetComponentInChildren<TMP_Text>();
                    int existingIconCount = int.Parse(existingIconText.text.Substring(0, existingIconText.text.Length - 1));
                    existingIconText.text = (existingIconCount + 1) + "x";

                    if (saveUpgrade) UpdatePlayerUpgradeData(player, card.upgradeName.text, (existingIconCount + 1));
                }

                foreach (Card playerCard in player1Cards)
                {
                    Destroy(playerCard.gameObject.transform.parent.gameObject);
                }
            }
            else
            {
                bool upgradeAlreadyExists = false;
                GameObject existingIcon = null;
                foreach (RawImage image in player2Upgrades.GetComponentsInChildren<RawImage>())
                {
                    if (image.texture == icon)
                    {
                        upgradeAlreadyExists = true;
                        existingIcon = image.gameObject;
                    }
                }

                if (!upgradeAlreadyExists)
                {
                    newIcon = SpawnUpgradeIcon(card.upgradeName.text, icon, iconColor);
                    newIcon.transform.SetParent(player2Upgrades.transform);

                    if (saveUpgrade) UpdatePlayerUpgradeData(player, card.upgradeName.text, 1);
                }
                else
                {
                    TMP_Text existingIconText = existingIcon.GetComponentInChildren<TMP_Text>();
                    int existingIconCount = int.Parse(existingIconText.text.Substring(0, existingIconText.text.Length - 1));
                    existingIconText.text = (existingIconCount + 1) + "x";

                    if (saveUpgrade) UpdatePlayerUpgradeData(player, card.upgradeName.text, (existingIconCount + 1));
                }

                foreach (Card playerCard in player2Cards)
                {
                    Destroy(playerCard.gameObject.transform.parent.gameObject);
                }

            }

            player.Upgrade(card.upgradeName.text);
            if (saveUpgrade) SaveUpgradesToJson();
            
            if (newIcon != null) newIcon.GetComponent<RectTransform>().localScale = new Vector3(2, 2, 2);

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

        public GameObject SpawnUpgradeIcon(string name, Texture2D icon, Color iconColor)
        {
            GameObject newIcon = Instantiate(upgradeIconPrefab);
            newIcon.name = name;
            newIcon.GetComponent<RawImage>().texture = icon;
            newIcon.GetComponent<RawImage>().color = iconColor;
            newIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            return newIcon;
        }

        // Save upgrades to JSON
        public void SaveUpgradesToJson()
        {
            SavePlayerUpgradesToJson(player1UpgradeData, "Player1Upgrades");
            SavePlayerUpgradesToJson(player2UpgradeData, "Player2Upgrades");
        }

        private void SavePlayerUpgradesToJson(List<UpgradeData> upgradeData, string fileName)
        {
            UpgradeDataList upgradeDataList = new UpgradeDataList
            {
                upgrades = upgradeData
            };

            string json = JsonUtility.ToJson(upgradeDataList);
            print("JSON save data: " + json);
            File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, fileName + ".json"), json);
        }

        // Load upgrades from JSON
        public void LoadUpgradesFromJson()
        {
            LoadPlayerUpgradesFromJson(player1UpgradeData, "Player1Upgrades");
            LoadPlayerUpgradesFromJson(player2UpgradeData, "Player2Upgrades");
        }

        private void LoadPlayerUpgradesFromJson(List<UpgradeData> upgradeData, string fileName)
        {
            print("Loading upgrades from JSON...");
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName + ".json");
            if (File.Exists(filePath))
            {
                print(fileName + " Save file exists");
                string json = File.ReadAllText(filePath);
                UpgradeDataList upgradeDataList = JsonUtility.FromJson<UpgradeDataList>(json);
                upgradeData = upgradeDataList.upgrades;
                if (fileName == "Player1Upgrades")
                {
                    player1UpgradeData = upgradeData;
                }
                else
                {
                    player2UpgradeData = upgradeData;
                }
                print("Loaded " + upgradeData.Count + " upgrades");
            }
        }

        private void UpdatePlayerUpgradeData(Player player, string upgradeName, int upgradeLevel)
        {
            List<UpgradeData> playerUpgradeData = player.playerNumber == 1 ? player1UpgradeData : player2UpgradeData;
            UpgradeData existingUpgrade = playerUpgradeData.Find(upgrade => upgrade.upgradeName == upgradeName);

            if (existingUpgrade != null)
            {
                existingUpgrade.upgradeLevel = upgradeLevel;
            }
            else
            {
                UpgradeData newUpgrade = new UpgradeData
                {
                    upgradeName = upgradeName,
                    upgradeLevel = upgradeLevel
                };

                playerUpgradeData.Add(newUpgrade);
            }
        }

        private void ClearAllPlayerUpgradeSaveData()
        {
            List <UpgradeData> emptyPlayer1UpgradeData = new List<UpgradeData>();
            List <UpgradeData> emptyPlayer2UpgradeData = new List<UpgradeData>();
            SavePlayerUpgradesToJson(emptyPlayer1UpgradeData, "Player1Upgrades");
            SavePlayerUpgradesToJson(emptyPlayer2UpgradeData, "Player2Upgrades");
        }
    }
}
