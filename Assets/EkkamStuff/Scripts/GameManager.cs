using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ekkam {
    public class GameManager : MonoBehaviour
    {
        public delegate void PlayerEvent();
        public static event PlayerEvent OnCombinePlayers;
        public static event PlayerEvent OnSeperatePlayers;

        [SerializeField] GameObject playerDuoPrefab;

        [SerializeField] GameObject gameVCam;
        [SerializeField] GameObject combinedVCam;

        void Start()
        {
            gameVCam.SetActive(true);
            combinedVCam.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (OnCombinePlayers != null) StartCoroutine(CombinePlayers());
            }   
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (OnSeperatePlayers != null) StartCoroutine(SeperatePlayers());
            }
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

        IEnumerator CombinePlayers()
        {
            if (AllPlayersInDuoMode()) yield break;
            if (OnCombinePlayers != null)
            {
                OnCombinePlayers();
                combinedVCam.SetActive(true);
                gameVCam.SetActive(false);
                yield return new WaitUntil(() => AllPlayersInDuoMode());
                GameObject playerDuo = Instantiate(playerDuoPrefab, PlayerInput.all[0].transform.position, Quaternion.identity);
                foreach (PlayerInput playerInput in PlayerInput.all)
                {
                    playerInput.GetComponent<Player>().playerDuo = playerDuo.GetComponent<Player>();
                }
            }
        }

        IEnumerator SeperatePlayers()
        {
            if (OnSeperatePlayers != null)
            {
                OnSeperatePlayers();
                gameVCam.SetActive(true);
                combinedVCam.SetActive(false);
                yield return new WaitUntil(() => !AllPlayersInDuoMode());
                foreach (PlayerInput playerInput in PlayerInput.all)
                {
                    playerInput.GetComponent<Player>().playerDuo = null;
                }
                Destroy(GameObject.FindGameObjectWithTag("PlayerDuo"));
            }
        }

        bool AllPlayersInDuoMode()
        {
            foreach (PlayerInput playerInput in PlayerInput.all)
            {
                if (playerInput.GetComponent<Player>().inDuoMode == false) return false;
            }
            return true;
        }
    }
}
