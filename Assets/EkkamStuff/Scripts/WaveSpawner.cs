using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] WaveConfigSO[] waves;
        public WaveConfigSO currentWave;

        [SerializeField] float timeBetweenWaves = 2f;
        [SerializeField] float enemyRotationY = 180f;

        void Start()
        {
            StartCoroutine(SpawnWaves());
        }

        IEnumerator SpawnWaves()
        {
            foreach (WaveConfigSO wave in waves)
            {
                currentWave = wave;
                for (int i = 0; i < wave.GetEnemyCount(); i++)
                {
                    Instantiate(
                        currentWave.GetEnemy(i),
                        currentWave.pathPrefab.GetChild(0).position,
                        Quaternion.Euler(0, enemyRotationY, 0),
                        transform
                    );
                    yield return new WaitForSeconds(currentWave.GetRandomSpawnTime());
                }

                yield return new WaitForSeconds(timeBetweenWaves);
            }

            print("All waves spawned");
        }
    }
}
