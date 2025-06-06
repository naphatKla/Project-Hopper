using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Controllers;
using Characters.HealthSystems;
using Platform;
using PoolingSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spawner.Object
{
    #region ObjectSetting
    [Serializable]
    public class ObjectSetting
    {
        [Tooltip("Prefab of object to spawn")]
        public GameObject objectPrefab;

        [Tooltip("Chance of object to random")]
        public float spawnChance;
        
        [Tooltip("Choose which platform that this object can spawn on")]
        [ValueDropdown(nameof(GetAllPlatformStatesDropdown), IsUniqueList = true)]
        public List<PlatformBaseStateSO> validPlatformTypes;
        
        [Tooltip("Increase Height from platform")]
        public float yOffset;
        
        [Tooltip("Amount of object to spawn and used by pooling")]
        public int poolingAmount;
        
        private static IEnumerable<ValueDropdownItem<PlatformBaseStateSO>> GetAllPlatformStatesDropdown()
        {
            return UnityEditor.AssetDatabase
                .FindAssets("t:PlatformBaseStateSO")
                .Select(guid =>
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<PlatformBaseStateSO>(path);
                    string shortName = asset.name;
                    return new ValueDropdownItem<PlatformBaseStateSO>(shortName, asset);
                });
        }

    }
    #endregion
    public class ObjectSpawner : MonoBehaviour , ISpawner
    {
        #region Inspector & Value
        [FoldoutGroup("Object Context")] [Tooltip("Object data list")]
        public List<ObjectSetting> objectDatas;
       
        [FoldoutGroup("Object Context")] [SerializeField] [Tooltip("Object parent")]
        private Transform parent;
        
        [FoldoutGroup("Object Context")] [Tooltip("Attemp object to wait before spawn again")]
        public float attempObject;
        
        private readonly Dictionary<GameObject, GameObject> platformObjectMap = new();
        private readonly Dictionary<GameObject, int> activeObjectCount = new();

        public event Action<GameObject> OnSpawned;
        public event Action<GameObject> OnDespawned;
        #endregion

        #region Public Methods
        /// <summary>
        /// Pre create object pooling
        /// </summary>
        public void PreWarm()
        {
            foreach (var data in objectDatas)
            {
                PoolingManager.Instance.PreWarm(data.objectPrefab, data.poolingAmount, parent);
                activeObjectCount[data.objectPrefab] = 0;
            }
        }

        /// <summary>
        /// Clear data
        /// </summary>
        public void ClearData()
        {
            platformObjectMap.Clear();
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

            var spawnPos = (Vector2)platform.transform.position + Vector2.up * selectedSetting.yOffset;
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
            
            if (activeObjectCount.GetValueOrDefault(objectSetting.objectPrefab, 0) >= objectSetting.poolingAmount)
            {
                Debug.Log($"Cannot spawn {objectSetting.objectPrefab.name}: Pool limit ({objectSetting.poolingAmount}) reached.");
                return;
            }
         
            var obj = PoolingManager.Instance.Spawn(objectSetting.objectPrefab, position, Quaternion.identity, parent);
            if (obj == null) return;
            
            activeObjectCount[objectSetting.objectPrefab] = activeObjectCount.GetValueOrDefault(objectSetting.objectPrefab, 0) + 1;
            var poolingDespawn = obj.GetComponent<PoolingDespawn>();

            poolingDespawn.OnObjectSpawnedEvent.AddListener((obj) => OnSpawned?.Invoke(obj));
            poolingDespawn.OnObjectDespawnedEvent.AddListener((obj) => Despawn(obj));
            
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
            
            var platform = platformObjectMap.FirstOrDefault(p => p.Value == obj).Key;
            if (platform != null)
                platformObjectMap.Remove(platform);
            
            foreach (var setting in objectDatas)
            {
                if (activeObjectCount.ContainsKey(setting.objectPrefab) && obj.name.Contains(setting.objectPrefab.name))
                { activeObjectCount[setting.objectPrefab] = Mathf.Max(0, activeObjectCount[setting.objectPrefab] - 1); break; }
            }
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
        
        #endregion

        #region Private Methods
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
        #endregion
    }
}

