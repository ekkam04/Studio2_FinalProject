using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters;
using UnityEngine.InputSystem;

namespace Ekkam {
    public class ShootingManager : MonoBehaviour
    {
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] List<GameObject> projectilePool;
        int projectilePoolSize = 3;
        float projectileLifetime = 15f;
        Vector3 originalProjectileScale;
        GameObject projectilePoolHolder;

        bool lightningActive = false;
        bool playerInDuoMode = false;

        int projectileCountBeforeLightning = 0;

        GameObject crosshair;

        [Header("----- Projectile Stats -----")]

        public bool shootProjectile = true;
        public bool shootBackShots = false;

        [Tooltip("The speed of the projectile")] public float projectileSpeed = 90f;
        [Tooltip("The damage of the projectile")] public float projectileDamage = 1f;
        [Tooltip("The size of the projectile")] public float projectileSize = 1f;

        [Tooltip("The amount of projectiles fired side by side")] public int multishotCount = 1;
        [Tooltip("The gap between projectiles when multishot is more than 1")] public float multishotGapX = 0.5f;
        public float multishotArcAngle = 45f;
        public bool skipMiddleProjectile = false;


        [Tooltip("The amount of projectiles fired per shot")] public int burstFireCount = 1;
        [Tooltip("The delay between each bullet being fired in burst")] public float burstFireDelay = 0.1f;

        [Header("----- Lightning Stats -----")]

        public bool shootLightning = false;
        public bool chainLightning = false;

