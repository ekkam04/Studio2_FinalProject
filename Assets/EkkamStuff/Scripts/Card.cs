using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace Ekkam {
    public class Card : MonoBehaviour
    {
        public TMP_Text upgradeName;
        public TMP_Text upgradeDescription;
        public Button upgradeButton;

        void Awake()
        {
            upgradeButton = GetComponent<Button>();
        }

        void OnCardSelected(Player player)
        {
            ShootingManager shootingManager = player.GetComponent<ShootingManager>();
            switch (upgradeName.text.ToLower())
            {
                case "damage":
                    // upgrade
                    break;
                case "projectile size":
                    // upgrade
                    break;
                case "projectile speed":
                    shootingManager.projectileSpeed += 5f;
                    break;
            }
        }
    }
}
