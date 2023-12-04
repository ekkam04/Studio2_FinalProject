using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class BackgroundMover : MonoBehaviour
    {
        public float moveSpeed = 0f;

        void Update()
        {
            transform.Translate(Vector3.back * moveSpeed * Time.deltaTime); 

            if (transform.position.z < -200f) {
                Destroy(gameObject);
            }
        }
    }
}