        [Tooltip("The amount of enemies the lightning can chain to")] public int chainLightningCount = 3;
        [Tooltip("The amount of enemies the lightning chains to at once")] public int chainLightingAtOnce = 1;
        [Tooltip("The amount of damage dealt to each enemy hit by the lightning")] public float lightningDamage = 0.25f;
        public int shootLightningEveryXShots = 5;


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
            if (GetComponent<Player>() != null && GetComponent<PlayerInput>() == null)
            {
                playerInDuoMode = true;
                crosshair = GetComponent<Player>().crosshair;
            }
        }

        public void Shoot(string tagToApply, GameObject owner, Transform target = null)
        {
            projectileCountBeforeLightning++;
            if (target != null)
            {
                if (shootProjectile) ShootProjectile(tagToApply, (target.position - owner.transform.position).normalized);
                if (shootBackShots) ShootProjectile(tagToApply, (owner.transform.position - target.position).normalized);
            }
            else
            {
                if (shootProjectile) ShootProjectile(tagToApply, transform.forward);
                if (shootBackShots) ShootProjectile(tagToApply, -transform.forward);
            }

            if (shootLightning && projectileCountBeforeLightning >= shootLightningEveryXShots)
            {
                projectileCountBeforeLightning = 0;
                Enemy closestEnemy = GetComponent<Player>().FindClosestEnemy();
                if (closestEnemy != null) ShootLightning(tagToApply, owner, closestEnemy.gameObject);
            }
        }

        async private void ShootProjectile(string tagToApply, Vector3 direction)
        {
            for (int i = 0; i < burstFireCount; i++)
            {
                float bulletGapX = 0;
                if (multishotCount > 1)
                {
                    bulletGapX = -(multishotGapX * (multishotCount - 1) / 2);
                }
                int halfwayPoint = (int)(multishotCount / 2);

                for (int j = 0; j < multishotCount; j++)
                {
                    if (j == halfwayPoint && skipMiddleProjectile)
                    {
                        continue;
                    }
                    float angleOffset = CalculateAngleOffset(j, multishotCount, multishotArcAngle);
                    Vector3 adjustedDirection = Quaternion.Euler(0f, angleOffset, 0f) * direction;
                    // Find an inactive projectile in the pool
                    bool foundInactiveProjectile = false;
                    for (int k = 0; k < projectilePool.Count; k++)
                    {
                        if (!projectilePool[k].activeInHierarchy)
                        {
                            print("Reusing projectile from pool");
                            projectilePool[k].tag = tagToApply;
                            projectilePool[k].GetComponent<Projectile>().projectileDamage = projectileDamage;
                            projectilePool[k].transform.rotation = Quaternion.LookRotation(adjustedDirection);
                            projectilePool[k].transform.eulerAngles = new Vector3(90, projectilePool[k].transform.eulerAngles.y, projectilePool[k].transform.eulerAngles.z);
                            projectilePool[k].transform.position = transform.position + (projectilePool[k].transform.right * bulletGapX);
                            projectilePool[k].transform.localScale = originalProjectileScale * projectileSize;
                            projectilePool[k].SetActive(true);
                            projectilePool[k].GetComponent<Rigidbody>().velocity = adjustedDirection * projectileSpeed;
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
                        newProjectile.tag = tagToApply;
                        newProjectile.GetComponent<Projectile>().projectileDamage = projectileDamage;
                        newProjectile.transform.rotation = Quaternion.LookRotation(adjustedDirection);
                        newProjectile.transform.eulerAngles = new Vector3(90, newProjectile.transform.eulerAngles.y, newProjectile.transform.eulerAngles.z);
                        newProjectile.transform.position = transform.position + (newProjectile.transform.right * bulletGapX);
                        newProjectile.transform.localScale = originalProjectileScale * projectileSize;
                        newProjectile.GetComponent<Rigidbody>().velocity = adjustedDirection * projectileSpeed;
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
                GetProjectileRotation(),
                projectilePoolHolder.transform
            );
            projectilePool.Add(newProjectile);
            return newProjectile;
        }

        Quaternion GetProjectileRotation()
        {
            if (playerInDuoMode)
            {
                return Quaternion.Euler(90, crosshair.transform.rotation.eulerAngles.y, 0);
            }
            else
            {
                return Quaternion.Euler(90, 0, 0);
            }
        }

        private float CalculateAngleOffset(int index, int totalShots, float arcAngle)
        {
            float totalAngle = (totalShots - 1) * arcAngle;
            float halfTotalAngle = totalAngle / 2f;

            float currentAngle = -halfTotalAngle + index * arcAngle;
            return currentAngle;
        }

        IEnumerator DeactivateProjectile(GameObject projectile)
        {
            yield return new WaitForSeconds(projectileLifetime);
            projectile.SetActive(false);
            print("Projectile deactivated");
        }

        async void ShootLightning(string tagToApply, GameObject transmitter, GameObject reciever)
        {
            // Goal: spawn a lightning between the transmitter and the reciever
            if (lightningActive) return;
            if (reciever != null)
            {
                lightningActive = true;
                LineRenderer lightningLineRenderer = CreateLightning(tagToApply);
                lightningLineRenderer.SetPosition(0, transmitter.transform.position);
                lightningLineRenderer.SetPosition(1, reciever.transform.position);
                
                await Task.Delay(100);
                Destroy(lightningLineRenderer.gameObject);
                if (reciever == null) return;
                reciever.GetComponent<Enemy>().TakeDamage(lightningDamage);

                // If the lightning is a chain lightning, chain it to other enemies
                if (chainLightning)
                {
                    List<Enemy> enemiesHit = new List<Enemy>();
                    enemiesHit.Add(reciever.GetComponent<Enemy>());
                    for (int i = 0; i < chainLightningCount; i++)
                    {
                        Enemy closestEnemy = null;
                        float closestDistance = Mathf.Infinity;
                        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                        {
                            if (enemiesHit.Contains(enemy)) continue;
                            if (reciever == null) break;
                            float distance = Vector3.Distance(reciever.transform.position, enemy.transform.position);
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestEnemy = enemy;
                            }
                        }
                        if (closestEnemy != null)
                        {
                            enemiesHit.Add(closestEnemy);
                            LineRenderer newLightning = CreateLightning(tagToApply);
                            newLightning.SetPosition(0, reciever.transform.position);
                            newLightning.SetPosition(1, closestEnemy.transform.position);
                            await Task.Delay(100);
                            Destroy(newLightning.gameObject);
                            closestEnemy.TakeDamage(lightningDamage);
                            if (enemiesHit.Count == chainLightingAtOnce) break;
                        }
                    }
                }
            }
            lightningActive = false;
        }

        LineRenderer CreateLightning(string tagToApply      )
        {
            LineRenderer newLightning = new GameObject(gameObject.name + "_Lightning").AddComponent<LineRenderer>();
            newLightning.startWidth = 1f;
            newLightning.endWidth = 1f;
            newLightning.material = new Material(Shader.Find("Sprites/Default"));  
            newLightning.startColor = Color.white;    
            newLightning.endColor = Color.blue;
            newLightning.gameObject.tag = tagToApply;
            newLightning.gameObject.SetActive(true);
            return newLightning;
        }
    }
}
