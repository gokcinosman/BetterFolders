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
}
