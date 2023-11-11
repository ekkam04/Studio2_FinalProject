using System.Collections;
using System.Collections.Generic;
using Ekkam;
using UnityEngine;

[CreateAssetMenu(menuName = "Wave Config", fileName = "New Wave Config")]
public class WaveConfigSO : ScriptableObject
{
    // [HideInInspector]
    public List<GameObject> enemies;

    // [HideInInspector]
    public List<Transform> enemyPaths;

    // [HideInInspector]
    public List<bool> spawnNextEnemyTogether;

    public bool randomizeSpawnTime = false;
    public float timeBetweenEnemySpawns = 1f;
    public float spawnTimeVariance = 0f;
    public float minSpawnTime = 0.2f;

    public float waitTimeBeforeNextWave = 0f;

    public int GetEnemyCount()
    {
        return enemies.Count;
    }

    public Enemy GetEnemy(int index)
    {
        if (enemies[index].GetComponent<Enemy>() != null)
        {
            return enemies[index].GetComponent<Enemy>();
        }
        else
        {
            Debug.LogWarning("Enemy not set for enemy " + index + " in wave " + name + ". Using first enemy in list.");
            return enemies[0].GetComponent<Enemy>();
        }
    }

    public Transform GetPathOfEnemy(int index)
    {
        if (enemyPaths[index] != null)
        {
            return enemyPaths[index];
        }
        else
        {
            Debug.LogWarning("Enemy path not set for enemy " + index + " in wave " + name + ". Using first path in list.");
            return enemyPaths[0];
        }
    }

    public float GetSpawnTime(int index)
    {
        if (spawnNextEnemyTogether[index])
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
