using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace Ekkam {
    public class Enemy : MonoBehaviour
    {
        WaveSpawner waveSpawner;

        [HideInInspector]
        public List<Transform> waypoints;
        int waypointIndex = 0;

        ShootingManager shootingManager;
        MeshRenderer meshRenderer;

        float shootTimer = 0f;
        float OnMoveTimer;

        [HideInInspector]
        public Transform pathPrefab;

        [Header("----- Enemy Stats -----")]

        [Tooltip("The total health")] public float health = 5f;

        [HideInInspector]
        public AnimationCurve speedCurve; // This is now controlled by the WaveConfigSO

        [HideInInspector]
        public float moveSpeed = 30f; // This is now controlled by the WaveConfigSO

        [Tooltip("The cooldown time before the enemy can shoot again")] public float attackSpeed = 2f;
        [Tooltip("The damage dealt to a player when they collide with the enemy")] public float damageOnImpact = 1f;

        void Awake()
        {
            waveSpawner = FindObjectOfType<WaveSpawner>();
            shootingManager = GetComponent<ShootingManager>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        void Start()
        {
            waypoints = GetWaypoints();
            transform.position = waypoints[waypointIndex].position;
            waveSpawner.enemiesOnScreen.Add(this);
        }

        void Update()
        {       
            shootTimer += Time.deltaTime;
            if (shootTimer >= attackSpeed)
            {
                shootingManager.Shoot("EnemyProjectile", this.gameObject);
                shootTimer = 0f;
            }

            FollowPath();
        }

        void FollowPath()
        {
            if (waypointIndex < waypoints.Count)
            {
                OnMoveTimer += Time.deltaTime;
                Vector3 targetPosition = new Vector3(waypoints[waypointIndex].position.x, 0, waypoints[waypointIndex].position.z);
                float movementThisFrame = moveSpeed * Time.deltaTime * speedCurve.Evaluate(OnMoveTimer);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementThisFrame);

                if (transform.position == targetPosition)
                {
                    waypointIndex++;
                    OnMoveTimer = 0f;
                }
            }
            else
            {
                waveSpawner.enemiesOnScreen.Remove(this);
                Destroy(gameObject);
            }
        }

        public void FollowEditorPath(float time)
        {
            if (waypointIndex < waypoints.Count)
            {
                OnMoveTimer += time;
                Vector3 targetPosition = new Vector3(waypoints[waypointIndex].position.x, 0, waypoints[waypointIndex].position.z);
                float movementThisFrame = moveSpeed * time * speedCurve.Evaluate(OnMoveTimer);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementThisFrame);

                if (transform.position == targetPosition)
                {
                    waypointIndex++;
                    OnMoveTimer = 0f;
                }
            }
            else
            {
                DestroyImmediate(gameObject);
            }
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

        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                StartCoroutine(FlashColor(Color.red, 0.1f));
            }
        }

        private void OnDestroy() {
            waveSpawner.enemiesOnScreen.Remove(this);
            Destroy(pathPrefab.gameObject);
        }

        public Enemy FindClosestEnemy()
        {
            Enemy closestEnemy = null;
            float closestDistance = Mathf.Infinity;
            foreach (Enemy enemy in FindObjectsOfType<Enemy>())
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
            return closestEnemy;
        }

        public Player FindClosestPlayer()
        {
            Player closestPlayer = null;
            float closestDistance = Mathf.Infinity;
            foreach (Player player in FindObjectsOfType<Player>())
            {
                if (player == this) continue;
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }
            return closestPlayer;
        }

        IEnumerator FlashColor(Color color, float duration)
        {
            // flash smoothly between the current color and the new color
            float timer = 0f;
            while (timer < duration)
            {
                meshRenderer.material.color = Color.Lerp(meshRenderer.material.color, color, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }
            // flash smoothly back to the original color
            timer = 0f;
            while (timer < duration * 2)
            {
                meshRenderer.material.color = Color.Lerp(meshRenderer.material.color, Color.white, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }
}
