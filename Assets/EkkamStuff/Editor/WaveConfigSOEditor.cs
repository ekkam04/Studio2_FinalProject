using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Ekkam
{
    [CustomEditor(typeof(WaveConfigSO))]
    public class WaveConfigSOEditor : Editor
    {
        private ReorderableList waveEnemiesList;

        private readonly int[] propertyWidths = new int[] { 120, 120, 80, 80, 80, 100 };

        private void OnEnable()
        {
            waveEnemiesList = new ReorderableList(serializedObject, serializedObject.FindProperty("waveEnemies"), true, true, true, true);

            waveEnemiesList.drawHeaderCallback = (Rect rect) =>
            {
                int draggableIconOffset = 15;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, propertyWidths[0], EditorGUIUtility.singleLineHeight), "Enemy");
                EditorGUI.LabelField(new Rect(rect.x + propertyWidths[0] + draggableIconOffset + 15, rect.y, propertyWidths[1], EditorGUIUtility.singleLineHeight), "Enemy Path");
                EditorGUI.LabelField(new Rect(rect.x + propertyWidths[0] + propertyWidths[1] + draggableIconOffset + 30, rect.y, propertyWidths[2], EditorGUIUtility.singleLineHeight), "Speed Curve");
                EditorGUI.LabelField(new Rect(rect.x + propertyWidths[0] + propertyWidths[1] + propertyWidths[2] + draggableIconOffset + 45, rect.y, propertyWidths[3], EditorGUIUtility.singleLineHeight), "Move Speed");
                EditorGUI.LabelField(new Rect(rect.x + propertyWidths[0] + propertyWidths[1] + propertyWidths[2] + propertyWidths[3] + draggableIconOffset + 60, rect.y, propertyWidths[4], EditorGUIUtility.singleLineHeight), "Invert Path X");
                EditorGUI.LabelField(new Rect(rect.x + propertyWidths[0] + propertyWidths[1] + propertyWidths[2] + propertyWidths[3] + propertyWidths[4] + draggableIconOffset + 75, rect.y, propertyWidths[5], EditorGUIUtility.singleLineHeight), "Spawn Together");
            };  

            waveEnemiesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = waveEnemiesList.serializedProperty.GetArrayElementAtIndex(index);
                waveEnemiesList.elementHeight = EditorGUIUtility.singleLineHeight + 5;
                rect.y += 3;

                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, propertyWidths[0], EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("enemy"),
                    GUIContent.none
                );

                EditorGUI.PropertyField(
                    new Rect(rect.x + propertyWidths[0] + 15, rect.y, propertyWidths[1], EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("enemyPath"),
                    GUIContent.none
                );

                EditorGUI.PropertyField(
                    new Rect(rect.x + propertyWidths[0] + propertyWidths[1] + 30, rect.y, propertyWidths[2], EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("speedCurve"),
                    GUIContent.none
                );

                EditorGUI.PropertyField(
                    new Rect(rect.x + propertyWidths[0] + propertyWidths[1] + propertyWidths[2] + 45, rect.y, propertyWidths[3], EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("moveSpeed"),
                    GUIContent.none
                );

                EditorGUI.PropertyField(
                    new Rect(rect.x + propertyWidths[0] + propertyWidths[1] + propertyWidths[2] + propertyWidths[3] + 60, rect.y, propertyWidths[4], EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("invertPathX"),
                    GUIContent.none
                );

                EditorGUI.PropertyField(
                    new Rect(rect.x + propertyWidths[0] + propertyWidths[1] + propertyWidths[2] + propertyWidths[3] + propertyWidths[4] + 75, rect.y, propertyWidths[5], EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("spawnNextEnemyTogether"),
                    GUIContent.none
                );
            };
        }

        public override void OnInspectorGUI()
        {
            WaveConfigSO waveConfig = (WaveConfigSO)target;

            // base.OnInspectorGUI();
            // EditorGUILayout.Space();

            EditorGUILayout.LabelField("Enemy Settings", EditorStyles.boldLabel);

            serializedObject.Update();
            waveEnemiesList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.LabelField("Spawn Settings", EditorStyles.boldLabel);
            waveConfig.randomizeSpawnTime = EditorGUILayout.Toggle("Randomize Spawn Time", waveConfig.randomizeSpawnTime);
            waveConfig.timeBetweenEnemySpawns = EditorGUILayout.FloatField("Time Between Enemy Spawns", waveConfig.timeBetweenEnemySpawns);

            if (waveConfig.randomizeSpawnTime)
            {
                waveConfig.spawnTimeVariance = EditorGUILayout.FloatField("Spawn Time Variance", waveConfig.spawnTimeVariance);
                waveConfig.minSpawnTime = EditorGUILayout.FloatField("Min Spawn Time", waveConfig.minSpawnTime);
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Wave Settings", EditorStyles.boldLabel);
            waveConfig.waitTimeBeforeNextWave = EditorGUILayout.FloatField("Wait Time Before Next Wave", waveConfig.waitTimeBeforeNextWave);
        }

    }
}
