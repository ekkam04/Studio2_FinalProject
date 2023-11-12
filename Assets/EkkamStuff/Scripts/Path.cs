using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class Path : MonoBehaviour
    {
        public Transform[] waypoints;

        private void OnDrawGizmos()
        {
            transform.position = Vector3.zero;
            waypoints = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                waypoints[i] = transform.GetChild(i);
            }

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (i == 0) Gizmos.color = Color.green;
                else if (i == waypoints.Length - 1) Gizmos.color = Color.red;
                else Gizmos.color = Color.yellow;

                Gizmos.DrawSphere(waypoints[i].position, 2f);
                if (i < waypoints.Length - 1)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
    }
}
