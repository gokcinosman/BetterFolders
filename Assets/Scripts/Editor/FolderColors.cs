using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using FolderColorNamespace;
using System.IO;
using System.Linq;
[System.Serializable]
public class FolderRule
{
    public string folderName;
    public Color folderColor;
    public Texture2D icon;
    public bool applyColorToSubfolders;
    public bool applyIconToSubfolders;
}
[InitializeOnLoad]
public static class FolderColors
{
    private static FolderColorSettings settings;
    private const string settingsPath = "Assets/Resources/FolderColorSettings.asset";
    private static Dictionary<string, Texture2D> combinedIconsCache = new Dictionary<string, Texture2D>();
    static FolderColors()
    {
        LoadSettings();
        EditorApplication.projectWindowItemOnGUI += HandleProjectWindowItem;
    }
    private static Texture2D m_folderImageCache;
    private static Texture2D FolderImage
    {
        get
        {
            if (m_folderImageCache != null) return m_folderImageCache;
            var imagePath = AssetDatabase.GUIDToAssetPath("d66445f0899e03442aba34473aee7242");
            m_folderImageCache = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            return m_folderImageCache;
        }
    }
    private static void LoadSettings()
    {
        settings = AssetDatabase.LoadAssetAtPath<FolderColorSettings>(settingsPath);
        if (settings == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                AssetDatabase.Refresh();
            }
            settings = ScriptableObject.CreateInstance<FolderColorSettings>();
            AssetDatabase.CreateAsset(settings, settingsPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!AssetDatabase.LoadAssetAtPath<FolderColorSettings>(settingsPath))
            {
                Debug.LogError("FolderColorSettings could not be created! Please check manually.");
            }
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
            bool shouldApplyColor = (isDirectMatch || (rule.applyColorToSubfolders && isParentFolder));
            bool shouldApplyIcon = (isDirectMatch || (rule.applyIconToSubfolders && isParentFolder));
            if (shouldApplyColor || shouldApplyIcon)
            {
                ApplyFolderStyle(rect, rule, shouldApplyColor, shouldApplyIcon);
                break;
            }
        }
        // Mouse2/3 için özel kontrol
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
        bool isTreeView = rect.height <= 20f;
        // Renk karışım oranını ayarla
        Color blendedColor = rule.folderColor;
        blendedColor.a = 0.85f; // DÜZELTME: Alpha değerini sabitledik
        if (applyColor && FolderImage != null)
        {
            GUI.DrawTexture(
                GetImagePosition(rect),
                FolderImage,
                ScaleMode.StretchToFill,
                true,
                0,
                blendedColor,
                0,
                0
            );
        }
        // Özel ikonu çiz (eğer varsa)
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
    private static void SetFolderColor(string guid, Rect rect)
    {
        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrWhiteSpace(assetPath)) return;
        if (!AssetDatabase.LoadAssetAtPath<FolderColorSettings>(assetPath))
        {
            Debug.LogError($"Settings file not found: {assetPath}");
            return;
        }
        var folderName = Path.GetFileNameWithoutExtension(assetPath);
        var data = settings.folderRules.FirstOrDefault(x => x.folderName == folderName);
        if (data == null) return;
        GUI.DrawTexture(
            position: GetImagePosition(rect),
            image: FolderImage,
            scaleMode: ScaleMode.StretchToFill,
            alphaBlend: true,
            imageAspect: 0,
            color: data.folderColor,
            borderWidth: 0,
            borderRadius: 0
        );
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
}
