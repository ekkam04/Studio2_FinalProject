using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class ProjectilePool : MonoBehaviour
    {
        public bool destroyWhenAllInactive = false;

        void Update()
        {
            if (destroyWhenAllInactive && AllInactive())
            {
                print("Destroying " + gameObject.name + " because all projectiles are inactive and the pool creator is destroyed.");
                Destroy(gameObject);
            }
        }

        bool AllInactive()
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeSelf) return false;
            }
            return true;
        }
    }
}
