using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace FolderColorNamespace
{
    [System.Serializable]
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
    [System.Serializable]
    public class FolderRule
    {
        public string folderName;
        public Color folderColor;
        public Texture2D icon;
        public bool applyColorToSubfolders;
        public bool applyIconToSubfolders;
        public MaterialColor materialColor = MaterialColor.Custom;
    }
    public enum MaterialColor
    {
        Custom,
        Red,
        Pink,
        Purple,
        DeepPurple,
        Indigo,
        Blue,
        LightBlue,
        Cyan,
        Teal,
        Green,
        LightGreen,
        Lime,
        Yellow,
        Amber,
        Orange,
        DeepOrange,
        Brown,
        Grey,
        BlueGrey
    }
}
