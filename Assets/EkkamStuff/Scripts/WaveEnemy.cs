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

        public WaveEnemy(GameObject enemy, Transform enemyPath, float moveSpeed, bool invertPathX)
        {
            this.enemy = enemy;
            this.enemyPath = enemyPath;
            this.speedCurve = new AnimationCurve(new Keyframe(1, 1), new Keyframe(1, 1));
            this.moveSpeed = moveSpeed;
            this.invertPathX = invertPathX;
            this.spawnNextEnemyTogether = false;
        }
    }
}
