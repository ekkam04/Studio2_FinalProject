using System;
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
    public class Player : DamagableEntity
    {
        public int playerNumber;

        [SerializeField] Material[] playerMaterials;
        [SerializeField] Color32[] playerSilhouetteColors;
        [SerializeField] GameObject[] playerParts;
        [SerializeField] GameObject[] playerGlassParts;
        // [SerializeField] GameObject playerCanvas;
        [SerializeField] GameObject playerCards;
        [SerializeField] GameObject playerSilhouette;
        [SerializeField] AudioSource playerAudioSource;
        [SerializeField] GameObject shield;
        [SerializeField] ParticleSystem shieldParticles;
        [SerializeField] Slider shieldSlider;
        public GameObject crosshair;

        public Player playerDuo;

        public Rigidbody rb;
        Collider col;
        PlayerInput playerInput;
        ShootingManager shootingManager;
        UpgradeManager upgradeManager;
        MultiplayerEventSystem eventSystem;
        Vector2 movementInput;
        Vector2 aimInput;
        Vector3 viewportPosition;
        Vector3 repellingForceDirection;
        Vector3 lastRepellingForceDirection;
        Vector3 lastMovementInputBeforeDash;
        Vector3 dualsenseGyroDirection;
        float rightTriggerAxis;
        float leftTriggerAxis;
        bool hasDualsense = false;

        public bool allowMovement = true;
        public bool allowDashing = true;
        public bool allowShooting = true;
        public bool inDuoMode = false;
        bool isDashing = false;
        public bool cardPicked = false;

        bool lockLeftMovement = false;
        bool lockRightMovement = false;
        bool lockUpMovement = false;
        bool lockDownMovement = false;

        float shootTimer = 0f;
        float silhouetteTimer = 0f;
        float onMoveTimer = 0f;
        float onDashTimer = 0f;
        float lockMovementTimer = 0f;
        float shieldTimer = 0f;

        public enum DashDirection
        {
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left,
            TopLeft
        }
        public DashDirection dashDirection;
        public DashDirection lastDashDirection;

        public int killCount = 0;

        [Header("----- Player Stats -----")]
        public AnimationCurve speedCurve;
        public AnimationCurve dashCurve;
        public float moveSpeed = 20f;
        public float maxSpeed = 25f;
        public float dodgeSpeed = 1f;
        public float dashSpeed = 2f;
        [Tooltip("The cooldown time before the player can shoot again")] public float attackSpeed = 0.05f;
        [Tooltip("The cooldown time before the player can dodge again")] public float dashCooldown = 0.1f;
        public float xpMultiplier = 1f;

        public bool hasShield = false;
        public float shieldDuration = 5f;
        public float shieldCooldown = 2f;

        bool isShieldActive = false;
        float currentShieldDuration = 0f;
        float shieldCooldownTimer = 0f;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            meshRenderer = GetComponent<MeshRenderer>();
            playerInput = GetComponent<PlayerInput>();
            shootingManager = GetComponent<ShootingManager>();
            upgradeManager = FindObjectOfType<UpgradeManager>();
            eventSystem = GetComponentInChildren<MultiplayerEventSystem>();
            eventSystem.SetSelectedGameObject(null);
            InitializeDamagableEntity();
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
            if (playerInput == null)
            {
                crosshair.SetActive(true);
            }
            else
            {
                hasDualsense = Dualsense.TryGettingDualsense(playerInput.devices[0] as Gamepad);
            }
        }

        void Update()
        {
            viewportPosition = Camera.main.WorldToViewportPoint(transform.position);
            repellingForceDirection = GetRepellingDirection();

            lockMovementTimer += Time.deltaTime; 
            shootTimer += Time.deltaTime;
            silhouetteTimer += Time.deltaTime;
            shieldTimer += Time.deltaTime;

            if (hasDualsense == true) dualsenseGyroDirection = Dualsense.GetDirection(4000f * Time.deltaTime, playerInput.devices[0] as Gamepad).normalized;
            // print("dualsense gyro direction: " + dualsenseGyroDirection);

            ConstraintMovement();
            ControlSpeed();
            Aim();

            RegenerateHealth();
            UpdateDashDirection(movementInput);

            if (hasDualsense == true && Dualsense.GetDirection(1000f * Time.deltaTime, playerInput.devices[0] as Gamepad).magnitude > 1f)
            {
                if (allowDashing == true)
                {
                    StartCoroutine(Dash(true));
                }
            }

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

            TiltOnMovement();
            TiltOnDash();

            if (rightTriggerAxis > 0.5f && allowShooting)
            {
                Shoot();
            }

            if (leftTriggerAxis > 0.5f && hasShield)
            {
                ActivateShield();
                allowShooting = false;
            }
            else
            {
                DeactivateShield();
                allowShooting = true;
            }

            UpdateShieldUI();


            if (Input.GetKey(KeyCode.Space) && allowShooting)
            {
                Shoot();
            }

            if (movementInput == Vector2.zero)
            {
                onMoveTimer = 0f;
            }

            if (isDashing)
            {
                if (silhouetteTimer > 0.1f)
                {
                    SpawnPlayerSilhouette();
                    silhouetteTimer = 0f;
                }
            }
        }

        private void FixedUpdate()
        {
            if (repellingForceDirection == Vector3.zero)
            {
                Move();
            }
            else
            {
                lastRepellingForceDirection = repellingForceDirection;
                lockMovementTimer = 0f;
            }

            if (lockMovementTimer < 0.1f)
            {
                rb.AddForce(lastRepellingForceDirection * 1000);
            }

            if (isDashing)
            {
                onDashTimer += Time.deltaTime;
                Vector3 dashDirectionVector = new Vector3(lastMovementInputBeforeDash.x, 0, lastMovementInputBeforeDash.y);
                dashDirectionVector = dashDirectionVector == Vector3.zero ? Vector3.forward : dashDirectionVector;
                rb.AddForce(dashDirectionVector.normalized * dashSpeed * 100 * dashCurve.Evaluate(onDashTimer));
            }
            else
            {
                onDashTimer = 0f;
            }
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
                crosshair.transform.localPosition = aimDirection.normalized * 5;
                // rotate the crosshair to face the direction of the aim but keep the x rotation at 90 degrees
                crosshair.transform.rotation = Quaternion.LookRotation(aimDirection, Vector3.up);
                crosshair.transform.eulerAngles = new Vector3(90, crosshair.transform.eulerAngles.y, crosshair.transform.eulerAngles.z);
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

        void ActivateShield()
        {
            if (!isShieldActive && shieldCooldownTimer <= 0f)
            {
                isShieldActive = true;
                shieldSlider.gameObject.SetActive(true);
                currentShieldDuration = shieldDuration;
                shield.SetActive(true);
                shieldParticles.Play();
            }
        }

        void DeactivateShield()
        {
            if (isShieldActive)
            {
                isShieldActive = false;
                shield.SetActive(false);
                shieldParticles.Stop();
                shieldCooldownTimer = shieldCooldown;
            }
        }

        void UpdateShieldUI()
        {
            if (isShieldActive)
            {
                // Update the shield slider based on the remaining duration.
                currentShieldDuration -= Time.deltaTime;
                shieldSlider.value = currentShieldDuration / shieldDuration;

                if (currentShieldDuration <= 0f)
                {
                    DeactivateShield();
                }
            }
            else if (shieldCooldownTimer > 0f)
            {
                // Update the shield slider during cooldown.
                shieldSlider.gameObject.SetActive(true);
                shieldCooldownTimer -= Time.deltaTime;
                shieldSlider.value = 1 - (shieldCooldownTimer / shieldCooldown);
            }
            else
            {
                shieldSlider.gameObject.SetActive(false);
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

        DashDirection UpdateDashDirection(Vector3 movementInput)
        {
            // update the dash direction depending on the movement input and keep top as the default direction if no input. snap to the closest direction from the 8 directions
            if (movementInput.x > 0.5f)
            {
                if (movementInput.y > 0.5f)
                {
                    dashDirection = DashDirection.TopRight;
                    return DashDirection.TopRight;
                }
                else if (movementInput.y < -0.5f)
                {
                    dashDirection = DashDirection.BottomRight;
                    return DashDirection.BottomRight;
                }
                else
                {
                    dashDirection = DashDirection.Right;
                    return DashDirection.Right;
                }
            }
            else if (movementInput.x < -0.5f)
            {
                if (movementInput.y > 0.5f)
                {
                    dashDirection = DashDirection.TopLeft;
                    return DashDirection.TopLeft;
                }
                else if (movementInput.y < -0.5f)
                {
                    dashDirection = DashDirection.BottomLeft;
                    return DashDirection.BottomLeft;
                }
                else
                {
                    dashDirection = DashDirection.Left;
                    return DashDirection.Left;
                }
            }
            else
            {
                if (movementInput.y < 0f)
                {
                    dashDirection = DashDirection.Bottom;
                    return DashDirection.Bottom;
                }
                else
                {
                    dashDirection = DashDirection.Top;
                    return DashDirection.Top;
                }
            }
        }

        void ConstraintMovement()
        {
            // prevent the player from moving the direction they were moving before they got repelled but allow them to move in other directions
            if (lockMovementTimer < 0.75f)
            {
                if (lastRepellingForceDirection == Vector3.left && movementInput.x > 0) lockRightMovement = true;
                if (lastRepellingForceDirection == Vector3.right && movementInput.x < 0) lockLeftMovement = true;
                if (lastRepellingForceDirection == Vector3.forward && movementInput.y < 0) lockDownMovement = true;
                if (lastRepellingForceDirection == Vector3.back && movementInput.y > 0) lockUpMovement = true;
            }

            // prevent the player from moving in the direction they are moving if they are being repelled until they move in the opposite direction
            if (movementInput.x > 0)
            {
                if (lockRightMovement)
                {
                    movementInput.x = 0;
                }
                else
                {
                    lockLeftMovement = false;
                }
            }
            else if (movementInput.x < 0)
            {
                if (lockLeftMovement)
                {
                    movementInput.x = 0;
                }
                else
                {
                    lockRightMovement = false;
                }
            }
            if (movementInput.y > 0)
            {
                if (lockUpMovement)
                {
                    movementInput.y = 0;
                }
                else
                {
                    lockDownMovement = false;
                }
            }
            else if (movementInput.y < 0)
            {
                if (lockDownMovement)
                {
                    movementInput.y = 0;
                }
                else
                {
                    lockUpMovement = false;
                }
            }

            // unlock movement depending on viewport position
            if (viewportPosition.x > 0.07f)
            {
                lockLeftMovement = false;
            }
            if (viewportPosition.x < 0.93f)
            {
                lockRightMovement = false;
            }
            if (viewportPosition.y > 0.12f)
            {
                lockDownMovement = false;
            }
            if (viewportPosition.y < 0.88f)
            {
                lockUpMovement = false;
            }
        }

        void TiltOnMovement()
        {
            if (Time.timeScale == 0) return;
            // tilt the player depending on the movement input
            if (movementInput.x > 0.1f)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0, -movementInput.x * 10);
            }
            else if (movementInput.x < -0.1f)
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0, -movementInput.x * 10);
            }
            else
            {
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0, 0);
            }
        }

        void TiltOnDash()
        {
            // tilt the player forward or backward while dashing depending on the dash curve
            if (isDashing)
            {
                if (lastDashDirection == DashDirection.Top)
                {
                    transform.eulerAngles = new Vector3(Mathf.Lerp(0, 45, dashCurve.Evaluate(onDashTimer)), transform.eulerAngles.y, transform.eulerAngles.z);
                }
                else if (lastDashDirection == DashDirection.Bottom)
                {
                    transform.eulerAngles = new Vector3(Mathf.Lerp(0, -45, dashCurve.Evaluate(onDashTimer)), transform.eulerAngles.y, transform.eulerAngles.z);
                }
            }
            else
            {
                transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
            }
        }

        Vector3 GetRepellingDirection()
        {
            if (viewportPosition.x < 0f)
            {
                return Vector3.right;
            }
            else if (viewportPosition.x > 1f)
            {
                return Vector3.left;
            }
            if (viewportPosition.y < 0.05f)
            {
                return Vector3.forward;
            }
            else if (viewportPosition.y > 0.95f)
            {
                return Vector3.back;
            }
            return Vector3.zero;
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

        public void OnUtility(InputAction.CallbackContext context)
        {
            leftTriggerAxis = context.ReadValue<float>();
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (!allowDashing) return;
                if (inDuoMode && playerDuo != null)
                {
                    if (playerNumber == 1)
                    {
                        playerDuo.OnDash(context);
                    }
                }
                else
                {
                    StartCoroutine(Dash());
                }
            }
        }

        public void EnableDuoMode()
        {
            allowMovement = false;
            allowDashing = false;
            entityCanvas.SetActive(false);
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
            allowDashing = true;
            HidePlayer();
        }

        IEnumerator Dash(bool usingGyro = false)
        {
            col.enabled = false;
            allowMovement = false;
            allowDashing = false;
            allowShooting = false;
            if (playerInput == null) DisableShootingForAllPlayers();
            if (crosshair != null) crosshair.SetActive(false);

            if (usingGyro == true)
            {
                print("Gyro dash!");
                lastMovementInputBeforeDash = dualsenseGyroDirection; 
                lastDashDirection = UpdateDashDirection(dualsenseGyroDirection);
            }
            else
            {
                lastMovementInputBeforeDash = movementInput;
                lastDashDirection = dashDirection;
            }

            RumbleManager.instance.RumblePulse(playerInput.devices[0] as Gamepad, 0.5f, 0.5f, 0.3f);
            StartCoroutine(DashCooldownVisual());

            // spin the player 360 degrees according to the direction
            float startingRotationZ = transform.rotation.eulerAngles.z;

            float targetRotationZ;
            if (lastDashDirection == DashDirection.TopRight || lastDashDirection == DashDirection.Right || lastDashDirection == DashDirection.BottomRight)
            {
                targetRotationZ = startingRotationZ - 360;
            }
            else if (lastDashDirection == DashDirection.BottomLeft || lastDashDirection == DashDirection.Left || lastDashDirection == DashDirection.TopLeft)
            {
                targetRotationZ = startingRotationZ + 360;
            }
            else
            {
                targetRotationZ = startingRotationZ;
            }
            
            float rotationTimer = 0f;
            float rotationDuration = 0.5f;
            isDashing = true;

            audioManager.PlayDashSound(playerAudioSource);

            if (lastDashDirection != DashDirection.Top && lastDashDirection != DashDirection.Bottom)
            {
                while (rotationTimer < rotationDuration)
                {
                    rotationTimer += Time.deltaTime;
                    float rotationZ = Mathf.Lerp(startingRotationZ, targetRotationZ, rotationTimer / rotationDuration) % 360;
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, rotationZ);
                    yield return null;
                }
            }
            else
            {
                float dashDuration = 0.5f;
                float dashTimer = 0f;
                while (dashTimer < dashDuration)
                {
                    dashTimer += Time.deltaTime;
                    yield return null;
                }
            }
            isDashing = false;
            transform.rotation = Quaternion.identity;
            
            allowMovement = true;
            allowShooting = true;
            col.enabled = true;
            yield return new WaitForSeconds(dashCooldown);

            if (lastDashDirection == DashDirection.Right) lockLeftMovement = false;
            else if (lastDashDirection == DashDirection.Left) lockRightMovement = false;

            transform.rotation = Quaternion.identity;
            if (playerInput == null) EnableShootingForAllPlayers();
            if (crosshair != null) crosshair.SetActive(true);
            allowDashing = true;
        }

        public void DisableDuoMode()
        {
            inDuoMode = false;
            allowMovement = true;
            allowDashing = true;
            entityCanvas.SetActive(true);

            print("Duo mode disabled for player " + playerNumber);
            ShowPlayer();
            col.enabled = true;

            // Add a force to the player to separate them, player 1 to the left, player 2 to the right
            ApplySeparationForce(7000);
        }

        void DisableShootingForAllPlayers()
        {
            foreach (Player player in FindObjectsOfType<Player>())
            {
                player.allowShooting = false;
            }
        }

        void EnableShootingForAllPlayers()
        {
            foreach (Player player in FindObjectsOfType<Player>())
            {
                player.allowShooting = true;
            }
        }

        public void AssignMaterial()
        {
            GetComponent<MeshRenderer>().material = playerMaterials[playerNumber - 1];
            // apply material to all parts
            foreach (GameObject playerPart in playerParts)
            {
                if (playerPart.name.Contains("Glass")) continue;
                playerPart.GetComponent<MeshRenderer>().material = playerMaterials[playerNumber - 1];
            }
        }

        void SpawnPlayerSilhouette()
        {
            GameObject newPlayerSilhouette = Instantiate(playerSilhouette, transform.position, transform.rotation);
            if (playerNumber == 1)
            {
                newPlayerSilhouette.GetComponent<PlayerSilhouette>().silhouetteColor = playerSilhouetteColors[0];
            }
            else if (playerNumber == 2)
            {
                newPlayerSilhouette.GetComponent<PlayerSilhouette>().silhouetteColor = playerSilhouetteColors[1];
            }
            else
            {
                newPlayerSilhouette.GetComponent<PlayerSilhouette>().silhouetteColor = Color.white;
            }
        }

        public void ApplySeparationForce(float force)
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
            rb.AddForce(forceDirection * force);
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
            foreach (Card card in newPlayerCards.GetComponentsInChildren<Card>())
            {
                card.ownerPlayer = this;
                if (card.gameObject.name == "Card1")
                {
                    eventSystem.firstSelectedGameObject = card.gameObject;
                    eventSystem.SetSelectedGameObject(card.gameObject);
                    card.gameObject.GetComponent<Button>().OnSelect(null);
                    card.startHighlighted = true;
                }
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
                    maxHealth += 8;
                    health += 8;
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
                case "Regeneration":
                    hasRegen = true;
                    break;
                case "Shield":
                    hasShield = true;
                    break;
                case "XP Multiplier":
                    xpMultiplier += 0.1f;
                    break;
                default:
                    break;
            }
            if (inDuoMode && playerDuo != null) playerDuo.CombineUpgrades();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Enemy"))
            {
                print("Player collided with enemy");
                Enemy collidedEnemy = other.GetComponent<Enemy>();
                TakeDamage(other.GetComponent<Enemy>().damageOnImpact, false, collidedEnemy);
            }
        }

        public void CombineUpgrades()
        {
            if (playerInput != null) return;

            health = 0;
            maxHealth = 0;
            hasRegen = false;

            shootingManager.projectileSpeed = 0;
            shootingManager.projectileDamage = 0;
            shootingManager.projectileSize = 0;
            shootingManager.multishotCount = 0;
            shootingManager.burstFireCount = 0;

            shootingManager.shootProjectile = false;
            shootingManager.shootBackShots = false;
            shootingManager.shootLightning = false;

            foreach (PlayerInput playerInput in PlayerInput.all)
            {
                Player player = playerInput.GetComponent<Player>();
                ShootingManager playerShootingManager = player.GetComponent<ShootingManager>();

                health += player.health;
                maxHealth += player.maxHealth;
                if (player.hasRegen) hasRegen = true;

                shootingManager.projectileSpeed += playerShootingManager.projectileSpeed / 2;
                shootingManager.projectileDamage += playerShootingManager.projectileDamage / 2;
                shootingManager.projectileSize += playerShootingManager.projectileSize / 2;
                shootingManager.multishotCount += playerShootingManager.multishotCount;
                shootingManager.burstFireCount += playerShootingManager.burstFireCount;

                if (shootingManager.shootProjectile == false) shootingManager.shootProjectile = playerShootingManager.shootProjectile;
                if (shootingManager.shootBackShots == false) shootingManager.shootBackShots = playerShootingManager.shootBackShots;
                if (shootingManager.shootLightning == false) shootingManager.shootLightning = playerShootingManager.shootLightning;
            }

            healthBar.maxValue = maxHealth;
            healthBar.value = health;
            print("Combined upgrades");
        }

        void GainXP(float xp)
        {
            
        }

        IEnumerator DashCooldownVisual()
        {
            foreach (GameObject playerGlassPart in playerGlassParts)
            {
                Color initialColor = playerGlassPart.GetComponent<MeshRenderer>().material.color;
                Color targetColor = Color.white;
                float pulseTimer = 0f;
                float pulseDuration = 0.2f;
                while (!allowDashing)
                {
                    pulseTimer += Time.deltaTime;
                    playerGlassPart.GetComponent<MeshRenderer>().material.color = Color.Lerp(initialColor, targetColor, pulseTimer / pulseDuration);
                    yield return null;
                }
                playerGlassPart.GetComponent<MeshRenderer>().material.color = initialColor;
            }
        }

        void ShowPlayer()
        {
            meshRenderer.enabled = true;
            foreach (GameObject playerPart in playerParts)
            {
                playerPart.SetActive(true);
            }
        }

        void HidePlayer()
        {
            meshRenderer.enabled = false;
            foreach (GameObject playerPart in playerParts)
            {
                playerPart.SetActive(false);
            }
        }
    }
}
