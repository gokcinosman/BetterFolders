using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FolderColorSettings : ScriptableObject
{
    public KeyCode modifierKey = KeyCode.LeftAlt;
    public List<FolderRule> folderRules = new List<FolderRule>();
}
