using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Ekkam {
    public class ShootingManager : MonoBehaviour
    {
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] List<GameObject> projectilePool;
        int projectilePoolSize = 3;
        float projectileLifetime = 10f;
        Vector3 originalProjectileScale;
        GameObject projectilePoolHolder;

        [Header("----- Projectile Stats -----")]

        [Tooltip("The speed of the projectile")] public float projectileSpeed = 90f;
        [Tooltip("The damage of the projectile")] public float projectileDamage = 1f;
        [Tooltip("The size of the projectile")] public float projectileSize = 1f;

        [Tooltip("The amount of projectiles fired side by side")] public int multishotCount = 1;
        [Tooltip("The gap between projectiles when multishot is more than 1")] public float multishotGapX = 0.5f;

        [Tooltip("The amount of projectiles fired per shot")] public int burstFireCount = 1;
        [Tooltip("The delay between each bullet being fired in burst")] public float burstFireDelay = 0.1f;

        private void OnEnable()
        {
            projectilePoolHolder = new GameObject(gameObject.name + "_ProjectilePool");

            // Add projectile prefabs to the pool
            projectilePool = new List<GameObject>();

            // This formula comes pretty close to the actual amount of projectiles needed in the pool
            projectilePoolSize = burstFireCount * multishotCount * ((int)projectileSpeed / 10) * (int)projectileLifetime;

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

        private void Awake()
        {
            originalProjectileScale = projectilePrefab.transform.localScale;
        }

        async public void Shoot(string tagToApply)
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
                            projectilePool[k].tag = tagToApply;
                            projectilePool[k].GetComponent<Projectile>().projectileDamage = projectileDamage;
                            projectilePool[k].transform.rotation = Quaternion.Euler(90, 0, 0);
                            projectilePool[k].transform.localScale = originalProjectileScale * projectileSize;
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
                        newProjectile.tag = tagToApply;
                        newProjectile.GetComponent<Projectile>().projectileDamage = projectileDamage;
                        newProjectile.transform.rotation = Quaternion.Euler(90, 0, 0);
                        newProjectile.transform.localScale = originalProjectileScale * projectileSize;
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
