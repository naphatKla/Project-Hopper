using System;
using System.Collections.Generic;
using System.Linq;
using Characters.HealthSystems;
using Platform;
using PoolingSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spawner.Object
{
    [Serializable]
    public class ObjectSetting
    {
        [Tooltip("Prefab of object to spawn")]
        public GameObject objectPrefab;

        [Tooltip("Chance of object to random")]
        public float spawnChance;
        
        [Tooltip("Choose which platform that this object can spawn on")]
        [ValueDropdown(nameof(GetAllPlatformStates), IsUniqueList = true)]
        public List<PlatformBaseStateSO> validPlatformTypes;
        
        [Tooltip("Amount of object to spawn and used by pooling")]
        public int poolingAmount;
        
        private static IEnumerable<PlatformBaseStateSO> GetAllPlatformStates()
        {
            return UnityEditor.AssetDatabase
                .FindAssets("t:PlatformBaseStateSO")
                .Select(guid => UnityEditor.AssetDatabase.LoadAssetAtPath<PlatformBaseStateSO>(
                    UnityEditor.AssetDatabase.GUIDToAssetPath(guid)));
        }
    }
    public class ObjectSpawner : MonoBehaviour , ISpawner
    {
        [FoldoutGroup("Object Context")] [Tooltip("Object data list")]
        public List<ObjectSetting> objectDatas;
       
        [FoldoutGroup("Object Context")] [SerializeField] [Tooltip("Object parent")]
        private Transform parent;
        
        private readonly Dictionary<GameObject, GameObject> platformObjectMap = new();
        private readonly Dictionary<GameObject, int> activeObjectCount = new();

        public event Action<GameObject> OnSpawned;
        public event Action<GameObject> OnDespawned;
        
        public void PreWarm()
        {
            foreach (var data in objectDatas)
            {
                PoolingManager.Instance.PreWarm(data.objectPrefab, data.poolingAmount, parent);
                activeObjectCount[data.objectPrefab] = 0;
            }
        }

        public void ClearData()
        {
            platformObjectMap.Clear();
            activeObjectCount.Clear();
        }
        
        /// <summary>
        /// Check the type of platform to despawn
        /// </summary>
        /// <param name="platform"></param>
        public void TrySpawnObjectOnPlatform(GameObject platform)
        {
            var platformManager = platform.GetComponent<PlatformManager>();
            if (platformManager == null || platformManager.data == null) return;

            var platformState = platformManager.data.state;
            
            var validSettings = objectDatas
                .Where(setting => CanSpawnOn(platformState, setting))
                .ToList();

            if (validSettings.Count == 0) return;

            var selectedSetting = GetRandomChanceObject(validSettings);
            if (selectedSetting == null) return;

            var spawnPos = platform.transform.position + Vector3.up * 0.55f;
            Spawn(platform, spawnPos, selectedSetting);
        }


        /// <summary>
        /// Despawn object when platform despawn
        /// </summary>
        /// <param name="platform"></param>
        public void OnPlatformDespawned(GameObject platform)
        {
            if (platformObjectMap.TryGetValue(platform, out var obj))
            {
                Despawn(obj);
            }
        }

        /// <summary>
        /// Spawn the object
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="position"></param>
        /// <param name="settings"></param>
        public void Spawn(GameObject platform,Vector2 position, object settings = null)
        {
            var objectSetting = settings as ObjectSetting ?? GetRandomChanceObject(objectDatas);
            if (objectSetting == null) { return; }
            
            if (activeObjectCount.TryGetValue(objectSetting.objectPrefab, out var currentCount) &&
                currentCount >= objectSetting.poolingAmount) { return; }
            
            var obj = PoolingManager.Instance.Spawn(objectSetting.objectPrefab, position, Quaternion.identity, parent);
            if (obj == null) return;
          
            activeObjectCount[objectSetting.objectPrefab]++;
            platformObjectMap[platform] = obj;
            OnSpawned?.Invoke(obj);
        }

        /// <summary>
        /// Despawn object
        /// </summary>
        /// <param name="obj"></param>
        public void Despawn(GameObject obj)
        {
            if (obj == null) return;
            PoolingManager.Instance.Despawn(obj);
            OnDespawned?.Invoke(obj);

            foreach (var pair in platformObjectMap)
            { if (pair.Value == obj) { platformObjectMap.Remove(pair.Key); break; } }

            if (activeObjectCount.ContainsKey(obj))
            { activeObjectCount[obj] = Mathf.Max(0, activeObjectCount[obj] - 1); }
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
                if (Random.value <= obj.spawnChance / 100f)
                    passed.Add(obj);
            
            if (passed.Count == 0) { return null; }
            return passed[Random.Range(0, passed.Count)];
        }
        
        /// <summary>
        /// Check that can spawn on platform
        /// </summary>
        /// <param name="platformState"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public bool CanSpawnOn(PlatformBaseStateSO platformState, ObjectSetting setting)
        {
            return setting.validPlatformTypes.Any(validState => platformState.GetType() == validState.GetType());
        }

    }
}

