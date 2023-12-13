using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam {
    [System.Serializable]
    public class UpgradeData
    {
        [SerializeField] public string upgradeName;
        [SerializeField] public int upgradeLevel;
    }
}
