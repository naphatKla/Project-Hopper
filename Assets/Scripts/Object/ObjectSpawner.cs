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
        
        [Tooltip("Amount of object to spawn and used by pooling")]
        public float poolingAmount;
    }
    public class ObjectSpawner : MonoBehaviour , ISpawner
    {
        [FoldoutGroup("Object Context")] [Tooltip("Object data list")]
        public List<ObjectSetting> objectDatas;
       
        [FoldoutGroup("Object Context")] [SerializeField] [Tooltip("Object parent")]
        private Transform parent;

        public event Action<GameObject> OnSpawned;
        public event Action<GameObject> OnDespawned;
        
        public void PreWarm()
        {
            
        }

        public void ClearData()
        {
            
        }
        
        public void TrySpawnObjectOnPlatform(GameObject platform)
        {
            var data = platform.GetComponent<PlatformManager>()?.currentState;
            if (data == null || !data.ObjectCanSpawn) return;
        }

        public void Spawn(Vector3 position, object settings = null)
        {
            
        }

        public void Despawn(GameObject obj)
        {
            
        }
    }
}

