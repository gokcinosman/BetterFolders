using UnityEditor;
using UnityEngine;
using FolderColorNamespace;
[CustomEditor(typeof(FolderColorSettings))]
public class FolderColorSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var settings = target as FolderColorSettings;
        EditorGUILayout.LabelField("Kontrol Ayarları", EditorStyles.boldLabel);
        settings.modifierKey = (FolderColorSettings.ModifierKeyType)EditorGUILayout.EnumPopup(
            "Modifier Tuşu",
            settings.modifierKey
        );
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Klasör Kuralları", EditorStyles.boldLabel);
        base.OnInspectorGUI();
    }
}