using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class WaveSpawner : MonoBehaviour
    {
        public bool spawningWaves = false;
        public bool spawnOnGameStart = false;
        [SerializeField] WaveConfigSO[] waves;
        // [SerializeField] int upgradesInterval = 3;
        [SerializeField] int combineInterval = 4;

        [HideInInspector]
        public List<Enemy> enemiesOnScreen = new List<Enemy>();

        public WaveConfigSO currentWave;
        public int currentWaveNumber = 0;
        public bool waitForAllEnemiesToDieBeforeSpawningNextWave = true;
        UpgradeManager upgradeManager;
        GameManager gameManager;

        [SerializeField] float enemyRotationY = 180f;

        void Awake()
        {
            upgradeManager = FindObjectOfType<UpgradeManager>();
            gameManager = FindObjectOfType<GameManager>();
        }

        void Start()
        {
            if (spawnOnGameStart == true) StartSpawningWaves();
        }

        public void StartSpawningWaves()
        {
            spawningWaves = true;
            StartCoroutine(SpawnWaves());
        }

        public void StopSpawningWaves()
        {
            spawningWaves = false;
            StopCoroutine(SpawnWaves());
        }

        IEnumerator SpawnWaves()
        {
            foreach (WaveConfigSO wave in waves)
            {
                currentWave = wave;
                currentWaveNumber++;
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

                // if (currentWaveNumber % upgradesInterval == 0 && currentWaveNumber != 0)
                // {
                //     upgradeManager.ShowUpgrades();
                //     yield return new WaitUntil(() => !upgradeManager.waitingForUpgrade);
                // }

                if (gameManager.AllPlayersInDuoMode())
                {
                    gameManager.StartCoroutine("SeperatePlayers");
                    yield return new WaitUntil(() => !gameManager.AllPlayersInDuoMode());
                }

                if (currentWaveNumber % combineInterval == 0 && currentWaveNumber != 0)
                {
                    gameManager.StartCoroutine("CombinePlayers");
                    yield return new WaitUntil(() => gameManager.AllPlayersInDuoMode());
                }

                print("Spawning next wave");
            }

            currentWave = null;
            print("All waves spawned");
        }
    }
}
