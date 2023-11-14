using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Ekkam {
    [CustomEditor(typeof(UpgradeManager))]
    public class UpgradeManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            UpgradeManager upgradeManager = (UpgradeManager)target;

            EditorGUILayout.LabelField("Upgrades", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            for (int i = 0; i < upgradeManager.upgradeNames.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Upgrade " + (i + 1).ToString(), GUILayout.Width(80));
                upgradeManager.upgradeNames[i] = EditorGUILayout.TextField(upgradeManager.upgradeNames[i], GUILayout.Width(125));
                upgradeManager.upgradeDescriptions[i] = EditorGUILayout.TextField(upgradeManager.upgradeDescriptions[i]);
                if (GUILayout.Button("-"))
                {
                    upgradeManager.upgradeNames.RemoveAt(i);
                    upgradeManager.upgradeDescriptions.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Upgrade"))
            {
                upgradeManager.upgradeNames.Add("");
                upgradeManager.upgradeDescriptions.Add("");
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Show Upgrades"))
            {
                upgradeManager.ShowUpgrades();
            }
            if (GUILayout.Button("Hide Upgrades"))
            {
                upgradeManager.HideUpgrades();
            }
            EditorGUILayout.EndHorizontal();

        }
    }
}
