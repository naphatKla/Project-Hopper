using System;
using System.Collections.Generic;
using Platform;
using PoolingSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Spawner.Object
{
    [Serializable]
    public class ObjectSetting
    {
        [Tooltip("Prefab of object to spawn")]
        public GameObject objectPrefab;

        [Tooltip("Weight of object to random")]
        public float weight;
    }
    public class ObjectSpawner : MonoBehaviour
    {
        [FoldoutGroup("Object Context")] [Tooltip("Object data list")]
        public List<ObjectSetting> objectDatas;
       
        [FoldoutGroup("Object Context")] [SerializeField] [Tooltip("Object parent")]
        private Transform parent;

        public event Action<GameObject> OnObjectSpawned;
        public event Action<GameObject> OnObjectDespawned;
        
        public void TrySpawnObjectOnPlatform(GameObject platform)
        {
            var data = platform.GetComponent<PlatformManager>()?.currentState;
            if (data == null || !data.ObjectCanSpawn) return;
        }
    }
}

