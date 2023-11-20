using System;
using UnityEngine;

namespace Ekkam
{
    [Serializable]
    public struct WaveEnemy
    {
        public GameObject enemy;
        public Transform enemyPath;
        public AnimationCurve speedCurve;
        public float moveSpeed;
        public bool invertPathX;
        public bool spawnNextEnemyTogether;
    }
}
