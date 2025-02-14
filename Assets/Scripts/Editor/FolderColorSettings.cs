using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace FolderColorNamespace
{
    [CustomEditor(typeof(FolderColorSettings))]
    public class FolderColorSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            FolderColorSettings settings = (FolderColorSettings)target;
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Klasör Kuralları", EditorStyles.boldLabel);
            for (int i = 0; i < settings.folderRules.Count; i++)
            {
                var rule = settings.folderRules[i];
                if (rule.icon == null)
                {
                    EditorGUILayout.HelpBox($"{rule.folderName} kuralında ikon eksik!", MessageType.Warning);
                }
                // ... diğer property çizimleri ...
            }
        }
    }
    public class FolderColorSettings : ScriptableObject
    {
        public KeyCode modifierKey = KeyCode.Mouse2;
        public List<FolderRule> folderRules = new List<FolderRule>();
    }
}
