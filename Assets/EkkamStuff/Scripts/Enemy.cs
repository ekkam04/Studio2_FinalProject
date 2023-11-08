using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class Enemy : MonoBehaviour
    {
        WaveSpawner waveSpawner;
        WaveConfigSO waveConfig;

        List<Transform> waypoints;
        int waypointIndex = 0;

        [SerializeField] GameObject projectilePrefab;
        [SerializeField] List<GameObject> projectilePool;
        [SerializeField] int projectilePoolSize = 3;
        GameObject projectilePoolHolder;
        float shootInterval;
        float shootTimer = 0f;
        float projectileLifetime = 10f;

        void Awake()
        {
            waveSpawner = FindObjectOfType<WaveSpawner>();
        }

        void Start()
        {
            waveConfig = waveSpawner.currentWave;
            waypoints = waveConfig.GetWaypoints();
            transform.position = waypoints[waypointIndex].position;
            shootInterval = waveConfig.shootInterval;
        }

        private void OnEnable()
        {
            projectilePoolHolder = new GameObject("ProjectilePool");

            // Add projectile prefabs to the pool
            projectilePool = new List<GameObject>();
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
            if (shootTimer >= shootInterval)
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
                float movementThisFrame = waveConfig.moveSpeed * Time.deltaTime;
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

        void Shoot()
        {
            // Find an inactive projectile in the pool
            bool foundInactiveProjectile = false;
            for (int i = 0; i < projectilePool.Count; i++)
            {
                if (!projectilePool[i].activeInHierarchy)
                {
                    projectilePool[i].transform.position = transform.position;
                    projectilePool[i].transform.rotation = Quaternion.Euler(90, 0, 0);
                    projectilePool[i].SetActive(true);
                    projectilePool[i].GetComponent<Rigidbody>().velocity = transform.forward * waveConfig.shootSpeed;
                    StartCoroutine(DeactivateProjectile(projectilePool[i]));
                    foundInactiveProjectile = true;
                    break;
                }
            }

            // If no inactive projectile is found, add a new one to the pool
            if (!foundInactiveProjectile)
            {
                GameObject newProjectile = SpawnProjectile();
                projectilePool.Add(newProjectile);
                newProjectile.GetComponent<Rigidbody>().velocity = transform.forward * waveConfig.shootSpeed;
                StartCoroutine(DeactivateProjectile(newProjectile));
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
