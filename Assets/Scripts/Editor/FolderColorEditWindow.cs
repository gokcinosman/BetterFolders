using UnityEditor;
using UnityEngine;
using FolderColorNamespace;
public class FolderColorEditWindow : EditorWindow
{
    private string folderPath;
    private Color selectedColor = Color.white;
    private Texture2D selectedIcon;
    private FolderColorSettings settings;
    public static void ShowWindow(string path, FolderColorSettings settings)
    {
        var window = GetWindow<FolderColorEditWindow>("Klasör Düzenle");
        window.folderPath = path;
        window.settings = settings;
        window.minSize = new Vector2(300, 200);
        // Mevcut klasör kuralını bul ve ayarları yükle
        string folderName = System.IO.Path.GetFileName(path);
        var existingRule = settings.folderRules.Find(r => r.folderName == folderName);
        if (existingRule != null)
        {
            window.selectedColor = existingRule.folderColor;
            window.selectedIcon = existingRule.icon;
        }
    }
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Klasör Ayarları", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        GUI.enabled = false;
        EditorGUILayout.TextField("Klasör Yolu", folderPath);
        GUI.enabled = true;
        EditorGUILayout.Space(10);
        selectedColor = EditorGUILayout.ColorField("Klasör Rengi", selectedColor);
        selectedIcon = (Texture2D)EditorGUILayout.ObjectField("Klasör İkonu", selectedIcon, typeof(Texture2D), false);
        EditorGUILayout.Space(20);
        if (GUILayout.Button("Kaydet"))
        {
            SaveFolderRule();
            Close();
        }
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Tüm Ayarları Düzenle"))
        {
            Selection.activeObject = settings;
            Close();
        }
    }
    private void SaveFolderRule()
    {
        string folderName = System.IO.Path.GetFileName(folderPath);
        // Mevcut kuralı kontrol et
        var existingRule = settings.folderRules.Find(r => r.folderName == folderName);
        if (existingRule != null)
        {
            existingRule.folderColor = selectedColor;
            existingRule.icon = selectedIcon;
        }
        else
        {
            var newRule = new FolderRule
            {
                folderName = folderName,
                folderColor = selectedColor,
                icon = selectedIcon
            };
            settings.folderRules.Add(newRule);
        }
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }
}