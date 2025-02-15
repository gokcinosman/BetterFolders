using UnityEditor;
using UnityEngine;
using FolderColorNamespace;
[CustomEditor(typeof(FolderColorSettings))]
public class FolderColorSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var settings = target as FolderColorSettings;
        EditorGUILayout.LabelField("Control Settings", EditorStyles.boldLabel);
        settings.modifierKey = (FolderColorSettings.ModifierKeyType)EditorGUILayout.EnumPopup(
            "Modifier Key",
            settings.modifierKey
        );
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Folder Rules", EditorStyles.boldLabel);
        base.OnInspectorGUI();
    }
}