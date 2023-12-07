using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam {
    public class TutorialAssistant : MonoBehaviour
    {
        [SerializeField] GameObject tutorialCanvas;
        [SerializeField] GameObject[] tutorials;
        [SerializeField] TMP_Text dialogText;

        void Start()
        {
            HideTutorial(); 
        }

        void Update()
        {
            
        }

        public void ShowTutorial(int tutorialIndex)
        {
            foreach (GameObject tutorial in tutorials)
            {
                tutorial.SetActive(false);
            }
            tutorials[tutorialIndex].SetActive(true);
            tutorialCanvas.SetActive(true);
        }

        void HideTutorial()
        {
            foreach (GameObject tutorial in tutorials)
            {
                tutorial.SetActive(false);
            }
            tutorialCanvas.SetActive(false);
        }
    }
}
