using UnityEditor;
using UnityEngine;
using FolderColorNamespace;
using System.Collections.Generic;
using System.Linq;
using System;
[CustomEditor(typeof(FolderColorSettings))]
public class FolderColorSettingsEditor : Editor
{
    // Preset veri yapısını taşıma
    private sealed class PresetData
    {
        public string Name { get; }
        public string Guid { get; }
        public PresetData(string name, string guid)
        {
            Name = name;
            Guid = guid;
        }
    }
    // Preset listesi
    private static readonly PresetData[] m_presetArray =
    {
        new("Tailwind 100", "db57b3d810ea5d749b3e13f89a5cbefe"),
        new("Tailwind 200", "e366dd80182b9974c96f178781064042"),
        new("Tailwind 300", "b28f9ff9a2b7b05479de0e4983179598"),
        new("Tailwind 400", "a9fe21d7661bc4b4aa21c20b7a9bb0ed"),
        new("Tailwind 500", "da1dc16b216ed6649ab4989701eb78c6"),
        new("Tailwind 600", "94e65ae3c1f253e40b7e9e8ef2dd7dd7"),
        new("Tailwind 700", "b8f3594ed26e56142a97ab371cd4ed0d"),
        new("Tailwind 800", "e6049f55824952b42b81c376d6b98dd1"),
        new("Tailwind 900", "160a9e8504d038641929418e9e0f2a72"),
    };
    private string searchText = string.Empty;
    public override void OnInspectorGUI()
    {
        var settings = target as FolderColorSettings;
        EditorGUILayout.LabelField("Control Settings", EditorStyles.boldLabel);
        settings.modifierKey = (FolderColorSettings.ModifierKeyType)EditorGUILayout.EnumPopup(
            "Modifier Key",
            settings.modifierKey
        );
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Load Preset"))
        {
            var menu = new GenericMenu();
            foreach (var preset in m_presetArray)
            {
                menu.AddItem(
                    new GUIContent(preset.Name),
                    false,
                    () => LoadPreset(settings, preset)
                );
            }
            menu.ShowAsContext();
        }
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Folder Rules", EditorStyles.boldLabel);
        searchText = EditorGUILayout.TextField("Search", searchText);
        if (!string.IsNullOrEmpty(searchText))
        {
            var filteredRules = settings.folderRules.Where(r =>
                r.folderName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            DrawFilteredRules(filteredRules);
        }
        else
        {
            base.OnInspectorGUI();
        }
    }
    private void DrawFilteredRules(List<FolderRule> rules)
    {
        var settings = target as FolderColorSettings;
        var so = new SerializedObject(target);
        var prop = so.FindProperty("folderRules");
        EditorGUI.BeginChangeCheck();
        foreach (var rule in rules)
        {
            // Orijinal listedeki indeksi bul
            int originalIndex = settings.folderRules.IndexOf(rule);
            if (originalIndex < 0) continue;
            var ruleProp = prop.GetArrayElementAtIndex(originalIndex);
            EditorGUILayout.PropertyField(ruleProp, new GUIContent(rule.folderName), true);
        }
        if (EditorGUI.EndChangeCheck())
        {
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
    private void LoadPreset(FolderColorSettings settings, PresetData preset)
    {
        var path = AssetDatabase.GUIDToAssetPath(preset.Guid);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError($"Preset not found! GUID: {preset.Guid}");
            return;
        }
        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Invalid preset file: {path}");
            return;
        }
        try
        {
            var presetRules = JsonUtility.FromJson<PresetWrapper>(textAsset.text).folderRules;
            Undo.RecordObject(settings, "Apply Preset Colors");
            foreach (var existingRule in settings.folderRules)
            {
                var matchedPresetRule = presetRules.FirstOrDefault(p =>
                    p.folderName.Equals(existingRule.folderName, StringComparison.OrdinalIgnoreCase));
                if (matchedPresetRule != null)
                {
                    existingRule.folderColor = matchedPresetRule.folderColor;
                    existingRule.materialColor = matchedPresetRule.materialColor;
                    existingRule.applyColorToSubfolders = matchedPresetRule.applyColorToSubfolders;
                }
            }
            // DEĞİŞİKLİKLERİ KAYDET
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            // GÖRSELLİĞİ YENİLE
            FolderColors.ClearCache();
            EditorApplication.RepaintProjectWindow();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Preset import error: {e.Message}");
        }
    }
    [System.Serializable]
    private class PresetWrapper
    {
        public List<PresetRule> folderRules;
    }
    [System.Serializable]
    private class PresetRule
    {
        public string folderName;
        public Color folderColor;
        public bool applyColorToSubfolders = true;
        public bool applyIconToSubfolders = false;
        public MaterialColor materialColor = MaterialColor.Custom;
    }
}