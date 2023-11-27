using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam {
    public class DamagableEntity : MonoBehaviour
    {
        public GameObject damagePopupPrefab;
        public Slider healthBar;
        [Tooltip("The total health")] public float health;

        [HideInInspector]
        public float maxHealth;

        [HideInInspector]
        public MeshRenderer meshRenderer;

        [HideInInspector]
        public DamagableEntity killer;

        public void InitializeHealth()
        {
            maxHealth = health;
            if (healthBar != null)
            {
                print("health bar found");
                healthBar.maxValue = maxHealth;
                healthBar.value = health;
            }
            else
            {
                print("health bar not found");
            }
        }

        public void TakeDamage(float damage, bool isCriticalHit, DamagableEntity attacker)
        {
            health -= damage;
            ShowDamagePopup(damage, isCriticalHit);
            if (healthBar != null) healthBar.value = health;
            if (health <= 0)
            {
                killer = attacker;
                Destroy(gameObject);
            }
            else
            {
                StartCoroutine(FlashColor(Color.red, 0.1f));
            }
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

        void ShowDamagePopup(float damage, bool isCriticalHit)
        {
            if (damagePopupPrefab == null) return;
            GameObject damagePopup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            damagePopup.GetComponent<DamagePopup>().SetDamageText(damage, isCriticalHit);
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
    }
}