using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ekkam {
    public class Player : MonoBehaviour
    {
        [SerializeField] Mesh[] playerMeshes; // 0 = player 1, 1 = player 2
        Rigidbody rb;
        Vector2 input;

        public int playerNumber;

        public float speed = 10;
        public float maxSpeed = 10;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        void Start()
        {
            
        }

        void Update()
        {
            ControlSpeed();
        }

        private void FixedUpdate()
        {
            Move();
        }

        void Move()
        {
            // Move player
            Vector3 movement = new Vector3(input.x, 0, input.y);
            rb.AddForce(movement * speed * 100);
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

        public void AssignMesh()
        {
            GetComponent<MeshFilter>().mesh = playerMeshes[playerNumber - 1];
        }
    }
}
