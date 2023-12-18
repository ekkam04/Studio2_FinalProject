using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Animations;

namespace Ekkam {
    public class DamagableEntity : MonoBehaviour
    {
        public GameObject damagePopupPrefab;
        public Slider healthBar;
        public GameObject entityCanvas;
        [Tooltip("The total health")] public float health;
        public bool isInvincible = false;

        public bool hasRegen;
        public float regenAmount;
        public float regenRate;
        float regenTimer;

        public AudioManager audioManager;
        public AudioSource weaponAudioSource;

        public AudioClip hitSound;

        [HideInInspector]
        public float maxHealth;

        [HideInInspector]
        public MeshRenderer meshRenderer;

        [HideInInspector]
        public DamagableEntity killer;

        public void InitializeDamagableEntity()
        {
            maxHealth = health;
            if (healthBar != null)
            {
                print("health bar found");
                healthBar.maxValue = maxHealth;
                healthBar.value = health;

                Camera mainCamera = Camera.main;
                RotationConstraint canvasRC = entityCanvas.GetComponent<RotationConstraint>();
                canvasRC.AddSource(new ConstraintSource { sourceTransform = mainCamera.transform, weight = 1 });
            }
            else
            {
                print("health bar not found");
            }
            audioManager = FindObjectOfType<AudioManager>();
        }

        public void TakeDamage(float damage, bool isCriticalHit, DamagableEntity attacker)
        {
            OnDamageTaken();
            if (!isInvincible) health -= damage;
            ShowDamagePopup(damage, isCriticalHit);
            audioManager.PlayHitSound();
            if (GetComponent<PlayerInput>() != null) RumbleManager.instance.RumblePulse(GetComponent<PlayerInput>().devices[0] as Gamepad, 0.5f, 0.5f, 0.1f);
            if (healthBar != null) healthBar.value = health;
            if (health <= 0) // When dead
            {
                health = 0;
                killer = attacker;

                if (GetComponent<Player>() != null) // if a player
                {
                    if (GetComponent<PlayerInput>() == null) // When combined player dies, both players die
                    {
                        GameManager.instance.EndGame();
                        foreach (Player player in FindObjectsOfType<Player>())
                        {
                            if (player != this)
                            {
                                Destroy(player.gameObject);
                            }
                        }
                        Destroy(gameObject);
                    }
                    else if (PlayerInput.all.Count > 1)
                    {
                        int playersAlive = 0;
                        foreach (Player player in FindObjectsOfType<Player>())
                        {
                            if (player.isDBNO == false) playersAlive++;
                        }

                        if (playersAlive > 1)
                        {
                            GetComponent<Player>().EnterDBNOState();
                        }
                        else
                        {
                            GameManager.instance.EndGame();
                            foreach (Player player in FindObjectsOfType<Player>())
                            {
                                if (player != this)
                                {
                                    Destroy(player.gameObject);
                                }
                            }
                            Destroy(gameObject);
                        }
                    }
                    else
                    {
                        GameManager.instance.EndGame();
                        Destroy(gameObject);
                    }
                }
                else // if not a player
                {
                    Destroy(gameObject);
                }

            }
            else
            {
                StartCoroutine(FlashColor(Color.red, 0.1f));
            }
        }

        public void Heal(float amount)
        {
            health += amount;
            if (health > maxHealth) health = maxHealth;
            if (healthBar != null) healthBar.value = health;
        }

        public virtual void OnDamageTaken()
        {
            // this function is meant to be overridden
        }

        public void RegenerateHealth()
        {
            if (hasRegen)
            {
                regenTimer += Time.deltaTime;
                if (regenTimer >= regenRate)
                {
                    regenTimer = 0f;
                    health += regenAmount;
                    if (health > maxHealth) health = maxHealth;
                    if (healthBar != null) healthBar.value = health;
                }
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
                if (player == this || player.isDBNO) continue;
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