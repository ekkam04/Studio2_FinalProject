using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ekkam {
    public class WavePreviewWindow : EditorWindow
    {
        [MenuItem("Window/Wave Preview")]
        public static void OpenWindow()
        {
            GetWindow<WavePreviewWindow>("Wave Preview Playback");
        }

        float playbackModifier;
        float lastTime;
        GameObject currentOpenedPrefab;

        Enemy enemyPrefab;
        Enemy previewEnemy;
        AnimationCurve speedCurve;
        float moveSpeed;

        void OnEnable()
        {
            EditorApplication.update += OnUpdate;
        }

        void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
        }

        void OnUpdate()
        {
            if (playbackModifier != 0f)
            {
                PreviewTime.time += (Time.realtimeSinceStartup - lastTime) * playbackModifier;
                Repaint();
                SceneView.RepaintAll();
                if (enemyPrefab != null)
                {
                    if (previewEnemy == null)
                    {
                        if (currentOpenedPrefab != null && currentOpenedPrefab.GetComponent<Path>() != null)
                        {
                            Path path = currentOpenedPrefab.GetComponent<Path>();
                            previewEnemy = path.SpawnPreviewEnemy(enemyPrefab);
                        }
                    }
                    previewEnemy.speedCurve = speedCurve;
                    previewEnemy.moveSpeed = moveSpeed;
                    previewEnemy.FollowEditorPath((Time.realtimeSinceStartup - lastTime) * playbackModifier);
                }
            }

            lastTime = Time.realtimeSinceStartup;
        }

        // show playback controls
        void OnGUI()
        {
            currentOpenedPrefab = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()?.prefabContentsRoot;
            float seconds = Mathf.Floor(PreviewTime.time % 60f);
            float minutes = Mathf.Floor(PreviewTime.time / 60f);

            GUILayout.Label(string.Format("{0:00}:{1:00}", minutes, seconds));
            GUILayout.Label("Playback Speed: " + playbackModifier.ToString("0.00") + "x");

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Pause"))
                {
                    playbackModifier = 0f;
                }

                if (GUILayout.Button("Play 1x Speed"))
                {
                    playbackModifier = 1f;
                }
                if (GUILayout.Button("Play 5x Speed"))
                {
                    playbackModifier = 5f;
                }
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Reset"))
            {
                ResetPreview();  
            }

            GUILayout.Space(10);

            if (currentOpenedPrefab != null && currentOpenedPrefab.GetComponent<Path>() != null)
            {
                Path path = currentOpenedPrefab.GetComponent<Path>();
                ShowPreviewEnemySettings();
            }
            else if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Path>() != null)
            {
                currentOpenedPrefab = Selection.activeGameObject;
                Path path = Selection.activeGameObject.GetComponent<Path>();
                ShowPreviewEnemySettings();
            }
            else
            {
                ResetPreview();
                GUILayout.Label("No path selected");
                GUILayout.Label("Select a path prefab in the scene");
            }   
        }

        void ShowPreviewEnemySettings()
        {
            GUILayout.Label("Selected Path: " + currentOpenedPrefab.name);
            GUILayout.Space(10);
            GUILayout.Label("Preview Enemy Settings", EditorStyles.boldLabel);
            enemyPrefab = (Enemy)EditorGUILayout.ObjectField("Enemy Prefab", enemyPrefab, typeof(Enemy), false);
            speedCurve = EditorGUILayout.CurveField("Speed Curve", speedCurve);
            moveSpeed = EditorGUILayout.FloatField("Move Speed", moveSpeed);
        }

        void ResetPreview()
        {
            playbackModifier = 0f;
            PreviewTime.time = 0f;
            if (previewEnemy != null)
            {
                DestroyImmediate(previewEnemy.gameObject);
                previewEnemy = null;
            }
        }
    }
}
