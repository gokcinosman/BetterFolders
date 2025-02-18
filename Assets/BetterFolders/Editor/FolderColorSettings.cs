using System.Collections.Generic;
using UnityEngine;
namespace BetterFolders
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
            Mouse2,
            Mouse3
        }
        [HideInInspector]
        public ModifierKeyType modifierKey = ModifierKeyType.Mouse2;
        public List<FolderRule> folderRules = new List<FolderRule>();
    }
    [System.Serializable]
    public class FolderRule
    {
        public string folderName;
        public Color folderColor;
        public string fullPath;
        public Texture2D icon;
        public bool applyToAllFolders = true;
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
