using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Controllers;
using Characters.HealthSystems;
using Platform;
using PoolingSystem;
using Sirenix.OdinInspector;
using Spawner.Platform;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spawner.Object
{
    #region ObjectSetting
    [Serializable]
    [InlineProperty]
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

        [Tooltip("if this enable mean it will not spawn near falling or danger platform")]
        public bool mustSafeBeforeSpawn;
        
        [Tooltip("if this enable mean it will not calculate attemp")]
        public bool bypassAttemp;
        
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
        [FoldoutGroup("Object Data")] [Tooltip("Object data list")]
        [TableList(ShowIndexLabels = true)]
        [SerializeField] private ObjectSetting[] objectDatas;
       
        [FoldoutGroup("Object Context")] [Tooltip("Object parent")]
        [SerializeField] private Transform parent;
        
        [FoldoutGroup("Object Context")] [Tooltip("Attemp object to wait before spawn again")]
        [SerializeField] private float attempObject;
      
        private float currentAttemp = 0;
        
        private readonly Dictionary<GameObject, int> activeObjectCount = new();
        private readonly Dictionary<GameObject, GameObject> platformObjectMap = new();

        public event Action<GameObject> OnSpawned;
        public event Action<GameObject> OnDespawned;
        private List<ObjectSetting> ObjectDataList => objectDatas.ToList();

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
            activeObjectCount.Clear();
            platformObjectMap.Clear();
        }
        
        /// <summary>
        /// Check the type of platform to despawn
        /// </summary>
        /// <param name="platform"></param>
        public void TrySpawnObjectOnPlatform(GameObject platform)
        {
            if (!IsValidPlatform(platform)) return;
     
            //Get properties
            var platformManager = platform.GetComponent<PlatformManager>();
            var platformState = platformManager.data.state;

            //Find type of platform and check it
            var validSettings = GetValidSettings(platformState);
            if (validSettings.Count == 0) return;

            //Random chance
            var selectedSetting = GetRandomChanceObject(validSettings);
            if (selectedSetting == null) return;
            
            //Check left of this platform is normal for player
            if (selectedSetting.mustSafeBeforeSpawn)
            { if (!IsLeftPlatformNormal(platform)) return; }
            
            //Bypass attemp object
            //Check Attemp to prevent it spawn next to each other
            if (CalculateAttemp(selectedSetting)) return;
            
            //Spawn and set position
            var spawnPos = (Vector2)platform.transform.position + Vector2.up * selectedSetting.yOffset;
            Spawn(platform, spawnPos, selectedSetting);
        }
        
        /// <summary>
        /// Calculate attemp to prevent it spawn next to each other
        /// </summary>
        private bool CalculateAttemp(ObjectSetting selectedSetting)
        {
            if (selectedSetting.bypassAttemp) return false;
            if (currentAttemp > 0)
            {
                currentAttemp-- ; return true;
            }
            currentAttemp = attempObject;
            return false;
        }

        /// <summary>
        /// Spawn the object
        /// </summary>
        /// <param name="platform"></param>
        /// <param name="position"></param>
        /// <param name="settings"></param>
        public void Spawn(GameObject platform,Vector2 position, object settings = null)
        {
            var objectSetting = settings as ObjectSetting ?? GetRandomChanceObject(ObjectDataList);
            if (objectSetting == null) { return; }
            
            //Check pool is full
            if (activeObjectCount.GetValueOrDefault(objectSetting.objectPrefab, 0) >= objectSetting.poolingAmount) { return; }
                
            var obj = PoolingManager.Instance.Spawn(objectSetting.objectPrefab, position, Quaternion.identity, parent);
            if (obj == null) return;
            var poolingDespawn = obj.GetComponent<PoolingDespawn>();

            //Add Event on spawn and despawn
            AddListener(obj , poolingDespawn);
            
            activeObjectCount[objectSetting.objectPrefab] = activeObjectCount.GetValueOrDefault(objectSetting.objectPrefab, 0) + 1;
            poolingDespawn.currentPlatform = platform;
            platformObjectMap[platform] = obj;
            OnSpawned?.Invoke(obj);
        }
        
        /// <summary>
        /// Add listener to object
        /// </summary>
        /// <param name="itemObj"></param>
        /// <param name="poolingDespawn"></param>
        public void AddListener(GameObject itemObj, PoolingDespawn poolingDespawn)
        {
            poolingDespawn.OnObjectDespawnedEvent.RemoveAllListeners();
            poolingDespawn.OnObjectSpawnedEvent.RemoveAllListeners();
            poolingDespawn.OnObjectSpawnedEvent.AddListener((obj) => OnSpawned?.Invoke(itemObj));
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
            
            foreach (var setting in objectDatas)
            {
                if (activeObjectCount.ContainsKey(setting.objectPrefab) && obj.name.Contains(setting.objectPrefab.name))
                {
                    activeObjectCount[setting.objectPrefab] = Mathf.Max(0, activeObjectCount[setting.objectPrefab] - 1); break;
                }
            }
        }
        
        /// <summary>
        /// Trigger when platform despawn
        /// </summary>
        /// <param name="platform"></param>
        public void OnPlatformDespawned(GameObject platform)
        {
            if (platformObjectMap.TryGetValue(platform, out var obj))
            {
                Despawn(obj);
                platformObjectMap.Remove(platform);
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
        
        /// <summary>
        /// Get platform manager
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        private bool IsValidPlatform(GameObject platform)
        {
            var pm = platform.GetComponent<PlatformManager>();
            return pm != null && pm.data != null;
        }

        /// <summary>
        /// Check left of platform is normal
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        private bool IsLeftPlatformNormal(GameObject platform)
        {
            var spawner = GetComponent<PlatformSpawner>();
            if (spawner == null) return false;
            var leftPlatform = spawner.CheckPreviousPlatform(platform);
            if (leftPlatform == null) return true;

            var leftPm = leftPlatform.GetComponent<PlatformManager>();
            if (leftPm == null || leftPm.data == null) return false;

            return leftPm.data.state is PlatformNormalStateSO;
        }

        /// <summary>
        /// Get valid object that can spawn on custom platform type
        /// </summary>
        /// <param name="platformState"></param>
        /// <returns></returns>
        private List<ObjectSetting> GetValidSettings(PlatformBaseStateSO platformState)
        {
            return objectDatas
                .Where(setting => CanSpawnOn(platformState, setting))
                .ToList();
        }
        #endregion
    }
}

