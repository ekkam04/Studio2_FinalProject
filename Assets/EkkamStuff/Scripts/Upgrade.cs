using System;
using UnityEngine;

namespace Ekkam {
    [Serializable]
    public struct Upgrade
    {
        public string upgradeName;
        public string upgradeDescription;
        public int timesUpgraded;
        public Color upgradeBorderColor;
        public Color upgradeBGColor;
    }
}
