using System.Collections;
using System.Collections.Generic;
using Ekkam;
using UnityEngine;

[CreateAssetMenu(menuName = "Wave Config", fileName = "New Wave Config")]
public class WaveConfigSO : ScriptableObject
{
    public List<GameObject> enemies = new List<GameObject>();
    public List<Transform> enemyPaths = new List<Transform>();
    public List<bool> spawnNextEnemyTogether = new List<bool>();
    public List<bool> invertPathX = new List<bool>();

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

    public bool GetPathXInversion(int index)
    {
        return invertPathX[index];
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
