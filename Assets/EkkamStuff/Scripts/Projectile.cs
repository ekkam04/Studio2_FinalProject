using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
public class Projectile : MonoBehaviour
    {
        [SerializeField] Collider projectileCollider;

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Enemy") && this.gameObject.CompareTag("PlayerProjectile")) {
                print("Enemy hit");
                other.GetComponent<Enemy>().TakeDamage(50f);
                // Destroy(gameObject);
            }
            else if (other.CompareTag("Player") && this.gameObject.CompareTag("EnemyProjectile")) {
                print("Player hit");
                other.GetComponent<Player>().TakeDamage(50f);
                // Destroy(gameObject);
            }
        }
    }
}
