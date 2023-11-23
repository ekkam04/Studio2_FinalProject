using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class Path : MonoBehaviour
    {
        [HideInInspector]
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
                else if (waypoints[i].gameObject.tag == "WaitWaypoint") Gizmos.color = Color.cyan;
                else Gizmos.color = Color.yellow;

                Gizmos.DrawSphere(waypoints[i].position, 2f);
                if (i < waypoints.Length - 1)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }

        public Enemy SpawnPreviewEnemy(Enemy enemyPrefab)
        {
            Enemy previewEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.Euler(0f, 180f, 0f)).GetComponent<Enemy>();
            previewEnemy.gameObject.name = "PreviewEnemy";
            previewEnemy.pathPrefab = transform;
            previewEnemy.waypoints = previewEnemy.GetWaypoints();
            previewEnemy.transform.position = previewEnemy.waypoints[0].position;
            return previewEnemy;
        }
    }
}
