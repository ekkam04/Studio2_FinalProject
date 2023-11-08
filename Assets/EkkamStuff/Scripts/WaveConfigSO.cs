using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Wave Config", fileName = "New Wave Config")]
public class WaveConfigSO : ScriptableObject
{
    [SerializeField] List<GameObject> enemies;
    public Transform pathPrefab;

    public float moveSpeed = 30f;
    public float shootSpeed = 30f;
    public float shootInterval = 1f;

    [SerializeField] float timeBetweenEnemySpawns = 1f;
    [SerializeField] float spawnTimeVariance = 0f;
    [SerializeField] float minSpawnTime = 0.2f;

    public int GetEnemyCount()
    {
        return enemies.Count;
    }

    public GameObject GetEnemy(int index)
    {
        return enemies[index];
    }

    public List<Transform> GetWaypoints()
    {
        List<Transform> waypoints = new List<Transform>();
        foreach(Transform child in pathPrefab )
        {
            waypoints.Add(child);
        }
        return waypoints;
    }

    public float GetRandomSpawnTime()
    {
        float spawnTime = Random.Range(timeBetweenEnemySpawns - spawnTimeVariance, timeBetweenEnemySpawns + spawnTimeVariance);

        return Mathf.Clamp(spawnTime, minSpawnTime, Mathf.Infinity);
    }
}
