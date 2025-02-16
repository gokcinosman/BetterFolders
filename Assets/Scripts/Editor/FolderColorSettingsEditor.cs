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
    public override void OnInspectorGUI()
    {
        var settings = target as FolderColorSettings;
        EditorGUILayout.LabelField("Control Settings", EditorStyles.boldLabel);
        settings.modifierKey = (FolderColorSettings.ModifierKeyType)EditorGUILayout.EnumPopup(
            "Modifier Key",
            settings.modifierKey
        );
        // Yeni preset yükleme butonu
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
        base.OnInspectorGUI();
    }
    private void LoadPreset(FolderColorSettings settings, PresetData preset)
    {
        var path = AssetDatabase.GUIDToAssetPath(preset.Guid);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError($"Preset bulunamadı! GUID: {preset.Guid}");
            return;
        }
        var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Geçersiz preset dosyası: {path}");
            return;
        }
        try
        {
            var presetRules = JsonUtility.FromJson<PresetWrapper>(textAsset.text).folderRules;
            Undo.RecordObject(settings, "Apply Preset Colors");
            // MEVCUT TÜM KURALLARI RENK BAZLI GÜNCELLE
            foreach (var existingRule in settings.folderRules)
            {
                // Presetteki eşleşen kuralı bul
                var matchedPresetRule = presetRules.FirstOrDefault(p =>
                    p.folderName.Equals(existingRule.folderName, StringComparison.OrdinalIgnoreCase));
                if (matchedPresetRule != null)
                {
                    // SADECE RENK VE İLGİLİ AYARLARI GÜNCELLE
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
            Debug.Log($"Preset renkleri başarıyla uygulandı: {preset.Name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Preset yükleme hatası: {e.Message}");
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