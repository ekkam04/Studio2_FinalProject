using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ekkam {
    public class GameManager : MonoBehaviour
    {
        void Start()
        {
            
        }

        void Update()
        {
            
        }

        public void AddPlayer(PlayerInput playerInput)
        {
            Player player = playerInput.GetComponent<Player>();
            
            // Assign player number
            player.playerNumber = PlayerInput.all.Count;
            playerInput.gameObject.name = "Player_" + player.playerNumber;
            print("Player " + player.playerNumber + " joined");

            // Assign player mesh
            player.AssignMesh();
        }
    }
}
