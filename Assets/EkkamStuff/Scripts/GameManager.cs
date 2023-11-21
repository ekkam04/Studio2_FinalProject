using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ekkam {
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public bool gamePaused = false;

        public delegate void PlayerEvent();
        public static event PlayerEvent OnCombinePlayers;
        public static event PlayerEvent OnSeperatePlayers;

        [SerializeField] GameObject playerDuoPrefab;

        // [SerializeField] GameObject gameVCam;
        // [SerializeField] GameObject combinedVCam;

        WaveSpawner waveSpawner;
        UpgradeManager upgradeManager;

        void Awake()
        {
            waveSpawner = FindObjectOfType<WaveSpawner>();
            upgradeManager = FindObjectOfType<UpgradeManager>();
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }   

        void Start()
        {
            Time.timeScale = 1;
            // gameVCam.SetActive(true);
            // combinedVCam.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (OnCombinePlayers != null) StartCoroutine(CombinePlayers());
            }   
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (OnSeperatePlayers != null) StartCoroutine(SeperatePlayers());
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                upgradeManager.ShowUpgrades();
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            // start game when there are 2 players
            if (PlayerInput.all.Count == 2 && !waveSpawner.spawningWaves)
            {
                waveSpawner.StartSpawningWaves();
            }
        }

        public void AddPlayer(PlayerInput playerInput)
        {
            Player player = playerInput.GetComponent<Player>();
            
            player.playerNumber = PlayerInput.all.Count;
            playerInput.gameObject.name = "Player_" + player.playerNumber;
            print("Player " + player.playerNumber + " joined");

            player.AssignMaterial();
            player.ApplySeparationForce();
        }

        IEnumerator CombinePlayers()
        {
            if (AllPlayersInDuoMode()) yield break;
            if (OnCombinePlayers != null)
            {
                OnCombinePlayers();
                // combinedVCam.SetActive(true);
                // gameVCam.SetActive(false);
                yield return new WaitUntil(() => AllPlayersInDuoMode());
                GameObject playerDuoGameObject = Instantiate(playerDuoPrefab, PlayerInput.all[0].transform.position, Quaternion.identity);
                playerDuoGameObject.name = "PlayerDuo";
                Player playerDuo = playerDuoGameObject.GetComponent<Player>();  

                playerDuo.health = 0f;
                playerDuo.maxHealth = 0f;
                foreach (PlayerInput playerInput in PlayerInput.all)
                {
                    Player player = playerInput.GetComponent<Player>();
                    player.playerDuo = playerDuo;
                    playerDuo.health += player.health;
                    playerDuo.maxHealth += player.maxHealth;
                }
                playerDuo.healthBar.maxValue = playerDuo.maxHealth;
                playerDuo.healthBar.value = playerDuo.health;
            }
        }

        IEnumerator SeperatePlayers()
        {
            if (OnSeperatePlayers != null)
            {
                OnSeperatePlayers();
                // gameVCam.SetActive(true);
                // combinedVCam.SetActive(false);
                yield return new WaitUntil(() => !AllPlayersInDuoMode());
                foreach (PlayerInput playerInput in PlayerInput.all)
                {
                    playerInput.GetComponent<Player>().playerDuo = null;
                }
                Destroy(GameObject.FindGameObjectWithTag("PlayerDuo"));
            }
        }

        bool AllPlayersInDuoMode()
        {
            foreach (PlayerInput playerInput in PlayerInput.all)
            {
                if (playerInput.GetComponent<Player>().inDuoMode == false) return false;
            }
            return true;
        }

        public void CombinePlayersButtonPressed()
        {
            if (OnCombinePlayers != null) StartCoroutine(CombinePlayers());
        }

        public void SeperatePlayersButtonPressed()
        {
            if (OnSeperatePlayers != null) StartCoroutine(SeperatePlayers());
        }

        public void QuitGameButtonPressed()
        {
            Application.Quit();
        }
    }
}
