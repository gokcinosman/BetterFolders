using UnityEditor;
using UnityEngine;
using FolderColorNamespace;
using System.Collections.Generic;
public class FolderColorEditWindow : EditorWindow
{
    private string folderPath;
    private Color selectedColor = Color.white;
    private Texture2D selectedIcon;
    private FolderColorSettings settings;
    // Yeni eklenen alanlar
    private bool applyColorToSubfolders;
    private bool applyIconToSubfolders;
    private MaterialColor selectedMaterialColor = MaterialColor.Custom;
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
            window.applyColorToSubfolders = existingRule.applyColorToSubfolders;
            window.applyIconToSubfolders = existingRule.applyIconToSubfolders;
            window.selectedMaterialColor = existingRule.materialColor;
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
        selectedMaterialColor = (MaterialColor)EditorGUILayout.EnumPopup("Material Color", selectedMaterialColor);
        if (selectedMaterialColor == MaterialColor.Custom)
        {
            selectedColor = EditorGUILayout.ColorField("Folder Color", selectedColor);
        }
        else
        {
            GUI.enabled = false;
            EditorGUILayout.ColorField("Folder Color", GetMaterialColor(selectedMaterialColor));
            GUI.enabled = true;
        }
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
        Color finalColor = selectedMaterialColor == MaterialColor.Custom ?
            selectedColor :
            GetMaterialColor(selectedMaterialColor);
        var existingRule = settings.folderRules.Find(r => r.folderName == folderName);
        if (existingRule != null)
        {
            existingRule.folderColor = finalColor;
            existingRule.icon = selectedIcon;
            existingRule.applyColorToSubfolders = applyColorToSubfolders;
            existingRule.applyIconToSubfolders = applyIconToSubfolders;
            existingRule.materialColor = selectedMaterialColor;
        }
        else
        {
            var newRule = new FolderColorNamespace.FolderRule
            {
                folderName = folderName,
                folderColor = finalColor,
                icon = selectedIcon,
                applyColorToSubfolders = applyColorToSubfolders,
                applyIconToSubfolders = applyIconToSubfolders,
                materialColor = selectedMaterialColor
            };
            settings.folderRules.Add(newRule);
        }
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }
    public static Color GetMaterialColor(MaterialColor color)
    {
        // Material UI renk paletleri (https://materialui.co/)
        var colorMap = new Dictionary<MaterialColor, string>
        {
            {MaterialColor.Red, "#F44336"},
            {MaterialColor.Pink, "#E91E63"},
            {MaterialColor.Purple, "#9C27B0"},
            {MaterialColor.DeepPurple, "#673AB7"},
            {MaterialColor.Indigo, "#3F51B5"},
            {MaterialColor.Blue, "#2196F3"},
            {MaterialColor.LightBlue, "#03A9F4"},
            {MaterialColor.Cyan, "#00BCD4"},
            {MaterialColor.Teal, "#009688"},
            {MaterialColor.Green, "#4CAF50"},
            {MaterialColor.LightGreen, "#8BC34A"},
            {MaterialColor.Lime, "#CDDC39"},
            {MaterialColor.Yellow, "#FFEB3B"},
            {MaterialColor.Amber, "#FFC107"},
            {MaterialColor.Orange, "#FF9800"},
            {MaterialColor.DeepOrange, "#FF5722"},
            {MaterialColor.Brown, "#795548"},
            {MaterialColor.Grey, "#9E9E9E"},
            {MaterialColor.BlueGrey, "#607D8B"}
        };
        if (colorMap.TryGetValue(color, out string hex))
        {
            ColorUtility.TryParseHtmlString(hex, out Color result);
            return result;
        }
        return Color.white;
    }
}