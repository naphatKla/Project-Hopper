using System;
using System.Collections.Generic;
using Platform;
using PoolingSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spawner.Object
{
    [System.Flags]
    public enum PlatformType
    {
        None = 0,
        Normal = 1 << 0,
        Falling = 1 << 1,
        Broken = 1 << 2,
        Spear = 1 << 3,
    }
    
    [Serializable]
    public class ObjectSetting
    {
        [Tooltip("Prefab of object to spawn")]
        public GameObject objectPrefab;

        [Tooltip("Chance of object to random")]
        public float spawnChance;
        
        [EnumToggleButtons, HideLabel] [Tooltip("Choose which platform that this object can spawn on")]
        public PlatformType validPlatformTypes;
        
        [Tooltip("Amount of object to spawn and used by pooling")]
        public int poolingAmount;
    }
    public class ObjectSpawner : MonoBehaviour , ISpawner
    {
        [FoldoutGroup("Object Context")] [Tooltip("Object data list")]
        public List<ObjectSetting> objectDatas;
       
        [FoldoutGroup("Object Context")] [SerializeField] [Tooltip("Object parent")]
        private Transform parent;
        
        private readonly Dictionary<GameObject, GameObject> platformObjectMap = new();

        public event Action<GameObject> OnSpawned;
        public event Action<GameObject> OnDespawned;
        
        public void PreWarm()
        {
            foreach (var data in objectDatas)
            {
                PoolingManager.Instance.PreWarm(data.objectPrefab, data.poolingAmount, parent);
            }
        }

        public void ClearData()
        {
            platformObjectMap.Clear();
        }
        
        public void TrySpawnObjectOnPlatform(GameObject platform)
        {
            /*var platformManager = platform.GetComponent<PlatformManager>();
            if (platformManager == null) return;
            
            //Flag select
            var typeName = platformManager.data.platformType.ToString();
            var availableObjects = objectDatas.FindAll(obj => obj.validPlatformTypes.ToString() == typeName);
            if (availableObjects.Count == 0) return;
            var chosen = availableObjects[Random.Range(0, availableObjects.Count)];
            
            //Spawn
            var position = platform.transform.position + Vector3.up * 0.5f;
            var obj = PoolingManager.Instance.Spawn(chosen.objectPrefab, position, Quaternion.identity, parent);
            if (obj == null) return;

            platformObjectMap[platform] = obj;
            OnSpawned?.Invoke(obj);*/
        }

        
        public void OnPlatformDespawned(GameObject platform)
        {
            if (platformObjectMap.TryGetValue(platform, out var obj))
            {
                Despawn(obj);
            }
        }

        public void Spawn(Vector3 position, object settings = null)
        {
            var objectSetting = settings as ObjectSetting ?? GetRandomChanceObject(objectDatas);
            var obj = PoolingManager.Instance.Spawn(objectSetting.objectPrefab, position, Quaternion.identity, parent);
            if (obj == null) return;
          
            OnSpawned?.Invoke(obj);
        }

        public void Despawn(GameObject obj)
        {
            if (obj == null) return;
            PoolingManager.Instance.Despawn(obj);
            OnDespawned?.Invoke(obj);
            
            foreach (var pair in platformObjectMap)
            {
                if (pair.Value == obj)
                {
                    platformObjectMap.Remove(pair.Key);
                    break;
                }
            }
        }

        /// <summary>
        /// Random object from chance
        /// </summary>
        /// <param name="objectDataList"></param>
        /// <returns></returns>
        private ObjectSetting GetRandomChanceObject(List<ObjectSetting> objectDataList)
        {
            List<ObjectSetting> passed = new();

            foreach (var obj in objectDataList)
                if (Random.value <= obj.spawnChance)
                    passed.Add(obj);
            
            return passed[Random.Range(0, passed.Count)];
        }
    }
}

