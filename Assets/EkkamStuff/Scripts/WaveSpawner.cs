using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ekkam {
    public class WaveSpawner : MonoBehaviour
    {
        public bool spawningWaves = false;
        public bool spawnOnGameStart = false;
        [SerializeField] List<WaveConfigSO> waves = new List<WaveConfigSO>();

        [SerializeField] int combineInterval = 4;

        [HideInInspector]
        public List<Enemy> enemiesOnScreen = new List<Enemy>();

        public WaveConfigSO currentWave;
        public int currentWaveNumber = 0;
        public bool waitForAllEnemiesToDieBeforeSpawningNextWave = true;
        GameManager gameManager;
        AudioManager audioManager;
        ProceduralWaveGenerator proceduralWaveGenerator;

        [SerializeField] float enemyRotationY = 180f;

        [Header("Procedural Wave Generation")]
        [SerializeField] bool useProceduralWaveGeneration = false;
        [SerializeField] bool onlyUseProceduralAfterPredefined = false;
        [SerializeField] int numberOfProceduralWavesToGenerate = 10;
        [SerializeField] List<WaveConfigSO> proceduralWaves = new List<WaveConfigSO>();

        void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            audioManager = FindObjectOfType<AudioManager>();
            proceduralWaveGenerator = GetComponent<ProceduralWaveGenerator>();
        }

        void Start()
        {
            if (spawnOnGameStart == true) StartSpawningWaves();
        }

        public void StartSpawningWaves()
        {
            spawningWaves = true;

            if (useProceduralWaveGeneration == true && onlyUseProceduralAfterPredefined == false)
            {
                StartCoroutine(SpawnProceduralWaves());
            }
            else
            {
                StartCoroutine(SpawnPredefinedWaves());
            }
        }

        public void StopSpawningWaves()
        {
            spawningWaves = false;
            StopAllCoroutines();
        }

        IEnumerator SpawnProceduralWaves()
        {
            for (int i = 0; i < numberOfProceduralWavesToGenerate; i++)
            {
                WaveConfigSO proceduralWave = proceduralWaveGenerator.GenerateWave();
                proceduralWaves.Add(proceduralWave);
                yield return StartCoroutine(SpawnWave(proceduralWave));
            }

            currentWave = null;

            print("All waves spawned");
        }

        IEnumerator SpawnPredefinedWaves()
        {
            foreach (WaveConfigSO wave in waves)
            {
                yield return StartCoroutine(SpawnWave(wave));
            }

            currentWave = null;

            print("All waves spawned");

            if (useProceduralWaveGeneration == true && onlyUseProceduralAfterPredefined == true)
            {
                print("Spawning procedural waves");
                StartCoroutine(SpawnProceduralWaves());

                print("No longer waiting for all enemies to die before spawning next wave");
                waitForAllEnemiesToDieBeforeSpawningNextWave = false;
            }
        }

        IEnumerator SpawnWave(WaveConfigSO wave)
        {
            print("Spawning wave " + currentWaveNumber);
            currentWave = wave;
            currentWaveNumber++;

            if (currentWave.name.Contains("Boss"))
            {
                audioManager.PlayBossMusic();
            }
            else
            {
                audioManager.PlayGameMusic();
            }

            for (int i = 0; i < wave.waveEnemies.Count; i++)
            {
                Enemy currentEnemy = currentWave.waveEnemies[i].enemy.GetComponent<Enemy>();
                Transform path = Instantiate(currentWave.waveEnemies[i].enemyPath, transform);
                if (currentWave.waveEnemies[i].invertPathX)
                {
                    path.localScale = new Vector3(-1, 1, 1);
                }
                currentEnemy.pathPrefab = path;
                currentEnemy.moveSpeed = currentWave.waveEnemies[i].moveSpeed;
                currentEnemy.speedCurve = currentWave.waveEnemies[i].speedCurve;
                Instantiate(
                    currentEnemy.gameObject,
                    new Vector3(currentEnemy.pathPrefab.GetChild(0).position.x, 0, currentEnemy.pathPrefab.GetChild(0).position.z),
                    Quaternion.Euler(0, enemyRotationY, 0),
                    transform
                );
                yield return new WaitForSeconds(currentWave.GetSpawnTime(i));
            }

            if (waitForAllEnemiesToDieBeforeSpawningNextWave)
            {
                yield return new WaitUntil(() => enemiesOnScreen.Count == 0);
            }
            else
            {
                yield return new WaitForSeconds(currentWave.waitTimeBeforeNextWave);
            }

            if (gameManager.AllPlayersInDuoMode())
            {
                gameManager.StartCoroutine("SeperatePlayers");
                yield return new WaitUntil(() => !gameManager.AllPlayersInDuoMode());
            }

            if (currentWaveNumber % combineInterval == 0 && currentWaveNumber != 0 && PlayerInput.all.Count > 1)
            {
                gameManager.StartCoroutine("CombinePlayers");
                yield return new WaitUntil(() => gameManager.AllPlayersInDuoMode());
            }
        }
    }
}
