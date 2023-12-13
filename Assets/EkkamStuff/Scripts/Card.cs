using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Ekkam {
    public class Card : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public Player ownerPlayer;

        public TMP_Text upgradeName;
        public TMP_Text upgradeDescription;
        public RawImage upgradeIcon;
        public Button upgradeButton;
        public Animator cardAnimator;

        public Image[] CardBGs;
        public Image CardBorder;

        [HideInInspector]
        public bool startHighlighted = false;

        Color bgColor;
        Color borderColor;
        Color dullingColor;

        UpgradeManager upgradeManager;

        void Awake()
        {
            upgradeManager = FindObjectOfType<UpgradeManager>();
            dullingColor = upgradeManager.dullingColor;
            cardAnimator.SetBool("Highlighted", startHighlighted);
        }

        public void OnCardSelected(TMP_Text waitingText)
        {
            print("Player " + ownerPlayer.playerNumber + " selected " + upgradeName.text);
            ownerPlayer.cardPicked = true;
            
            if (ownerPlayer.playerNumber == 1)
            {
                waitingText.text = "Waiting for Player 2 to select an upgrade";
            }
            else
            {
                waitingText.text = "Waiting for Player 1 to select an upgrade";
            }
            waitingText.gameObject.SetActive(true);
            
            Color iconColor = upgradeIcon.color;
            iconColor.a = 0.75f;
            upgradeManager.UpgradePlayer(ownerPlayer, this, upgradeIcon.texture as Texture2D, iconColor);
        }

        public void SetCardColors(Color bgColor, Color borderColor)
        {
            this.bgColor = bgColor;
            this.borderColor = borderColor;
            foreach (Image bg in CardBGs)
            {
                bg.color = bgColor;
            }
            CardBorder.color = borderColor;
            ColorBlock cb = upgradeButton.colors;
            cb.normalColor = borderColor;
            upgradeButton.colors = cb;

            upgradeName.color = Color.white;
            upgradeDescription.color = Color.white;

            upgradeIcon.color = borderColor;

            if (startHighlighted == false) DullCardColors();
        }

        public void SetUpgradeIcon(Texture2D icon)
        {
            upgradeIcon.texture = icon;
        }

        void DullCardColors()
        {
            foreach (Image bg in CardBGs)
            {
                bg.color = GetMiddleColor(bgColor, dullingColor);
            }
            CardBorder.color = GetMiddleColor(borderColor, dullingColor);
            ColorBlock cb = upgradeButton.colors;
            cb.normalColor = GetMiddleColor(borderColor, dullingColor);
            upgradeButton.colors = cb;

            upgradeName.color = GetMiddleColor(upgradeName.color, dullingColor);
            upgradeDescription.color = GetMiddleColor(upgradeDescription.color, dullingColor);

            upgradeIcon.color = GetMiddleColor(borderColor, dullingColor);
        }

        void BrightenCardColors()
        {
            foreach (Image bg in CardBGs)
            {
                bg.color = bgColor;
            }
            CardBorder.color = borderColor;
            ColorBlock cb = upgradeButton.colors;
            cb.normalColor = borderColor;
            upgradeButton.colors = cb;

            upgradeName.color = Color.white;
            upgradeDescription.color = Color.white;

            upgradeIcon.color = borderColor;
        }

        Color GetMiddleColor(Color color1, Color color2)
        {
            Color middleColor = new Color();
            middleColor.r = (color1.r + color2.r) / 2;
            middleColor.g = (color1.g + color2.g) / 2;
            middleColor.b = (color1.b + color2.b) / 2;
            middleColor.a = 1f;
            return middleColor;
        }

        public void OnSelect(BaseEventData eventData)
        {
            print("Selected " + upgradeName.text);
            BrightenCardColors();
            cardAnimator.SetBool("Highlighted", true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            print("Deselected " + upgradeName.text);
            DullCardColors();
            cardAnimator.SetBool("Highlighted", false);
        }
    }
}
