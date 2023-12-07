using System;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam {
    [Serializable]
    public struct Upgrade
    {
        public string upgradeName;
        public string upgradeDescription;
        public int timesUpgraded;
        public Texture2D upgradeIcon;
        public Color upgradeBorderColor;
        public Color upgradeBGColor;
    }
}
