using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Ekkam {
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public bool gamePaused = false;

        public bool skipIntro = false;

        [SerializeField] GameObject playerPrefab;

        public delegate void PlayerEvent();
        public static event PlayerEvent OnCombinePlayers;
        public static event PlayerEvent OnSeperatePlayers;

        [SerializeField] GameObject playerDuoPrefab;
        [SerializeField] ParticleSystem xpParticles;

        float landingPointHeightOffset = 40f;
        [SerializeField] Transform player1LandingPoint;
        [SerializeField] Transform player2LandingPoint;
        [SerializeField] AnimationCurve landingCurve;
        [SerializeField] AnimationCurve takeOffCurve;

        [SerializeField] AudioSource xpAudioSource;
        [SerializeField] AudioSource gameAudioSource;

        [SerializeField] Material BGPlaneMaterial;

        [SerializeField] GameObject player1VCam;
        [SerializeField] GameObject player2VCam;
        [SerializeField] GameObject takeOffVCam;
        [SerializeField] GameObject gameVCam;
        [SerializeField] GameObject mainMenuVCam;

        [SerializeField] 

        public float playersXP = 0f;
        public float playersTotalXP = 0f;
        public float playersXPToNextLevel = 20f;

        public int waveNumber = 0;

        WaveSpawner waveSpawner;
        UpgradeManager upgradeManager;
        AudioManager audioManager;
        BackgroundLooper bgSpawner;
        PlayerInputManager playerInputManager;
        UIStateMachine uiStateMachine;
        TutorialAssistant tutorialAssistant;

        Gamepad controller;
        [SerializeField] GameObject testCube;

        void Awake()
        {
            waveSpawner = FindObjectOfType<WaveSpawner>();
            upgradeManager = FindObjectOfType<UpgradeManager>();
            audioManager = FindObjectOfType<AudioManager>();
            bgSpawner = FindObjectOfType<BackgroundLooper>();
            playerInputManager = GetComponent<PlayerInputManager>();
            uiStateMachine = FindObjectOfType<UIStateMachine>();
            tutorialAssistant = FindObjectOfType<TutorialAssistant>();
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
            playerPrefab.GetComponent<Player>().transform.position = player1LandingPoint.position + new Vector3(0, landingPointHeightOffset, 0);
            playerInputManager.DisableJoining();

            if (skipIntro == true)
            {
                gameVCam.SetActive(true);
                takeOffVCam.SetActive(false);   
                player1VCam.SetActive(false);
                player2VCam.SetActive(false);
                mainMenuVCam.SetActive(false);
                uiStateMachine.ShowGameplayUI();
                StartCoroutine(FadeBGPlaneTransparency(0.8f, 0.5f));
            }
            else
            {
                mainMenuVCam.SetActive(true);
                gameVCam.SetActive(false);
                player1VCam.SetActive(false);
                player2VCam.SetActive(false);
                takeOffVCam.SetActive(false);
                StartCoroutine(FadeBGPlaneTransparency(0f, 0.5f));
            }

            // controller = Dualsense.GetController();
        }

        void Update()
        {
            // testCube.transform.rotation *= Dualsense.GetRotation(4000f * Time.deltaTime);
            // move test cube with gyro if tilted and stop moving if not tilted
            // if (Dualsense.GetDirection(1000f * Time.deltaTime, controller).magnitude > 0.75f)
            // {
            //     print(Dualsense.GetDirection(1000f * Time.deltaTime, controller).magnitude);
            //     testCube.transform.position += Dualsense.GetDirection(1000f * Time.deltaTime, controller);
            // }
            // else
            // {
            //     testCube.transform.position = testCube.transform.position;
            // }

            waveNumber = waveSpawner.currentWaveNumber;

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
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            // start game when there are 2 players
            // if (PlayerInput.all.Count == 2 && !waveSpawner.spawningWaves)
            // {
            //     waveSpawner.StartSpawningWaves();
            //     // disable players joining
            //     GetComponent<PlayerInputManager>().DisableJoining();
            // }

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

            if (skipIntro == true)
            {
                player.rb.position = new Vector3(0, 0, -40);
                player.ApplySeparationForce(15000);

                if (tutorialAssistant.playTutorialAtStart != true)
                {
                    player.allowMovement = true;
                    player.allowDashing = true;
                    player.allowShooting = true;
                }
                else
                {
                    player.allowMovement = false;
                    player.allowDashing = false;
                    player.allowShooting = false;
                }
            }
            else
            {
                player.rb.isKinematic = true;
                player.entityCanvas.gameObject.SetActive(false);
                StartCoroutine(LandPlayer(player, player.playerNumber == 1 ? player1LandingPoint : player2LandingPoint));

                if (player.playerNumber == 1)
                {
                    playerPrefab.GetComponent<Player>().transform.position = player2LandingPoint.position + new Vector3(0, landingPointHeightOffset, 0);
                    Invoke("SwitchToNextPlayerCamera", 2f);
                }
                else if (player.playerNumber == 2)
                {
                    playerPrefab.GetComponent<Player>().transform.position = Vector3.zero;
                    playerInputManager.DisableJoining();
                    StartCoroutine(TeleportPlayersToPlayAreaAndStart());
                } 
            }

        }

        void SwitchToNextPlayerCamera()
        {
            player2VCam.SetActive(true);
            player1VCam.SetActive(false);
        }

        IEnumerator TeleportPlayersToPlayAreaAndStart()
        {
            yield return new WaitForSeconds(2f);
            takeOffVCam.SetActive(true);
            player2VCam.SetActive(false);
            player1VCam.SetActive(false);

            yield return new WaitForSeconds(2f);
            uiStateMachine.ShowGameplayUI();
            
            foreach (Player player in GetPlayers())
            {
                StartCoroutine(TakeOffPlayer(player, player.playerNumber == 1 ? player1LandingPoint : player2LandingPoint));
            }
            yield return new WaitForSeconds(1f);
            gameVCam.SetActive(true);
            takeOffVCam.SetActive(false);

            StartCoroutine(FadeBGPlaneTransparency(0.8f, 1.5f));

            foreach (Player player in GetPlayers())
            {
                player.rb.isKinematic = false;
                player.rb.position = new Vector3(50, 0, -40);

                player.ApplySeparationForce(15000);

                if (tutorialAssistant.playTutorialAtStart != true)
                {
                    player.allowMovement = true;
                    player.allowDashing = true;
                    player.allowShooting = true;
                }
                else
                {
                    player.allowMovement = false;
                    player.allowDashing = false;
                    player.allowShooting = false;
                }

                player.entityCanvas.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(1f);
            bgSpawner.enabled = true;
            yield return new WaitForSeconds(2f);

            // waveSpawner.StartSpawningWaves();
            tutorialAssistant.StartTutorialSequence();
        }

        IEnumerator FadeBGPlaneTransparency(float targetAlpha, float duration)
        {
            float startAlpha = BGPlaneMaterial.color.a;
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
                BGPlaneMaterial.color = new Color(BGPlaneMaterial.color.r, BGPlaneMaterial.color.g, BGPlaneMaterial.color.b, newAlpha);
                yield return null;
            }
        }  

        IEnumerator LandPlayer(Player player, Transform landingPoint)
        {
            float timer = 0f;
            Vector3 startPosition = player.transform.position;  
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                float newY = Mathf.Lerp(startPosition.y, landingPoint.position.y, landingCurve.Evaluate(timer));
                player.transform.position = new Vector3(startPosition.x, newY, startPosition.z);
                yield return null;
            }
        } 

        IEnumerator TakeOffPlayer(Player player, Transform landingPoint)
        {
            float timer = 0f;
            Vector3 startPosition = player.transform.position;  
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                float newY = Mathf.Lerp(startPosition.y, landingPoint.position.y + landingPointHeightOffset * 2, takeOffCurve.Evaluate(timer));
                player.transform.position = new Vector3(startPosition.x, newY, startPosition.z);
                yield return null;
            }
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
            if (PlayerInput.all.Count < 2) return false;
            foreach (PlayerInput playerInput in PlayerInput.all)
            {
                if (playerInput.GetComponent<Player>().inDuoMode == false) return false;
            }
            return true;
        }

        public Player[] GetPlayers()
        {
            Player[] players = new Player[PlayerInput.all.Count];
            for (int i = 0; i < PlayerInput.all.Count; i++)
            {
                players[i] = PlayerInput.all[i].GetComponent<Player>();
            }
            return players;
        }

        public void EndGame()
        {
            print("Game Over");
            uiStateMachine.ShowGameOverUI();
            waveSpawner.StopSpawningWaves();
            // reload current scene
            // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void CombinePlayersButtonPressed()
        {
            if (OnCombinePlayers != null) StartCoroutine(CombinePlayers());
        }

        public void SeperatePlayersButtonPressed()
        {
            if (OnSeperatePlayers != null) StartCoroutine(SeperatePlayers());
        }

        public void StartGameButtonPressed()
        {
            print("Starting Game...");
            player1VCam.SetActive(true);
            mainMenuVCam.SetActive(false);

            uiStateMachine.CloseMainMenu();
            playerInputManager.EnableJoining();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void QuitGameButtonPressed()
        {
            print("Quitting Game...");
            Application.Quit();
        }

        public void RestartGameButtonPressed()
        {
            print("Restarting Game...");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ResumeGameButtonPressed()
        {
            print("Resuming Game...");
            uiStateMachine.TogglePauseMenu();

            foreach (Player player in GetPlayers())
            {
                player.AssignMenuControls(null, null, player.playerNumber);
            }
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
                // triggerModule.SetCollider(0, killerPlayer.GetComponent<Collider>());
                int colliderIndex = 0;
                foreach (Player player in GetPlayers())
                {
                    triggerModule.SetCollider(colliderIndex, player.GetComponent<Collider>());
                    colliderIndex++;
                }

                ParticleSystem.ExternalForcesModule externalForcesModule = xpParticle.externalForces;
                // externalForcesModule.AddInfluence(killerPlayer.GetComponent<ParticleSystemForceField>());
                foreach (Player player in GetPlayers())
                {
                    externalForcesModule.AddInfluence(player.GetComponent<ParticleSystemForceField>());
                }

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

        public void IncrementKillCount(Player killer)
        {
            killer.killCount++;
            uiStateMachine.UpdatePlayerKillCount(killer);
            
        }

        void OnDisable()
        {
            foreach (PlayerInput playerInput in PlayerInput.all)
            {
                RumbleManager.instance.StopContinuousRumble(playerInput.devices[0] as Gamepad);
            }
        }
    }
}
