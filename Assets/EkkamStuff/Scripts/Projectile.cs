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
        [SerializeField] ParticleSystem projectileParticleSystem;
        [SerializeField] ParticleSystem[] projectileSubEmitters;

        public DamagableEntity projectileOwner; // Since only damagable entities can shoot projectiles, this will always be a damagable entity
        public ShootingManager shootingManager;

        void Update()
        {
            if (projectileParticleSystem.particleCount > 0)
            {
                ParticleSystem.Particle[] particles = new ParticleSystem.Particle[projectileParticleSystem.particleCount];
                projectileParticleSystem.GetParticles(particles);
                for (int i = 0; i < particles.Length; i++)
                {
                    viewportPosition = Camera.main.WorldToViewportPoint(particles[i].position);
                    if (viewportPosition.x > 1 || viewportPosition.x < 0 || viewportPosition.y > 1 || viewportPosition.y < 0)
                    {
                        particles[i].remainingLifetime = 0.1f;
                    }
                }
            }
            else
            {
                shootingManager.DeactivateProjectile(this.gameObject);
            }

        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Enemy") && this.gameObject.CompareTag("PlayerProjectile")) {
                print("Enemy hit");
                other.GetComponent<Enemy>().TakeDamage(projectileDamage, isCriticalHit, projectileOwner);
                shootingManager.DeactivateProjectile(this.gameObject);
            }
            else if ((other.CompareTag("Player") || other.CompareTag("PlayerDuo")) && this.gameObject.CompareTag("EnemyProjectile")) {
                print("Player hit");
                other.GetComponent<Player>().TakeDamage(projectileDamage, isCriticalHit, projectileOwner);
                shootingManager.DeactivateProjectile(this.gameObject); 
            }
        }

        private void OnParticleCollision(GameObject other) {
            print("Particle collision");
            print(other.tag);
            if (other.CompareTag("Enemy") && this.gameObject.CompareTag("PlayerProjectile")) {
                print("Enemy hit");
                other.GetComponent<Enemy>().TakeDamage(projectileDamage, isCriticalHit, projectileOwner);
                shootingManager.DeactivateProjectile(this.gameObject);
            }
            else if ((other.CompareTag("Player") || other.CompareTag("PlayerDuo")) && this.gameObject.CompareTag("EnemyProjectile")) {
                print("Player hit");
                other.GetComponent<Player>().TakeDamage(projectileDamage, isCriticalHit, projectileOwner);
                shootingManager.DeactivateProjectile(this.gameObject);
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

        public void UpdateSubEmitterSpeed(float speed)
        {
            for (int i = 0; i < projectileSubEmitters.Length; i++)
            {
                ParticleSystem.MainModule mainModule = projectileSubEmitters[i].main;
                mainModule.startSpeed = speed;
            }
        }
    }
}
