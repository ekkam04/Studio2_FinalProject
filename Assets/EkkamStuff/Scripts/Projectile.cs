using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
public class Projectile : MonoBehaviour
    {
        [SerializeField] Collider projectileCollider;
        public float projectileDamage;
        public bool isCriticalHit = false;
        Vector3 viewportPosition;

        void Update()
        {
            viewportPosition = Camera.main.WorldToViewportPoint(transform.position);
            if (viewportPosition.x > 1 || viewportPosition.x < 0 || viewportPosition.y > 1 || viewportPosition.y < 0)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Enemy") && this.gameObject.CompareTag("PlayerProjectile")) {
                print("Enemy hit");
                other.GetComponent<Enemy>().TakeDamage(projectileDamage, isCriticalHit);
                gameObject.SetActive(false);
            }
            else if ((other.CompareTag("Player") || other.CompareTag("PlayerDuo")) && this.gameObject.CompareTag("EnemyProjectile")) {
                print("Player hit");
                other.GetComponent<Player>().TakeDamage(projectileDamage, isCriticalHit);
                gameObject.SetActive(false); 
            }
        }

        public void SetDamageWithCritChance(float damage, float critChance, float critMultiplier)
        {
            projectileDamage = damage;
            float critRoll = Random.Range(0f, 100f);
            if (critRoll <= critChance)
            {
                isCriticalHit = true;
                projectileDamage *= critMultiplier;
            }
            else
            {
                isCriticalHit = false;
            }
        
        }
    }
}
