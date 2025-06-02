using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum PlatformType
{
    Normal,
    Falling,
    Broken,
    TNT,
    Spear
}

[CreateAssetMenu(fileName = "PlatformDataSO", menuName = "Scriptable Objects/PlatformDataSO")]
public class PlatformDataSO : ScriptableObject
{
    public List<Sprite> sprites = new List<Sprite>();
    public PlatformType platformType = PlatformType.Normal;
    public Action OnSteppedAction;
}
