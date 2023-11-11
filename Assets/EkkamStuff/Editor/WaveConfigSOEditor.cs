using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ekkam
{
    [CustomEditor(typeof(WaveConfigSO))]
    public class WaveConfigSOEditor : Editor
    {
        // Allows the user to add or remove enemies. Show enemy path and delay next spawn time for each enemy in one line.
        public override void OnInspectorGUI()
        {
            WaveConfigSO waveConfig = (WaveConfigSO)target;

            // base.OnInspectorGUI();
            // EditorGUILayout.Space();

            EditorGUILayout.LabelField("Enemies", EditorStyles.boldLabel);
            for (int i = 0; i < waveConfig.enemies.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                waveConfig.enemies[i] = (GameObject)EditorGUILayout.ObjectField(waveConfig.enemies[i], typeof(GameObject), true);
                waveConfig.enemyPaths[i] = (Transform)EditorGUILayout.ObjectField(waveConfig.enemyPaths[i], typeof(Transform), true);
                EditorGUILayout.Space(width: 20);
                waveConfig.spawnNextEnemyTogether[i] = EditorGUILayout.ToggleLeft("Spawn Next Enemy Together", waveConfig.spawnNextEnemyTogether[i]);
                if (GUILayout.Button("-"))
                {
                    waveConfig.enemies.RemoveAt(i);
                    waveConfig.enemyPaths.RemoveAt(i);
                    waveConfig.spawnNextEnemyTogether.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Enemy"))
            {
                waveConfig.enemies.Add(null);
                waveConfig.enemyPaths.Add(null);
                waveConfig.spawnNextEnemyTogether.Add(false);

                // If the user adds a new enemy, set the new enemy to be the same as the previous enemy.
                waveConfig.enemies[waveConfig.enemies.Count - 1] = waveConfig.enemies[waveConfig.enemies.Count - 2];
                waveConfig.enemyPaths[waveConfig.enemyPaths.Count - 1] = waveConfig.enemyPaths[waveConfig.enemyPaths.Count - 2];  
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Spawn Settings", EditorStyles.boldLabel);
            waveConfig.randomizeSpawnTime = EditorGUILayout.Toggle("Randomize Spawn Time", waveConfig.randomizeSpawnTime);
            waveConfig.timeBetweenEnemySpawns = EditorGUILayout.FloatField("Time Between Enemy Spawns", waveConfig.timeBetweenEnemySpawns);

            if (waveConfig.randomizeSpawnTime)
            {
                waveConfig.spawnTimeVariance = EditorGUILayout.FloatField("Spawn Time Variance", waveConfig.spawnTimeVariance);
                waveConfig.minSpawnTime = EditorGUILayout.FloatField("Min Spawn Time", waveConfig.minSpawnTime);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Wave Settings", EditorStyles.boldLabel);
            waveConfig.waitTimeBeforeNextWave = EditorGUILayout.FloatField("Wait Time Before Next Wave", waveConfig.waitTimeBeforeNextWave);
        }

    }
}
