using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class PlayerSilhouette : MonoBehaviour
    {

        public Color silhouetteColor;
        float disappearTimer;
        public List<Material> materials = new List<Material>();

        void Awake()
        {
            materials.Add(GetComponent<MeshRenderer>().material);
            foreach (Transform child in transform)
            {
                foreach (Transform grandchild in child)
                {
                    materials.Add(grandchild.GetComponent<MeshRenderer>().material);
                    foreach (Transform greatGrandchild in grandchild)
                    {
                        materials.Add(greatGrandchild.GetComponent<MeshRenderer>().material);
                    }
                }
            }
        }

        void Update()
        {
            disappearTimer += Time.deltaTime;
            foreach (Material material in materials)
            {
                // change albedo color alpha
                Color color = silhouetteColor;
                color.a = Mathf.Lerp(0.1f, 0f, disappearTimer * 2.5f);
                material.color = color;
            }
            if (disappearTimer >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
