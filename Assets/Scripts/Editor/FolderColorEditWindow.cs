using UnityEditor;
using UnityEngine;
using FolderColorNamespace;
public class FolderColorEditWindow : EditorWindow
{
    private string folderPath;
    private Color selectedColor = Color.white;
    private Texture2D selectedIcon;
    private FolderColorSettings settings;
    // Yeni eklenen alanlar
    private bool applyColorToSubfolders;
    private bool applyIconToSubfolders;
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
            // Yeni başlangıç değerleri
            window.applyColorToSubfolders = existingRule.applyColorToSubfolders;
            window.applyIconToSubfolders = existingRule.applyIconToSubfolders;
        }
    }
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Folder Color Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        GUI.enabled = false;
        EditorGUILayout.TextField("Folder Path", folderPath);
        GUI.enabled = true;
        EditorGUILayout.Space(10);
        selectedColor = EditorGUILayout.ColorField("Folder Color", selectedColor);
        selectedIcon = (Texture2D)EditorGUILayout.ObjectField("Folder Icon", selectedIcon, typeof(Texture2D), false);
        // Yeni recursive seçenekler
        EditorGUILayout.Space(10);
        applyColorToSubfolders = EditorGUILayout.Toggle("Apply Color to Subfolders", applyColorToSubfolders);
        applyIconToSubfolders = EditorGUILayout.Toggle("Apply Icon to Subfolders", applyIconToSubfolders);
        EditorGUILayout.Space(20);
        if (GUILayout.Button("Save"))
        {
            SaveFolderRule();
            Close();
        }
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Edit All Settings"))
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
            existingRule.applyColorToSubfolders = applyColorToSubfolders;
            existingRule.applyIconToSubfolders = applyIconToSubfolders;
        }
        else
        {
            var newRule = new FolderRule
            {
                folderName = folderName,
                folderColor = selectedColor,
                icon = selectedIcon,
                applyColorToSubfolders = applyColorToSubfolders,
                applyIconToSubfolders = applyIconToSubfolders
            };
            settings.folderRules.Add(newRule);
        }
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }
}