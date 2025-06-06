using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Platform
{
  
    [CreateAssetMenu(fileName = "PlatformDataSO", menuName = "Scriptable Objects/PlatformDataSO")]
    public class PlatformDataSO : ScriptableObject
    {
        [BoxGroup("Data Setting")] [Tooltip("Sprite list use to random when spawned")]
        public List<Sprite> sprites = new List<Sprite>();
        [BoxGroup("Data Setting")] [Tooltip("Weight of platform")]
        public float weight;
        [BoxGroup("Data Setting")] [Tooltip("State type each platform")]
        public PlatformBaseStateSO state;
        [BoxGroup("Data Setting")] [Tooltip("Particle when stepped on platform")]
        public ParticleSystem particle;

        public Sprite GetRandomSprite()
        {
            if (sprites == null || sprites.Count == 0)
                return null;

            return sprites[Random.Range(0, sprites.Count)];
        }
    }
}


