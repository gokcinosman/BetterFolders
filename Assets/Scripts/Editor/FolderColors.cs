using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using FolderColorNamespace;
[System.Serializable]
public class FolderRule
{
    public string folderName;
    public Color folderColor;
    public Texture2D icon;
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
    private static void LoadSettings()
    {
        settings = AssetDatabase.LoadAssetAtPath<FolderColorSettings>(settingsPath);
        if (settings == null)
        {
            // Resources klasörü yoksa oluştur
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
                Debug.LogError("FolderColorSettings oluşturulamadı! Lütfen manuel olarak kontrol edin.");
            }
        }
    }
    private static void HandleProjectWindowItem(string guid, Rect rect)
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
        if (!AssetDatabase.IsValidFolder(path)) return;
        // Klasör rengini ve ikonunu uygula
        foreach (var rule in settings.folderRules)
        {
            if (path.EndsWith(rule.folderName))
            {
                ApplyFolderStyle(rect, rule);
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
    private static void ApplyFolderStyle(Rect rect, FolderRule rule)
    {
        bool isTreeView = rect.height <= 20f;
        // Özel ikonu çiz (eğer varsa)
        if (rule.icon != null)
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
        menu.AddItem(new GUIContent("Klasörü Düzenle"), false, () =>
        {
            FolderColorEditWindow.ShowWindow(path, settings);
        });
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Tüm Ayarları Düzenle"), false, () =>
        {
            Selection.activeObject = settings;
        });
        menu.ShowAsContext();
    }
}
