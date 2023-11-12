using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class Path : MonoBehaviour
    {
        public Transform[] waypoints;

        void Awake()
        {
            waypoints = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                waypoints[i] = transform.GetChild(i);
            }
        }

        private void OnDrawGizmos()
        {
            waypoints = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                waypoints[i] = transform.GetChild(i);
            }
            
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.DrawSphere(waypoints[i].position, 0.5f);
                if (i < waypoints.Length - 1)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
    }
}
