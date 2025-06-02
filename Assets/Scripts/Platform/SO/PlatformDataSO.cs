using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Platform
{
    [Serializable]
    public enum PlatformType
    {
        Normal,
    }

    [CreateAssetMenu(fileName = "PlatformDataSO", menuName = "Scriptable Objects/PlatformDataSO")]
    public class PlatformDataSO : ScriptableObject
    {
        public List<Sprite> sprites = new List<Sprite>();
        public PlatformType platformType = PlatformType.Normal;
        public float weight;
        public PlatformBaseStateSO state;

        public Sprite GetRandomSprite()
        {
            if (sprites == null || sprites.Count == 0)
                return null;

            return sprites[Random.Range(0, sprites.Count)];
        }
    }
}


