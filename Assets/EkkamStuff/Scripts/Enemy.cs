using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace Ekkam {
    public class Enemy : DamagableEntity
    {
        WaveSpawner waveSpawner;

        [HideInInspector]
        public List<Transform> waypoints;
        int waypointIndex = 0;

        ShootingManager shootingManager;
        GameManager gameManager;

        float shootTimer = 0f;
        float waitTimer = 0f;
        float OnMoveTimer;

        [HideInInspector]
        public Transform pathPrefab;

        public bool shootTowardsInitialPlayerPos = false;

        [Header("----- Enemy Stats -----")]

        [HideInInspector]
        public AnimationCurve speedCurve; // This is now controlled by the WaveConfigSO

        [HideInInspector]
        public float moveSpeed = 30f; // This is now controlled by the WaveConfigSO

        [Tooltip("The cooldown time before the enemy can shoot again")] public float attackSpeed = 2f;
        [Tooltip("The damage dealt to a player when they collide with the enemy")] public float damageOnImpact = 1f;

        public float waitWaypointDuration = 1f;

        void Awake()
        {
            waveSpawner = FindObjectOfType<WaveSpawner>();
            shootingManager = GetComponent<ShootingManager>();
            gameManager = FindObjectOfType<GameManager>();
            meshRenderer = GetComponent<MeshRenderer>();
            InitializeDamagableEntity();
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
            waitTimer += Time.deltaTime;

            if (shootTimer >= attackSpeed)
            {
                if (shootTowardsInitialPlayerPos)
                {
                    shootingManager.Shoot("EnemyProjectile", this.gameObject, FindClosestPlayer().transform);
                }
                else
                {
                    shootingManager.Shoot("EnemyProjectile", this.gameObject);
                }
                shootTimer = 0f;
            }

            if (waitTimer >= waitWaypointDuration)
            {
                FollowPath();
            }
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
                    if (waypoints[waypointIndex].gameObject.tag == "WaitWaypoint")
                    {
                        waitTimer = 0f;
                    }
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

        private void OnDestroy() {
            if (killer != null) gameManager.SpawnXP(transform.position, 5, killer);
            waveSpawner.enemiesOnScreen.Remove(this);
            Destroy(pathPrefab.gameObject);
        }
    }
}
