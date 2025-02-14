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
        if (rule.icon == null) return;
        bool isTreeView = rect.height <= 20f;
        float defaultFolderIconSize = isTreeView ? 16f : 64f;
        // İkon boyutunu dinamik olarak hesapla
        float overlayIconSize;
        if (isTreeView)
        {
            overlayIconSize = defaultFolderIconSize * 0.75f; // Tree görünümünde varsayılan boyutun yarısı
        }
        else
        {
            // Grid görünümünde rect boyutuna göre orantılı (minimum 24, maksimum 48)
            overlayIconSize = Mathf.Clamp(rect.width * 0.3f, 24f, 64f);
        }
        float paddingBottom = isTreeView ? 2f : 8f;
        float paddingRight = isTreeView ? 4f : 16f;
        if (isTreeView)
        {
            Rect iconRect = new Rect(
                rect.x + defaultFolderIconSize - overlayIconSize - paddingRight,
                rect.y + defaultFolderIconSize - overlayIconSize - paddingBottom,
                overlayIconSize,
                overlayIconSize
            );
            GUI.DrawTexture(iconRect, rule.icon, ScaleMode.ScaleToFit);
        }
        else
        {
            Rect iconRect = new Rect(
                rect.x + rect.width - overlayIconSize - paddingRight,
                rect.y + (rect.height * 0.75f) - overlayIconSize - paddingBottom,
                overlayIconSize,
                overlayIconSize
            );
            GUI.DrawTexture(iconRect, rule.icon, ScaleMode.ScaleToFit);
        }
    }
    private static Texture2D CombineTextures(Texture2D background, Texture2D overlay)
    {
        // Texture'ları okunabilir hale getir
        RenderTexture tmp = RenderTexture.GetTemporary(background.width, background.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(background, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;
        Texture2D readableBg = new Texture2D(background.width, background.height);
        readableBg.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        readableBg.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);
        // Overlay'i boyutlandır ve çiz
        Texture2D resizedOverlay = ResizeTexture(overlay, background.width / 2, background.height / 2);
        Color[] bgPixels = readableBg.GetPixels();
        Color[] overlayPixels = resizedOverlay.GetPixels();
        // Pixel birleştirme
        int startX = background.width - resizedOverlay.width;
        int startY = background.height - resizedOverlay.height;
        for (int x = 0; x < resizedOverlay.width; x++)
        {
            for (int y = 0; y < resizedOverlay.height; y++)
            {
                int bgIndex = (startY + y) * background.width + (startX + x);
                int overlayIndex = y * resizedOverlay.width + x;
                if (overlayPixels[overlayIndex].a > 0.1f)
                    bgPixels[bgIndex] = overlayPixels[overlayIndex];
            }
        }
        // Yeni texture oluştur
        Texture2D combined = new Texture2D(background.width, background.height);
        combined.SetPixels(bgPixels);
        combined.Apply();
        return combined;
    }
    private static Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Bilinear;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(newWidth, newHeight);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }
    private static float GetIconSizeBasedOnView(Rect rect)
    {
        // Görünüm tipini belirle (List vs Grid)
        bool isTreeView = rect.height <= 20f; // Unity'nin tree view yüksekliği
        return isTreeView ? 16f : Mathf.Min(rect.width, rect.height) * 0.5f;
    }
    private static Rect CalculateIconPosition(Rect mainRect, float iconSize, float padding, bool hasFolderIcon)
    {
        // Klasör ikonu varsa merkeze, yoksa sağ alta yerleştir
        if (hasFolderIcon)
        {
            return new Rect(
                mainRect.center.x - iconSize / 2,
                mainRect.center.y - iconSize / 2,
                iconSize,
                iconSize
            );
        }
        else
        {
            return new Rect(
                mainRect.xMax - iconSize - padding,
                mainRect.yMax - iconSize - padding,
                iconSize,
                iconSize
            );
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
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Ayarları Düzenle"), false, () =>
        {
            Selection.activeObject = settings;
        });
        menu.ShowAsContext();
    }
}
public class FolderColorSettings : ScriptableObject
{
    public enum ModifierKeyType
    {
        LeftAlt,
        RightAlt,
        LeftControl,
        RightControl,
        LeftShift,
        RightShift,
        LeftCommand,
        RightCommand,
        Mouse2,    // Orta tıklama
        Mouse3     // Ekstra fare tuşu
    }
    public ModifierKeyType modifierKey = ModifierKeyType.LeftAlt;
    public List<FolderRule> folderRules = new List<FolderRule>();
}
