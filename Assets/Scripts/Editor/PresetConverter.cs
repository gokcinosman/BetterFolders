using UnityEditor;
using UnityEngine;
public static class PresetConverter
{
    [MenuItem("Tools/Convert Old Presets")]
    public static void ConvertPresets()
    {
        TextAsset[] oldPresets = Resources.LoadAll<TextAsset>("Presets");
        foreach (var preset in oldPresets)
        {
            string newJson = preset.text
                .Replace("\"m_list\"", "\"folderRules\"")
                .Replace("\"m_folderName\"", "\"folderName\"")
                .Replace("\"m_color\"", "\"folderColor\"")
                .Replace("\"R\"", "\"r\"")
                .Replace("\"G\"", "\"g\"")
                .Replace("\"B\"", "\"b\"")
                .Replace("\"A\"", "\"a\"");
            System.IO.File.WriteAllText(AssetDatabase.GetAssetPath(preset), newJson);
        }
        AssetDatabase.Refresh();
        Debug.Log($"Converted {oldPresets.Length} presets");
    }
}