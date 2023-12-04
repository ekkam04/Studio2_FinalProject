using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class BackgroundLooper : MonoBehaviour
    {
        [SerializeField] GameObject[] backgroundPrefabs;

        [SerializeField] float bgSpawnDelay = 2f;
        [SerializeField] float moveSpeed = 10f;
        float timer = 0f;

        void Start()
        {
            SpawnBackground();

            BackgroundMover[] backgroundMovers = FindObjectsOfType<BackgroundMover>();
            foreach (BackgroundMover bg in backgroundMovers) {
                bg.moveSpeed = moveSpeed;
            }
        }

        void Update()
        {
            timer += Time.deltaTime;    
            if (timer >= bgSpawnDelay) {
                timer = 0f;
                SpawnBackground();
            }
        }

        void SpawnBackground() {
            int randomIndex = Random.Range(0, backgroundPrefabs.Length);
            GameObject bg = Instantiate(backgroundPrefabs[randomIndex], transform.position, Quaternion.identity);
            bg.transform.parent = transform;
            bg.GetComponent<BackgroundMover>().moveSpeed = moveSpeed;
        }
    }
}