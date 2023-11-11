using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ekkam {
    public class Player : MonoBehaviour
    {
        public int playerNumber;

        [SerializeField] Mesh[] playerMeshes; // 0 = player 1, 1 = player 2

        public Player playerDuo;

        Rigidbody rb;
        ShootingManager shootingManager;
        Vector2 input;

        public bool allowMovement = true;
        public bool inDuoMode = false;

        float shootTimer = 0f;
        float dodgeTimer = 0f;

        [Header("----- Player Stats -----")]
        [Tooltip("The total health")] [SerializeField] float health = 100f; // Still need to implement
        [SerializeField] float moveSpeed = 20f;
        [SerializeField] float maxSpeed = 25f;
        [SerializeField] float dodgeSpeed = 1f;
        [Tooltip("The cooldown time before the player can shoot again")] [SerializeField] float attackSpeed = 0.05f;
        [Tooltip("The cooldown time before the player can dodge again")] [SerializeField] float dodgeCooldown = 1f;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            shootingManager = GetComponent<ShootingManager>();
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
                        playerDuo.shootingManager.Shoot();
                    }
                }
                else
                {
                    shootingManager.Shoot();
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
            rb.velocity = Vector3.zero;
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
            while (Vector3.Distance(transform.position, targetPlayer.transform.position) > 0.11f)
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
            GetComponent<Collider>().enabled = false;
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

            print("Duo mode disabled for player " + playerNumber);
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<Collider>().enabled = true;

            // Add a force to the player to separate them, player 1 to the left, player 2 to the right
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

        public void AssignMesh()
        {
            GetComponent<MeshFilter>().mesh = playerMeshes[playerNumber - 1];
        }
    }
}
