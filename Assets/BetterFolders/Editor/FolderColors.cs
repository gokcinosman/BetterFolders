using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace BetterFolders
{
    [InitializeOnLoad]
    public static class FolderColors
    {
        private static FolderColorSettings settings;
        private const string settingsPath = "Assets/BetterFolders/Resources/FolderColorSettings.asset";
        private static Dictionary<string, Texture2D> combinedIconsCache = new Dictionary<string, Texture2D>();
        static FolderColors()
        {
            EditorApplication.delayCall += () =>
            {
                LoadSettings();
                EditorApplication.projectWindowItemOnGUI += HandleProjectWindowItem;
            };
        }
        private static Texture2D m_folderImageCache;
        private static Texture2D FolderImage
        {
            get
            {
                if (m_folderImageCache != null) return m_folderImageCache;
                var imagePath = AssetDatabase.GUIDToAssetPath("ad63eb4211812f646a7d48663be817d9");
                m_folderImageCache = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
                return m_folderImageCache;
            }
        }
        private static void LoadSettings()
        {
            settings = AssetDatabase.LoadAssetAtPath<FolderColorSettings>(settingsPath);
            if (settings == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/BetterFolders/Resources"))
                {
                    Directory.CreateDirectory("Assets/BetterFolders");
                    AssetDatabase.CreateFolder("Assets/BetterFolders", "Resources");
                    AssetDatabase.Refresh();
                }
                settings = ScriptableObject.CreateInstance<FolderColorSettings>();
                AssetDatabase.CreateAsset(settings, settingsPath);
                EditorApplication.delayCall += () =>
                {
                    // Package içindeki preset dosyasının yolunu kontrol et
                    string[] possiblePaths = new string[]
                    {
                    "Assets/BetterFolders/Resources/Presets/DefaultFolderRules.json",
                    };
                    TextAsset defaultPreset = null;
                    string foundPath = "";
                    foreach (var path in possiblePaths)
                    {
                        AssetDatabase.Refresh();
                        defaultPreset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                        if (defaultPreset != null)
                        {
                            foundPath = path;
                            Debug.Log($"Preset found: {path}");
                            break;
                        }
                    }
                    if (defaultPreset != null)
                    {
                        try
                        {
                            var importedRules = JsonUtility.FromJson<FolderColorSettingsEditor.PresetWrapper>(defaultPreset.text).folderRules;
                            if (importedRules != null && importedRules.Count > 0)
                            {
                                settings.folderRules = importedRules.Select(r => new FolderRule
                                {
                                    folderName = r.folderName,
                                    folderColor = r.folderColor,
                                    materialColor = r.materialColor,
                                    applyColorToSubfolders = r.applyColorToSubfolders,
                                    applyIconToSubfolders = r.applyIconToSubfolders,
                                    icon = !string.IsNullOrEmpty(r.iconGuid) ?
                                        AssetDatabase.LoadAssetAtPath<Texture2D>(
                                            AssetDatabase.GUIDToAssetPath(r.iconGuid)
                                        ) : null
                                }).ToList();
                                Debug.Log($"Preset loaded successfully: {foundPath}, Rule count: {settings.folderRules.Count}");
                            }
                            else
                            {
                                Debug.LogWarning($"Preset is empty or invalid: {foundPath}");
                                settings.folderRules = new List<FolderRule>();
                            }
                            EditorUtility.SetDirty(settings);
                            AssetDatabase.SaveAssets();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error loading default preset: {e.Message}\nStack Trace: {e.StackTrace}");
                            settings.folderRules = new List<FolderRule>();
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find any preset! Searched paths:\n{string.Join("\n", possiblePaths)}");
                        settings.folderRules = new List<FolderRule>();
                    }
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                };
            }
            if (settings.folderRules == null)
            {
                settings.folderRules = new List<FolderRule>();
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }
        private static void HandleProjectWindowItem(string guid, Rect rect)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!AssetDatabase.IsValidFolder(path)) return;
            var currentFolder = Path.GetFileName(path);
            var hierarchyFolders = path.Split('/').ToList();
            foreach (var rule in settings.folderRules)
            {
                bool isParentFolder = hierarchyFolders.Contains(rule.folderName);
                bool isDirectMatch = currentFolder == rule.folderName;
                bool shouldApply = rule.applyToAllFolders ?
                    isDirectMatch || (rule.applyColorToSubfolders && isParentFolder) :
                    path == rule.fullPath;
                bool shouldApplyIcon = (isDirectMatch || (rule.applyIconToSubfolders && isParentFolder));
                if (shouldApply || shouldApplyIcon)
                {
                    ApplyFolderStyle(rect, rule, shouldApply, shouldApplyIcon);
                    break;
                }
            }
            bool isMouseButtonEvent = settings.modifierKey == FolderColorSettings.ModifierKeyType.Mouse2 ||
                                     settings.modifierKey == FolderColorSettings.ModifierKeyType.Mouse3;
            if (Event.current.type == EventType.MouseDown &&
                (isMouseButtonEvent || Event.current.button == 0) &&
                rect.Contains(Event.current.mousePosition) &&
                IsModifierPressed())
            {
                ShowFolderMenu(guid);
                Event.current.Use();
            }
        }
        private static void ApplyFolderStyle(Rect rect, FolderRule rule, bool applyColor, bool applyIcon)
        {
            Color finalColor = rule.materialColor == MaterialColor.Custom ?
                rule.folderColor :
                FolderColorEditWindow.GetMaterialColor(rule.materialColor);
            bool isTreeView = rect.height <= 20f;
            finalColor.a = 0.85f;
            if (applyColor && FolderImage != null)
            {
                GUI.DrawTexture(
                    GetImagePosition(rect),
                    FolderImage,
                    ScaleMode.StretchToFill,
                    true,
                    0,
                    finalColor,
                    0,
                    0
                );
            }
            if (applyIcon && rule.icon != null)
            {
                if (isTreeView)
                {
                    float overlayIconSize = 10f;
                    float paddingRight = 3f;
                    Rect overlayRect = new Rect(
                        rect.x + 16f - overlayIconSize - paddingRight,
                        rect.y + (rect.height - overlayIconSize) / 2,
                        overlayIconSize,
                        overlayIconSize
                    );
                    GUI.DrawTexture(overlayRect, rule.icon, ScaleMode.ScaleToFit);
                }
                else
                {
                    float gridFolderSize = Mathf.Min(rect.width, rect.height) * 0.75f;
                    float overlayIconSize = gridFolderSize * 0.5f;
                    float folderCenterX = rect.x + (rect.width - gridFolderSize) / 2;
                    float folderCenterY = rect.y + (rect.height * 0.3f);
                    Rect overlayRect = new Rect(
                        folderCenterX + gridFolderSize * 0.5f,
                        folderCenterY,
                        overlayIconSize,
                        overlayIconSize
                    );
                    GUI.DrawTexture(overlayRect, rule.icon, ScaleMode.ScaleToFit);
                }
            }
        }
        private static Rect GetImagePosition(Rect selectionRect)
        {
            var position = selectionRect;
            var isOneColumn = position.height < position.width;
            if (isOneColumn)
            {
                position.width = position.height;
            }
            else
            {
                position.height = position.width;
            }
            return position;
        }
        private static bool IsModifierPressed()
        {
            switch (settings.modifierKey)
            {
                case FolderColorSettings.ModifierKeyType.LeftAlt:
                    return Event.current.modifiers == EventModifiers.Alt;
                case FolderColorSettings.ModifierKeyType.RightAlt:
                    return Event.current.modifiers == EventModifiers.Alt &&
                           Event.current.keyCode == KeyCode.RightAlt;
                case FolderColorSettings.ModifierKeyType.LeftControl:
                    return Event.current.modifiers == EventModifiers.Control;
                case FolderColorSettings.ModifierKeyType.RightControl:
                    return Event.current.modifiers == EventModifiers.Control &&
                           Event.current.keyCode == KeyCode.RightControl;
                case FolderColorSettings.ModifierKeyType.LeftShift:
                    return Event.current.modifiers == EventModifiers.Shift;
                case FolderColorSettings.ModifierKeyType.RightShift:
                    return Event.current.modifiers == EventModifiers.Shift &&
                           Event.current.keyCode == KeyCode.RightShift;
                case FolderColorSettings.ModifierKeyType.LeftCommand:
                    return Event.current.modifiers == EventModifiers.Command;
                case FolderColorSettings.ModifierKeyType.RightCommand:
                    return Event.current.modifiers == EventModifiers.Command &&
                           Event.current.keyCode == KeyCode.RightCommand;
                case FolderColorSettings.ModifierKeyType.Mouse2:
                    return Event.current.button == 2;
                case FolderColorSettings.ModifierKeyType.Mouse3:
                    return Event.current.button == 3;
                default:
                    return false;
            }
        }
        private static void ShowFolderMenu(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Edit Folder"), false, () =>
            {
                FolderColorEditWindow.ShowWindow(path, settings);
            });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Edit All Settings"), false, () =>
            {
                Selection.activeObject = settings;
            });
            menu.ShowAsContext();
        }
        public static void ClearCache()
        {
            combinedIconsCache.Clear();
            m_folderImageCache = null;
            EditorApplication.RepaintProjectWindow();
        }
    }
}