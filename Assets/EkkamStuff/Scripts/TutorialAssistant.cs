using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam {
    public class TutorialAssistant : DamagableEntity
    {
        GameManager gameManager;
        WaveSpawner waveSpawner;
        public bool playTutorialAtStart = false;
        bool skipTutorial = false;
        [SerializeField] Transform path;
        int waypointIndex = 0;

        [SerializeField] GameObject tutorialCanvas;
        [SerializeField] GameObject[] tutorials;
        [SerializeField] TMP_Text dialogText;
        [SerializeField] string[] dialogues;

        void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            waveSpawner = FindObjectOfType<WaveSpawner>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        void Start()
        {
            HideTutorial(); 
            transform.position = path.GetChild(waypointIndex).position;
            InitializeDamagableEntity();

            if (playTutorialAtStart == true)
            {
                StartCoroutine(StartTutorial());
            }
        }

        void Update()
        {
            
        }

        IEnumerator FlyToNextWaypoint()
        {
            while (waypointIndex < path.childCount)
            {
                transform.position = Vector3.MoveTowards(transform.position, path.GetChild(waypointIndex).position, 100f * Time.deltaTime);
                if (transform.position == path.GetChild(waypointIndex).position)
                {
                    waypointIndex++;
                    yield break;
                }
                yield return null;
            }
        }

        IEnumerator FlyToLastWaypoint()
        {
            yield return new WaitForSeconds(3);
            int lastWaypointIndex = path.childCount - 1;
            while (transform.position != path.GetChild(lastWaypointIndex).position)
            {
                transform.position = Vector3.MoveTowards(transform.position, path.GetChild(lastWaypointIndex).position, 100f * Time.deltaTime);
                yield return null;
            }
            tutorialCanvas.SetActive(false);
            waveSpawner.StartSpawningWaves();
            Destroy(gameObject);
        }

        public void StartTutorialSequence()
        {
            StartCoroutine(StartTutorial());
        }

        IEnumerator StartTutorial()
        {
            StartCoroutine(FlyToNextWaypoint());
            ShowDialog(0);
            yield return new WaitForSeconds(1);
            StartCoroutine(FlyToNextWaypoint());
            UnlockAbilitiesForAllPlayers(true, false, true);
            yield return new WaitForSeconds(5);
            ShowDialog(1);
            StartCoroutine(FlyToNextWaypoint());
            yield return new WaitForSeconds(4);

            ShowTutorial(0);
            // UnlockAbilitiesForAllPlayers(true, false, true);
            yield return new WaitUntil(() => AllPlayersMoved());
            yield return new WaitForSeconds(3);

            ShowDialog(2);
            StartCoroutine(FlyToNextWaypoint());
            yield return new WaitForSeconds(5);

            ShowTutorial(1);
            // UnlockAbilitiesForAllPlayers(true, false, true);
            yield return new WaitUntil(() => AllPlayersShot());
            yield return new WaitForSeconds(3);

            ShowDialog(3);
            StartCoroutine(FlyToNextWaypoint());
            yield return new WaitForSeconds(5);

            ShowTutorial(2);
            UnlockAbilitiesForAllPlayers(true, true, true);
            yield return new WaitUntil(() => AllPlayersDashed());
            yield return new WaitForSeconds(3);

            ShowDialog(4);
            StartCoroutine(FlyToNextWaypoint());
            yield return new WaitForSeconds(5);

            ShowDialog(5);
            StartCoroutine(FlyToNextWaypoint());
            yield return new WaitForSeconds(5);

            ShowDialog(6);
            StartCoroutine(FlyToNextWaypoint());
            yield return new WaitForSeconds(5);

            HideTutorial();
            StartCoroutine(FlyToNextWaypoint());
            yield return new WaitForSeconds(5);

            waveSpawner.StartSpawningWaves();
            Destroy(gameObject);
        }

        public void ShowDialog(int dialogueIndex)
        {
            foreach (GameObject tutorial in tutorials)
            {
                tutorial.SetActive(false);
            }
            dialogText.text = dialogues[dialogueIndex];
            tutorialCanvas.SetActive(true);
        }

        public void ShowTutorial(int tutorialIndex)
        {
            dialogText.text = "";
            foreach (GameObject tutorial in tutorials)
            {
                tutorial.SetActive(false);
            }
            tutorials[tutorialIndex].SetActive(true);
            tutorialCanvas.SetActive(true);
        }

        public void HideTutorial()
        {
            foreach (GameObject tutorial in tutorials)
            {
                tutorial.SetActive(false);
            }
            tutorialCanvas.SetActive(false);
        }

        private void UnlockAbilitiesForAllPlayers(bool allowMovement, bool allowDashing, bool allowShooting)
        {
            foreach (Player p in FindObjectsOfType<Player>())
            {
                p.allowMovement = allowMovement;
                p.allowDashing = allowDashing;
                p.allowShooting = allowShooting;
            }
        }

        private bool AllPlayersMoved()
        {
            foreach (Player player in gameManager.GetPlayers())
            {
                if (!player.hasMoved) return false;
            }
            return true;
        }

        private bool AllPlayersShot()
        {
            foreach (Player player in gameManager.GetPlayers())
            {
                if (!player.hasShot) return false;
            }
            return true;
        }

        private bool AllPlayersDashed()
        {
            foreach (Player player in gameManager.GetPlayers())
            {
                if (!player.hasDashed) return false;
            }
            return true;
        }

        public override void OnDamageTaken()
        {
            if (skipTutorial == true) return;
            skipTutorial = true;
            StopAllCoroutines();
            UnlockAbilitiesForAllPlayers(true, true, true);

            foreach (GameObject tutorial in tutorials)
            {
                tutorial.SetActive(false);
            }
            dialogText.text = "Owww! ok ok, I'll stop! Just don't hurt me anymore!";
            tutorialCanvas.SetActive(true);
            StartCoroutine(FlyToLastWaypoint());
        }
    }
}
