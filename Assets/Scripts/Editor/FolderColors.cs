using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
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
        // Modifier tuş kontrolü ve menüyü aç
        if (Event.current.type == EventType.MouseDown &&
            Event.current.button == 0 &&
            rect.Contains(Event.current.mousePosition) &&
            IsModifierPressed())
        {
            ShowFolderMenu(guid);
            Event.current.Use();
        }
    }
    private static void ApplyFolderStyle(Rect rect, FolderRule rule)
    {
        // Arkaplan rengi
        EditorGUI.DrawRect(rect, rule.folderColor);
        // Sağ alt köşe ikonu
        var iconRect = new Rect(rect.xMax - 20, rect.yMax - 20, 20, 20);
        GUI.DrawTexture(iconRect, rule.icon);
    }
    private static bool IsModifierPressed()
    {
        return Event.current.modifiers == EventModifiers.Alt;
    }
    private static void ShowFolderMenu(string guid)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Ayarları Düzenle"), false, () =>
        {
            Selection.activeObject = settings;
        });
        menu.ShowAsContext();
    }
}
