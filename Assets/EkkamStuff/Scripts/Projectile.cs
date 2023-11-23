using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
public class Projectile : MonoBehaviour
    {
        [SerializeField] Collider projectileCollider;
        public float projectileDamage;
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
                other.GetComponent<Enemy>().TakeDamage(projectileDamage);
                gameObject.SetActive(false);
            }
            else if ((other.CompareTag("Player") || other.CompareTag("PlayerDuo")) && this.gameObject.CompareTag("EnemyProjectile")) {
                print("Player hit");
                other.GetComponent<Player>().TakeDamage(projectileDamage);
                gameObject.SetActive(false); 
            }
        }
    }
}
