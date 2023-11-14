using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Ekkam {
    public class Player : MonoBehaviour
    {
        public int playerNumber;

        [SerializeField] Mesh[] playerMeshes; // 0 = player 1, 1 = player 2
        [SerializeField] GameObject playerCanvas;
        [SerializeField] GameObject playerCards;
        [SerializeField] GameObject mainCanvas;
        public Slider healthBar;

        public Player playerDuo;

        Rigidbody rb;
        Collider col;
        ShootingManager shootingManager;
        UpgradeManager upgradeManager;
        MultiplayerEventSystem eventSystem;
        Vector2 input;

        public bool allowMovement = true;
        public bool inDuoMode = false;
        public bool cardPicked = false;

        float shootTimer = 0f;
        float dodgeTimer = 0f;

        [HideInInspector]
        public float maxHealth;

        [Header("----- Player Stats -----")]
        [Tooltip("The total health")] public float health = 5f; // Still need to implement
        public float moveSpeed = 20f;
        public float maxSpeed = 25f;
        public float dodgeSpeed = 1f;
        [Tooltip("The cooldown time before the player can shoot again")] public float attackSpeed = 0.05f;
        [Tooltip("The cooldown time before the player can dodge again")] public float dodgeCooldown = 1f;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            shootingManager = GetComponent<ShootingManager>();
            upgradeManager = FindObjectOfType<UpgradeManager>();
            eventSystem = GetComponentInChildren<MultiplayerEventSystem>();
            maxHealth = health;
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }

        void OnEnable()
        {
            if (GetComponent<PlayerInput>() != null)
            {
                GameManager.OnCombinePlayers += EnableDuoMode;
                GameManager.OnSeperatePlayers += DisableDuoMode;
            }
        }

        void OnDisable()
        {
            if (GetComponent<PlayerInput>() != null)
            {
                GameManager.OnCombinePlayers -= EnableDuoMode;
                GameManager.OnSeperatePlayers -= DisableDuoMode;
            }
        }

        void Start()
        {
            Camera mainCamera = Camera.main;
            RotationConstraint rc = playerCanvas.GetComponent<RotationConstraint>();
            rc.AddSource(new ConstraintSource { sourceTransform = mainCamera.transform, weight = 1 });
        }

        void Update()
        {
            shootTimer += Time.deltaTime;
            dodgeTimer += Time.deltaTime;

            ControlSpeed();

            if (inDuoMode && playerDuo != null)
            {
                transform.position = playerDuo.transform.position;
                if (playerNumber == 1)
                {
                    playerDuo.input = input;
                }
            }
        }

        private void FixedUpdate()
        {
            Move();
        }

        void Move()
        {
            if (!allowMovement) return;
            Vector3 movement = new Vector3(input.x, 0, input.y);
            rb.AddForce(movement * moveSpeed * 100);
        }

        void ControlSpeed()
        {
            // Limit velocity if needed
            Vector3 flatVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if (flatVelocity.magnitude > maxSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;
                rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            input = context.ReadValue<Vector2>();
        }

        public void OnShoot(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (shootTimer < attackSpeed) return;
                shootTimer = 0f;
                if (inDuoMode && playerDuo != null)
                {
                    if (playerNumber == 2)
                    {
                        playerDuo.shootingManager.Shoot("PlayerProjectile");
                    }
                }
                else
                {
                    shootingManager.Shoot("PlayerProjectile");
                }
            }
        }

        public void OnDodge(InputAction.CallbackContext context)
        {
            // read 1D input, roll left if < 0, right if > 0
            if (context.performed)
            {
                if (dodgeTimer < dodgeCooldown) return;
                dodgeTimer = 0f;
                if (inDuoMode && playerDuo != null)
                {
                    if (playerNumber == 1)
                    {
                        playerDuo.OnDodge(context);
                    }
                }
                else
                {
                    float dodgeDirection = context.ReadValue<float>();
                    if (dodgeDirection < 0)
                    {
                        StartCoroutine(Dodge(-1));
                    }
                    else if (dodgeDirection > 0)
                    {
                        StartCoroutine(Dodge(1));
                    }
                }
            }
        }

        public void EnableDuoMode()
        {
            allowMovement = false;
            playerCanvas.SetActive(false);
            rb.velocity = Vector3.zero;
            col.enabled = false;
            StartCoroutine(FlyTowardsTheOtherPlayer());
        }

        IEnumerator FlyTowardsTheOtherPlayer()
        {
            GameObject targetPlayer = null;
            foreach (PlayerInput playerInput in PlayerInput.all)
            {
                if (playerInput.gameObject != this.gameObject)
                {
                    targetPlayer = playerInput.gameObject;
                }
            }

            float timeout = 0;
            while (Vector3.Distance(transform.position, targetPlayer.transform.position) > 0.15f)
            {
                print("Player " + playerNumber + " flying towards the other player");
                transform.position = Vector3.Lerp(transform.position, targetPlayer.transform.position, Time.deltaTime * 5);
                timeout += Time.deltaTime;
                if (timeout > 5f) break;
                yield return null;
            }

            print("Duo mode enabled for player " + playerNumber);
            inDuoMode = true;
            GetComponent<MeshRenderer>().enabled = false;
        }

        IEnumerator Dodge(int direction)
        {
            allowMovement = false;
            rb.velocity = Vector3.zero;
            // spin the player 360 degrees according to the direction
            float startingRotationZ = transform.rotation.eulerAngles.z;
            float targetRotationZ = startingRotationZ + (-direction * 360);
            float rotationTimer = 0f;
            float rotationDuration = 0.5f;
            while (rotationTimer < rotationDuration)
            {
                rotationTimer += Time.deltaTime;
                float rotationZ = Mathf.Lerp(startingRotationZ, targetRotationZ, rotationTimer / rotationDuration) % 360;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, rotationZ);
                // add a force to the player to move them left or right
                rb.AddForce(new Vector3(direction * dodgeSpeed * 1000, 0, 0));
                yield return null;
            }
            transform.rotation = Quaternion.identity;
            
            yield return new WaitForSeconds(0.2f);
            transform.rotation = Quaternion.identity;
            allowMovement = true;
        }

        public void DisableDuoMode()
        {
            inDuoMode = false;
            allowMovement = true;
            playerCanvas.SetActive(true);

            print("Duo mode disabled for player " + playerNumber);
            GetComponent<MeshRenderer>().enabled = true;
            col.enabled = true;

            // Add a force to the player to separate them, player 1 to the left, player 2 to the right
            ApplySeparationForce();
        }

        public void AssignMesh()
        {
            GetComponent<MeshFilter>().mesh = playerMeshes[playerNumber - 1];
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            healthBar.value = health;
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }

        public void ApplySeparationForce()
        {
            rb = GetComponent<Rigidbody>(); // This is needed because the rb is not assigned yet in the Awake() function
            Vector3 forceDirection = Vector3.zero;
            if (playerNumber == 1)
            {
                forceDirection = Vector3.left;
            }
            else if (playerNumber == 2)
            {
                forceDirection = Vector3.right;
            }
            rb.AddForce(forceDirection * 7000);
        }

        public void AddCardsToUpgradeMenu()
        {
            GameObject newPlayerCards = Instantiate(playerCards, upgradeManager.upgradeMenu.transform);
            newPlayerCards.name = "Player" + playerNumber + "Cards";
            if (playerNumber == 1)
            {
                newPlayerCards.transform.SetAsFirstSibling();
            }
            foreach (Transform child in newPlayerCards.transform)
            {
                if (child.name == "SelectText")
                {
                    child.GetComponent<TMP_Text>().text = "Player " + playerNumber + " - Select a Card";
                }
            }
            eventSystem.playerRoot = newPlayerCards;
            for (int i = 0; i < newPlayerCards.transform.childCount; i++)
            {
                if (newPlayerCards.transform.GetChild(i).GetComponent<Card>() == null) continue;
                Card card = newPlayerCards.transform.GetChild(i).GetComponent<Card>();
                if (i == 0) {
                    eventSystem.firstSelectedGameObject = card.gameObject;
                    eventSystem.SetSelectedGameObject(card.gameObject);
                }
                card.ownerPlayer = this;
                if (playerNumber == 1)
                {
                    upgradeManager.player1Cards.Add(card);
                }
                else if (playerNumber == 2)
                {
                    upgradeManager.player2Cards.Add(card);
                }
            }
        }

        public void Upgrade(string upgradeName)
        {
            switch (upgradeName)
            {
                /* Damage 
                Increases Your Projectile damage

                Projectile size 
                Increases Your Projectile size 


                Projectile speed
                Increases Your Projectile 

                Defense 
                Increase Your Defense

                Extra dash
                Adds another dash to your 

                Lightning
                Adds another weapon to your ship

                Shotgun 
                Adds another weapon to your ship

                Diagonal shots 
                You shoot diagonally as well

                Backshots 
                You shoot backwards 

                Multishot
                Adds another Projectile
                
                Burst
                Shoot another Projectile at once
                */
                case "Damage":
                    shootingManager.projectileDamage += 1;
                    break;
                case "Projectile Size":
                    shootingManager.projectileSize += 0.5f;
                    break;
                case "Projectile Speed":
                    shootingManager.projectileSpeed += 10f;
                    break;
                case "Defense":
                    health += 1;
                    break;
                case "Extra Dash":
                    print("Extra Dash"); // TODO: Implement
                    break;
                case "Lightning":
                    print("Lightning Weapon"); // TODO: Implement
                    break;
                case "Shotgun":
                    print("Shotgun Weapon"); // TODO: Implement
                    break;
                case "Diagonal Shots":
                    print("Diagonal Shots"); // TODO: Implement
                    break;
                case "Backshots":
                    print("Backshots"); // TODO: Implement
                    break;
                case "Multishot":
                    shootingManager.multishotCount += 1;
                    break;
                case "Burst":
                    shootingManager.burstFireCount += 1;
                    break;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Enemy"))
            {
                print("Player collided with enemy");
                TakeDamage(other.GetComponent<Enemy>().damageOnImpact);
            }
        }
    }
}
