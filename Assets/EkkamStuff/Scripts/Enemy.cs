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

        [SerializeField] GameObject projectilePrefab;
        [SerializeField] List<GameObject> projectilePool;
        int projectilePoolSize = 3;
        GameObject projectilePoolHolder;

        [Header("----- Enemy Stats -----")]
        [Tooltip("The total health")] [SerializeField] float health = 100f; // Still need to implement
        [Tooltip("The speed of the enemy")] [SerializeField] float moveSpeed = 30f;
        [Tooltip("The speed of the projectile")] [SerializeField] float projectileSpeed = 90f;
        [Tooltip("The damage of the projectile")] [SerializeField] float projectileDamage = 5f; // Still need to implement
        [Tooltip("The size of the projectile")] [SerializeField] float projectileSize = 1f;
        [Tooltip("The cooldown time before the enemy can shoots again")] [SerializeField] float attackSpeed = 2f;
        [Tooltip("The damage dealt to a player when they collide with the enemy")] [SerializeField] float damageOnImpact = 1f; // Still need to implement
        [Tooltip("The amount of projectiles fired side by side")] [SerializeField] int multishotCount = 1;
        [Tooltip("The gap between projectiles when multishot is more than 1")] [SerializeField] float multishotGapX = 0.5f;
        [Tooltip("The amount of projectiles fired per shot")] [SerializeField] int burstFireCount = 1;
        [Tooltip("The delay between each bullet being fired in burst")] [SerializeField] float burstFireDelay = 0.1f;
        
        float shootTimer = 0f;
        float projectileLifetime = 3f;

        void Awake()
        {
            waveSpawner = FindObjectOfType<WaveSpawner>();
        }

        void Start()
        {
            waveConfig = waveSpawner.currentWave;
            waypoints = waveConfig.GetWaypoints();
            transform.position = waypoints[waypointIndex].position;
        }

        private void OnEnable()
        {
            projectilePoolHolder = new GameObject("ProjectilePool");

            // Add projectile prefabs to the pool
            projectilePool = new List<GameObject>();
            projectilePoolSize = burstFireCount * multishotCount * 2;
            for (int i = 0; i < projectilePoolSize; i++)
            {
                GameObject newProjectile = SpawnProjectile();
                projectilePool.Add(newProjectile);
                newProjectile.SetActive(false);
            }
        }

        private void OnDisable()
        {
            Destroy(projectilePoolHolder);
        }

        void Update()
        {
            shootTimer += Time.deltaTime;
            if (shootTimer >= attackSpeed)
            {
                Shoot();
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

        async void Shoot()
        {
            for (int i = 0; i < burstFireCount; i++)
            {
                float bulletGapX = 0;
                if (multishotCount > 1)
                {
                    bulletGapX = -(multishotGapX * (multishotCount - 1) / 2);
                }
                for (int j = 0; j < multishotCount; j++)
                {
                    // Find an inactive projectile in the pool
                    bool foundInactiveProjectile = false;
                    for (int k = 0; k < projectilePool.Count; k++)
                    {
                        if (!projectilePool[k].activeInHierarchy)
                        {
                            print("Reusing projectile from pool");
                            projectilePool[k].transform.position = transform.position + new Vector3(bulletGapX, 0, 0);
                            projectilePool[k].transform.rotation = Quaternion.Euler(90, 0, 0);
                            projectilePool[k].transform.localScale *= projectileSize;
                            projectilePool[k].SetActive(true);
                            projectilePool[k].GetComponent<Rigidbody>().velocity = transform.forward * projectileSpeed;
                            StartCoroutine(DeactivateProjectile(projectilePool[k]));
                            foundInactiveProjectile = true;
                            break;
                        }
                    }
                    // If no inactive projectile is found, add a new one to the pool
                    if (!foundInactiveProjectile)
                    {
                        print("Adding new projectile to pool");
                        GameObject newProjectile = SpawnProjectile();
                        projectilePool.Add(newProjectile);
                        newProjectile.transform.position = transform.position + new Vector3(bulletGapX, 0, 0);
                        newProjectile.transform.rotation = Quaternion.Euler(90, 0, 0);
                        newProjectile.transform.localScale *= projectileSize;
                        newProjectile.GetComponent<Rigidbody>().velocity = transform.forward * projectileSpeed;
                        StartCoroutine(DeactivateProjectile(newProjectile));
                    }
                    bulletGapX += multishotGapX;
                }
                int loopDelay = (int)(burstFireDelay * 1000);
                await Task.Delay(loopDelay);
            }
        }

        GameObject SpawnProjectile()
        {
            GameObject newProjectile = Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.Euler(90, 0, 0),
                projectilePoolHolder.transform
            );
            projectilePool.Add(newProjectile);
            return newProjectile;
        }

        IEnumerator DeactivateProjectile(GameObject projectile)
        {
            yield return new WaitForSeconds(projectileLifetime);
            projectile.SetActive(false);
            print("Projectile deactivated");
        }
    }
}
