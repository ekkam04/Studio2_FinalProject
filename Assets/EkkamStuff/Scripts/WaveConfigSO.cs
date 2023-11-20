using System.Collections;
using System.Collections.Generic;
using Ekkam;
using UnityEngine;

[CreateAssetMenu(menuName = "Wave Config", fileName = "New Wave Config")]
public class WaveConfigSO : ScriptableObject
{
    public List<WaveEnemy> waveEnemies = new List<WaveEnemy>();

    public bool randomizeSpawnTime = false;
    public float timeBetweenEnemySpawns = 1f;
    public float spawnTimeVariance = 0f;
    public float minSpawnTime = 0.2f;

    public float waitTimeBeforeNextWave = 0f;

    public float GetSpawnTime(int index)
    {
        if (waveEnemies[index].spawnNextEnemyTogether)
        {
            return 0f;
        }
        else if (randomizeSpawnTime)
        {
            float spawnTime = Random.Range(timeBetweenEnemySpawns - spawnTimeVariance, timeBetweenEnemySpawns + spawnTimeVariance);
            return Mathf.Clamp(spawnTime, minSpawnTime, Mathf.Infinity);
        }
        else
        {
            return timeBetweenEnemySpawns;
        } 
    }
}
