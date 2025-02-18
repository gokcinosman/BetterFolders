using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
namespace BetterFolders
{
    [CustomEditor(typeof(FolderColorSettings))]
    public class FolderColorSettingsEditor : Editor
    {
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
        new("Natural Tones", "76902034c7116aa4e9eef64f0afa4dca"),
        new("Ocean Tones", "56947363dd205ae4bbe495d43c6d5e24"),
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
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Export Settings"))
                {
                    ExportSettings(settings);
                }
                if (GUILayout.Button("Import Settings"))
                {
                    ImportSettings(settings);
                }
            }
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
            searchText = EditorGUILayout.TextField("Search Folder Name", searchText);
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
                var updatedRules = new List<FolderRule>(settings.folderRules);
                foreach (var presetRule in presetRules)
                {
                    var existingRule = updatedRules.FirstOrDefault(r =>
                        r.folderName.Equals(presetRule.folderName, StringComparison.OrdinalIgnoreCase));
                    if (existingRule != null)
                    {
                        existingRule.folderColor = presetRule.folderColor;
                        existingRule.materialColor = presetRule.materialColor;
                        existingRule.applyColorToSubfolders = presetRule.applyColorToSubfolders;
                        existingRule.applyIconToSubfolders = presetRule.applyIconToSubfolders;
                        if (!string.IsNullOrEmpty(presetRule.iconGuid))
                        {
                            var iconPath = AssetDatabase.GUIDToAssetPath(presetRule.iconGuid);
                            if (!string.IsNullOrEmpty(iconPath))
                            {
                                existingRule.icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
                            }
                        }
                    }
                    else
                    {
                        var newRule = new FolderRule
                        {
                            folderName = presetRule.folderName,
                            folderColor = presetRule.folderColor,
                            materialColor = presetRule.materialColor,
                            applyColorToSubfolders = presetRule.applyColorToSubfolders,
                            applyIconToSubfolders = presetRule.applyIconToSubfolders
                        };
                        if (!string.IsNullOrEmpty(presetRule.iconGuid))
                        {
                            var iconPath = AssetDatabase.GUIDToAssetPath(presetRule.iconGuid);
                            if (!string.IsNullOrEmpty(iconPath))
                            {
                                newRule.icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
                            }
                        }
                        updatedRules.Add(newRule);
                    }
                }
                settings.folderRules = updatedRules;
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                FolderColors.ClearCache();
                EditorApplication.RepaintProjectWindow();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Preset import error: {e.Message}");
            }
        }
        private void ExportSettings(FolderColorSettings settings)
        {
            string path = EditorUtility.SaveFilePanel(
                "Export Settings",
                "",
                "FolderColorSettings.json",
                "json"
            );
            if (!string.IsNullOrEmpty(path))
            {
                var wrapper = new PresetWrapper
                {
                    folderRules = settings.folderRules.Select(r => new PresetRule
                    {
                        folderName = r.folderName,
                        folderColor = r.folderColor,
                        materialColor = r.materialColor,
                        applyColorToSubfolders = r.applyColorToSubfolders,
                        applyIconToSubfolders = r.applyIconToSubfolders,
                        iconGuid = r.icon ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(r.icon)) : null
                    }).ToList()
                };
                string json = JsonUtility.ToJson(wrapper, true);
                System.IO.File.WriteAllText(path, json);
                Debug.Log($"Settings exported successfully: {path}");
            }
        }
        private void ImportSettings(FolderColorSettings settings)
        {
            string path = EditorUtility.OpenFilePanel(
                "Import Settings",
                "",
                "json"
            );
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(path);
                    var importedRules = JsonUtility.FromJson<PresetWrapper>(json).folderRules;
                    Undo.RecordObject(settings, "Import Folder Colors");
                    foreach (var existingRule in settings.folderRules)
                    {
                        var matchedPresetRule = importedRules.FirstOrDefault(p =>
                            p.folderName.Equals(existingRule.folderName, StringComparison.OrdinalIgnoreCase));
                        if (matchedPresetRule != null)
                        {
                            existingRule.folderColor = matchedPresetRule.folderColor;
                            existingRule.materialColor = matchedPresetRule.materialColor;
                            existingRule.applyColorToSubfolders = matchedPresetRule.applyColorToSubfolders;
                            existingRule.applyIconToSubfolders = matchedPresetRule.applyIconToSubfolders;
                            existingRule.icon = !string.IsNullOrEmpty(matchedPresetRule.iconGuid) ?
                                AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(matchedPresetRule.iconGuid)) : null;
                        }
                    }
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssets();
                    FolderColors.ClearCache();
                    EditorApplication.RepaintProjectWindow();
                    Debug.Log($"Settings imported successfully: {path}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Ocurred an error while importing settings: {e.Message}");
                }
            }
        }
        [System.Serializable]
        public class PresetWrapper
        {
            public List<PresetRule> folderRules;
        }
        [System.Serializable]
        public class PresetRule
        {
            public string folderName;
            public Color folderColor;
            public bool applyColorToSubfolders = true;
            public bool applyIconToSubfolders = false;
            public MaterialColor materialColor = MaterialColor.Custom;
            public string iconGuid;
        }
    }
}