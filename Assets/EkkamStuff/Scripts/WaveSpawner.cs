using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] WaveConfigSO[] waves;
        [SerializeField] int upgradesInterval = 3;

        public WaveConfigSO currentWave;
        public int currentWaveNumber = 0;
        UpgradeManager upgradeManager;

        [SerializeField] float enemyRotationY = 180f;

        void Awake()
        {
            upgradeManager = FindObjectOfType<UpgradeManager>();
        }

        void Start()
        {
            StartCoroutine(SpawnWaves());
        }

        IEnumerator SpawnWaves()
        {
            foreach (WaveConfigSO wave in waves)
            {
                currentWave = wave;
                currentWaveNumber++;
                for (int i = 0; i < wave.GetEnemyCount(); i++)
                {
                    Enemy currentEnemy = currentWave.GetEnemy(i);
                    Transform path = Instantiate(currentWave.GetPathOfEnemy(i), transform);
                    if (currentWave.GetPathXInversion(i))
                    {
                        path.localScale = new Vector3(-1, 1, 1);
                    }
                    currentEnemy.pathPrefab = path;
                    Instantiate(
                        currentEnemy.gameObject,
                        currentEnemy.pathPrefab.GetChild(0).position,
                        Quaternion.Euler(0, enemyRotationY, 0),
                        transform
                    );
                    yield return new WaitForSeconds(currentWave.GetSpawnTime(i));
                }

                if (currentWaveNumber % upgradesInterval == 0 && currentWaveNumber != 0)
                {
                    upgradeManager.ShowUpgrades();
                    yield return new WaitUntil(() => !upgradeManager.waitingForUpgrade);
                }
                
                yield return new WaitForSeconds(currentWave.waitTimeBeforeNextWave);
            }

            print("All waves spawned");
        }
    }
}
