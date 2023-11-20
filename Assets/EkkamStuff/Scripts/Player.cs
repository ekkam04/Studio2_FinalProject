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
        [SerializeField] GameObject crosshair;
        public Slider healthBar;

        public Player playerDuo;

        Rigidbody rb;
        Collider col;
        PlayerInput playerInput;
        ShootingManager shootingManager;
        UpgradeManager upgradeManager;
        UIManager uiManager;
        MultiplayerEventSystem eventSystem;
        Vector2 movementInput;
        Vector2 aimInput;
        float rightTriggerAxis;

        public bool allowMovement = true;
        public bool inDuoMode = false;
        public bool cardPicked = false;

        float shootTimer = 0f;
        float dodgeTimer = 0f;
        float onMoveTimer = 0f;

        [HideInInspector]
        public float maxHealth;

        [Header("----- Player Stats -----")]
        [Tooltip("The total health")] public float health = 5f;
        public AnimationCurve speedCurve;
        public float moveSpeed = 20f;
        public float maxSpeed = 25f;
        public float dodgeSpeed = 1f;
        [Tooltip("The cooldown time before the player can shoot again")] public float attackSpeed = 0.05f;
        [Tooltip("The cooldown time before the player can dodge again")] public float dodgeCooldown = 1f;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            playerInput = GetComponent<PlayerInput>();
            shootingManager = GetComponent<ShootingManager>();
            upgradeManager = FindObjectOfType<UpgradeManager>();
            uiManager = FindObjectOfType<UIManager>();
            eventSystem = GetComponentInChildren<MultiplayerEventSystem>();
            eventSystem.SetSelectedGameObject(null);
            maxHealth = health;
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }

        void OnDestroy()
        {
            if (playerInput == null)
            {
                crosshair.SetActive(false);
            }
        }

        void OnEnable()
        {
            if (playerInput != null)
            {
                GameManager.OnCombinePlayers += EnableDuoMode;
                GameManager.OnSeperatePlayers += DisableDuoMode;
            }
        }

        void OnDisable()
        {
            if (playerInput != null)
            {
                GameManager.OnCombinePlayers -= EnableDuoMode;
                GameManager.OnSeperatePlayers -= DisableDuoMode;
            }
        }

        void Start()
        {
            Camera mainCamera = Camera.main;
            RotationConstraint playerCanvasRC = playerCanvas.GetComponent<RotationConstraint>();
            playerCanvasRC.AddSource(new ConstraintSource { sourceTransform = mainCamera.transform, weight = 1 });

            if (playerInput == null)
            {
                crosshair.SetActive(true);
                RotationConstraint crosshairRC = crosshair.GetComponent<RotationConstraint>();
                crosshairRC.AddSource(new ConstraintSource { sourceTransform = mainCamera.transform, weight = 1 });
            } 
        }

        void Update()
        {
            shootTimer += Time.deltaTime;
            dodgeTimer += Time.deltaTime;

            ControlSpeed();
            Aim();

            if (inDuoMode && playerDuo != null)
            {
                transform.position = playerDuo.transform.position;
                if (playerNumber == 1)
                {
                    playerDuo.movementInput = movementInput;
                }
                else if (playerNumber == 2)
                {
                    playerDuo.aimInput = aimInput;
                }   
            }

            if (rightTriggerAxis > 0.5f)
            {
                Shoot();
            }

            if (movementInput == Vector2.zero)
            {
                onMoveTimer = 0f;
            }
        }

        private void FixedUpdate()
        {
            Move();
        }

        void Move()
        {
            if (!allowMovement) return;
            onMoveTimer += Time.deltaTime;
            Vector3 movement = new Vector3(movementInput.x, 0, movementInput.y);
            rb.AddForce(movement * moveSpeed * 100 * speedCurve.Evaluate(onMoveTimer));
        }

        void Aim()
        {
            if (crosshair == null) return;
            Vector3 aimDirection = new Vector3(aimInput.x, 0, aimInput.y);
            if (aimDirection.magnitude > 0.1f)
            {
                crosshair.transform.localPosition = aimDirection.normalized * 60;
            }
        }

        void Shoot()
        {
            if (shootTimer < attackSpeed) return;
            shootTimer = 0f;
            if (inDuoMode && playerDuo != null)
            {
                if (playerNumber == 2)
                {
                    playerDuo.shootingManager.Shoot("PlayerProjectile", this.gameObject, playerDuo.crosshair.transform);
                }
            }
            else
            {
                shootingManager.Shoot("PlayerProjectile", this.gameObject);
            }
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
            movementInput = context.ReadValue<Vector2>();
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            aimInput = context.ReadValue<Vector2>();
        }

        public void OnShoot(InputAction.CallbackContext context)
        {
            rightTriggerAxis = context.ReadValue<float>();
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
            foreach (PlayerInput pi in PlayerInput.all)
            {
                if (pi.gameObject != this.gameObject)
                {
                    targetPlayer = pi.gameObject;
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
            if (gameObject.tag == "PlayerDuo")
            {
                cardPicked = true;
                return;
            }
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
                    card.gameObject.GetComponent<Button>().OnSelect(null);
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
                case "Damage":
                    shootingManager.projectileDamage += 1;
                    break;
                case "Projectile Size":
                    shootingManager.projectileSize += 0.5f;
                    break;
                case "Projectile Speed":
                    shootingManager.projectileSpeed += 10f;
                    break;
                case "Health":
                    maxHealth += 3;
                    health += 3;
                    healthBar.maxValue = maxHealth;
                    healthBar.value = health;
                    break;
                case "Lightning":
                    shootingManager.shootLightning = true;
                    break;
                case "Backshots":
                    shootingManager.shootBackShots = true;
                    break;
                case "Multishot":
                    shootingManager.multishotCount += 1;
                    break;
                case "Burst":
                    shootingManager.burstFireCount += 1;
                    break;
                case "Fire Rate":
                    attackSpeed -= 0.05f;
                    break;
                default:
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
