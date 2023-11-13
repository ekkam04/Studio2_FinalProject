using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace Ekkam {
    public class Enemy : MonoBehaviour
    {
        WaveSpawner waveSpawner;
        WaveConfigSO waveConfig;

        List<Transform> waypoints;
        int waypointIndex = 0;

        ShootingManager shootingManager;

        float shootTimer = 0f;

        [HideInInspector]
        public Transform pathPrefab;

        [Header("----- Enemy Stats -----")]

        [Tooltip("The total health")] [SerializeField] float health = 100f; // Still need to implement
        [Tooltip("The speed of the enemy")] [SerializeField] float moveSpeed = 30f;
        [Tooltip("The cooldown time before the enemy can shoot again")] [SerializeField] float attackSpeed = 2f;
        [Tooltip("The damage dealt to a player when they collide with the enemy")] [SerializeField] float damageOnImpact = 1f; // Still need to implement

        void Awake()
        {
            waveSpawner = FindObjectOfType<WaveSpawner>();
            shootingManager = GetComponent<ShootingManager>();
        }

        void Start()
        {
            waveConfig = waveSpawner.currentWave;
            waypoints = GetWaypoints();
            transform.position = waypoints[waypointIndex].position;
        }

        void Update()
        {
            shootTimer += Time.deltaTime;
            if (shootTimer >= attackSpeed)
            {
                shootingManager.Shoot("EnemyProjectile");
                shootTimer = 0f;
            }

            FollowPath();
        }

        void FollowPath()
        {
            if (waypointIndex < waypoints.Count)
            {
                Vector3 targetPosition = waypoints[waypointIndex].position;
                float movementThisFrame = moveSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementThisFrame);

                if (transform.position == targetPosition)
                {
                    waypointIndex++;
                }
            }
            else
            {
                Destroy(gameObject);
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
        }
    }
}
