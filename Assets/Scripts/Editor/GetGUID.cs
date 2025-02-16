using UnityEditor;
using UnityEngine;
public static class GetGUID
{
    [MenuItem("Assets/Get GUID")]
    private static void ShowGUID()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        var guid = AssetDatabase.AssetPathToGUID(path);
        EditorGUIUtility.systemCopyBuffer = guid;
        Debug.Log($"GUID: {guid}\nPath: {path}\n(Clipboard'a kopyalandı)");
    }
}