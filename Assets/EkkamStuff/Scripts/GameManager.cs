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
        [SerializeField] ParticleSystem xpParticles;

        [SerializeField] AudioSource xpAudioSource;
        [SerializeField] AudioSource gameAudioSource;

        public float playersXP = 0f;
        public float playersTotalXP = 0f;
        public float playersXPToNextLevel = 20f;

        WaveSpawner waveSpawner;
        UpgradeManager upgradeManager;
        AudioManager audioManager;

        void Awake()
        {
            waveSpawner = FindObjectOfType<WaveSpawner>();
            upgradeManager = FindObjectOfType<UpgradeManager>();
            audioManager = FindObjectOfType<AudioManager>();
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

            // check if players have enough XP to level up
            if (playersXP >= playersXPToNextLevel)
            {
                playersXP = 0;
                playersXPToNextLevel *= 1.5f;
                upgradeManager.ShowUpgrades();
                audioManager.PlayLevelUpSound(gameAudioSource);
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
                yield return new WaitUntil(() => AllPlayersInDuoMode());
                GameObject playerDuoGameObject = Instantiate(playerDuoPrefab, PlayerInput.all[0].transform.position, Quaternion.identity);
                playerDuoGameObject.name = "PlayerDuo";
                Player playerDuo = playerDuoGameObject.GetComponent<Player>();  
                foreach (PlayerInput playerInput in PlayerInput.all)
                {
                    Player player = playerInput.GetComponent<Player>();
                    player.playerDuo = playerDuo;
                }
                playerDuo.CombineUpgrades();
            }
        }

        IEnumerator SeperatePlayers()
        {
            if (OnSeperatePlayers != null)
            {
                OnSeperatePlayers();
                yield return new WaitUntil(() => !AllPlayersInDuoMode());
                foreach (PlayerInput playerInput in PlayerInput.all)
                {
                    playerInput.GetComponent<Player>().playerDuo = null;
                }
                Destroy(GameObject.FindGameObjectWithTag("PlayerDuo"));
            }
        }

        public bool AllPlayersInDuoMode()
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

        public void SpawnXP(Vector3 position, int amount, DamagableEntity killer)
        {
            // print(killer.name + " killed an enemy and gained " + amount + " XP");
            ParticleSystem xpParticle = Instantiate(xpParticles, position, Quaternion.identity);

            // check if the killer is a player and if so, add the player's collider and forcefield to the particle system
            if (killer != null && killer.GetComponent<Player>() != null)
            {
                Player killerPlayer = killer.GetComponent<Player>();

                ParticleSystem.TriggerModule triggerModule = xpParticle.trigger;
                triggerModule.SetCollider(0, killerPlayer.GetComponent<Collider>());

                ParticleSystem.ExternalForcesModule externalForcesModule = xpParticle.externalForces;
                externalForcesModule.AddInfluence(killerPlayer.GetComponent<ParticleSystemForceField>());

                xpParticle.GetComponent<XPParticle>().xpAmount *= killerPlayer.xpMultiplier;
            }

            xpParticle.Emit(amount);
        }

        public void CollectXP(float amount)
        {
            playersXP += amount;
            playersTotalXP += amount;
            audioManager.PlayXPSound(xpAudioSource);
        }
    }
}
