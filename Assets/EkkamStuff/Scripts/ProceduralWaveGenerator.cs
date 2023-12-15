using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class ProceduralWaveGenerator : MonoBehaviour
    {
        public float timeSinceGameStart = 0f;
        public float timeToReachMaxDifficulty = 120f;
        public float timeToReachMaxEnemyTier = 60f;

        public float waitTimeBetweenEnemySpawns = 0.75f;
        public float waitTimeBetweenWaves = 2f;

        [SerializeField] [Tooltip("Enemies to use in procedural wave generation ordered in acsending difficulty.")] List<Enemy> enemies = new List<Enemy>();
        [SerializeField] List<Transform> paths = new List<Transform>();

        [Header("Difficulty Settings")]

        // The max and min number of enemies that can spawn in a wave
        public int min_maxEnemiesPerWave = 5;
        public int max_maxEnemiesPerWave = 20;

        public int min_minEnemiesPerWave = 1;
        public int max_minEnemiesPerWave = 5;

        [SerializeField] AnimationCurve EnemyCountCurve = new AnimationCurve(new Keyframe(1, 1), new Keyframe(1, 1));

        // Enemy tiers

        [SerializeField] AnimationCurve enemyTierCurve = new AnimationCurve(new Keyframe(1, 1), new Keyframe(1, 1));

        [Header("Current Difficulty (Don't Edit These Values Directly)")]

        public int currentMaxEnemiesPerWave;
        public int currentMinEnemiesPerWave;

        public int currentEnemyTier = 0;

        WaveConfigSO previouslyGeneratedWave = null;

        public WaveConfigSO GenerateWave()
        {
            WaveConfigSO proceduralWave = ScriptableObject.CreateInstance<WaveConfigSO>();

            WaveEnemy previousWaveEnemy = new WaveEnemy();
            WaveEnemy[] waveEnemies = new WaveEnemy[Random.Range(currentMinEnemiesPerWave, currentMaxEnemiesPerWave)];
            for (int i = 0; i < waveEnemies.Length; i++)
            {
                bool invertPathX = Random.Range(0, 2) == 0 ? false : true;
                Enemy enemy = enemies[Random.Range(0, currentEnemyTier + 1)];
                Transform path = paths[Random.Range(0, paths.Count)];
                while (previousWaveEnemy.enemyPath == path && previousWaveEnemy.invertPathX == invertPathX)
                {
                    path = paths[Random.Range(0, paths.Count)];
                }

                waveEnemies[i] = new WaveEnemy(enemy.gameObject, paths[Random.Range(0, paths.Count)], 60f, invertPathX);
                previousWaveEnemy = waveEnemies[i];
            }
            proceduralWave.waveEnemies.AddRange(waveEnemies);
            proceduralWave.timeBetweenEnemySpawns = waitTimeBetweenEnemySpawns;

            if (waitTimeBetweenEnemySpawns > 0.2f) waitTimeBetweenEnemySpawns -= 0.03f;

            if (waitTimeBetweenWaves > 0.5f) waitTimeBetweenWaves -= 0.03f;

            previouslyGeneratedWave = proceduralWave;
            return proceduralWave;
        }

        void Awake()
        {
            EvaluateCurves();
        }

        void Update()
        {
            timeSinceGameStart += Time.deltaTime;
            EvaluateCurves();
        }

        void EvaluateCurves()
        {
            currentMaxEnemiesPerWave = Mathf.RoundToInt(EnemyCountCurve.Evaluate(timeSinceGameStart / timeToReachMaxDifficulty) * (max_maxEnemiesPerWave - min_maxEnemiesPerWave) + min_maxEnemiesPerWave);
            currentMinEnemiesPerWave = Mathf.RoundToInt(EnemyCountCurve.Evaluate(timeSinceGameStart / timeToReachMaxDifficulty) * (max_minEnemiesPerWave - min_minEnemiesPerWave) + min_minEnemiesPerWave);

            currentEnemyTier = Mathf.RoundToInt(enemyTierCurve.Evaluate(timeSinceGameStart / timeToReachMaxEnemyTier) * (enemies.Count - 1));
        }
    }
}
