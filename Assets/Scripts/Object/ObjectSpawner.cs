/*
using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Controllers;
using Characters.HealthSystems;
using Platform;
using PoolingSystem;
using Sirenix.OdinInspector;
using Spawner.Controller;
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
        
        /*[Tooltip("Choose which platform that this object can spawn on")]
        [ValueDropdown(nameof(GetAllPlatformStatesDropdown), IsUniqueList = true)]
        public List<PlatformBaseStateSO> validPlatformTypes;#1#
        
        [Tooltip("Increase Height from platform")]
        public float yOffset;
        
        [Tooltip("Amount of object to spawn and used by pooling")]
        public int poolingAmount;

        [Tooltip("if this enable mean it will not spawn near falling or danger platform")]
        public bool mustSafeBeforeSpawn;
        
        [Tooltip("if this enable mean it will not calculate attemp")]
        public bool bypassAttemp;
        
        /*private static IEnumerable<ValueDropdownItem<PlatformBaseStateSO>> GetAllPlatformStatesDropdown()
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
        }#1#

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
            /*if (!IsValidPlatform(platform)) return;
     
            //Get properties
            var platformManager = platform.GetComponent<PlatformManager>();
            var platformState = platformManager.data.state;
            
            //Check first
            bool isLeftSafe = IsLeftPlatformNormal(platform);
            
            //Find type of platform and check it
            var validSettings = GetValidSettings(platformState);
            if (validSettings.Count == 0) return;
            
            //Random chance
            var selectedSetting = GetRandomChanceObject(validSettings);
            if (selectedSetting == null) return;
            
            //Check left of this platform is normal for player
            if (selectedSetting.mustSafeBeforeSpawn && !isLeftSafe) return;
            
            //Bypass attemp object
            //Check Attemp to prevent it spawn next to each other
            if (CalculateAttemp(selectedSetting)) return;
            
            //Spawn and set position
            var spawnPos = (Vector2)platform.transform.position + Vector2.up * selectedSetting.yOffset;
            Spawn(platform, spawnPos, null , selectedSetting);#1#
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
        /// Main method to spawn an object using various levels of override: custom prefab, setting override, or random selection.
        /// Also determines spawn position, validates spawn conditions, and finalizes the spawn process.
        /// </summary>
        public GameObject Spawn(GameObject platform = null, Vector2? position = null, GameObject customPrefab = null, ObjectSetting settingOverride = null)
        {
            var objectSetting = AssignSetting(customPrefab, settingOverride);
            if (objectSetting == null) return null;

            var spawnPos = AssignSpawnPosition(platform, position, objectSetting);
            if (!CanSpawn(objectSetting)) return null;

            var obj = PoolingManager.Instance.Spawn(objectSetting.objectPrefab, parent, spawnPos, Quaternion.identity);
            if (obj == null) return null;

            FinalizeSpawn(obj, platform, objectSetting);

            return obj;
        }
        
        /// <summary>
        /// Resolves the ObjectSetting to use for spawning.
        /// Priority: custom prefab → setting override → random from object data list.
        /// </summary>
        /// <param name="customPrefab">Custom GameObject to match against settings.</param>
        /// <param name="overrideSetting">Manually specified ObjectSetting.</param>
        /// <returns>The resolved ObjectSetting or null if none is valid.</returns>
        private ObjectSetting AssignSetting(GameObject customPrefab, ObjectSetting overrideSetting)
        {
            if (customPrefab != null) return ObjectDataList.FirstOrDefault(x => x.objectPrefab == customPrefab);
            if (overrideSetting != null) return overrideSetting;
            return GetRandomChanceObject(ObjectDataList);
        }

        /// <summary>
        /// Calculates the spawn position based on platform, explicit position, or fallback to spawner's own transform.
        /// </summary>
        /// <param name="platform">Platform GameObject, if any.</param>
        /// <param name="position">Explicit position override, if provided.</param>
        /// <param name="setting">The resolved ObjectSetting (used for yOffset).</param>
        /// <returns>The final position to spawn the object at.</returns>
        private Vector2 AssignSpawnPosition(GameObject platform, Vector2? position, ObjectSetting setting)
        {
            if (position.HasValue) return position.Value;
            if (platform != null) return (Vector2)platform.transform.position + Vector2.up * setting.yOffset;
            return transform.position;
        }

        /// <summary>
        /// Checks if the object associated with the given setting can be spawned,
        /// based on the active pool count versus the allowed pooling amount.
        /// </summary>
        /// <param name="setting">The ObjectSetting to check against.</param>
        /// <returns>True if the object can be spawned; false otherwise.</returns>
        private bool CanSpawn(ObjectSetting setting)
        {
            return activeObjectCount.GetValueOrDefault(setting.objectPrefab, 0) < setting.poolingAmount;
        }

        /// <summary>
        /// Completes the spawning process by registering listeners, updating counters,
        /// assigning platform linkage (if any), and invoking spawn event.
        /// </summary>
        /// <param name="obj">The GameObject that was spawned.</param>
        /// <param name="platform">The platform associated with the spawn (can be null).</param>
        /// <param name="setting">The ObjectSetting used for the spawn.</param>
        private void FinalizeSpawn(GameObject obj, GameObject platform, ObjectSetting setting)
        {
            var poolingDespawn = obj.GetComponent<PoolingDespawn>();
            AddListener(obj, poolingDespawn);

            activeObjectCount[setting.objectPrefab] = activeObjectCount.GetValueOrDefault(setting.objectPrefab, 0) + 1;
            poolingDespawn.currentPlatform = platform;

            if (platform != null)
                platformObjectMap[platform] = obj;

            OnSpawned?.Invoke(obj);
        }
        
        /// <summary>
        /// Find prefab name
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        public ObjectSetting GetSettingByPrefabName(string prefabName)
        {
            return objectDatas.FirstOrDefault(setting =>
                setting.objectPrefab != null &&
                setting.objectPrefab.name.Equals(prefabName, StringComparison.OrdinalIgnoreCase));
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
        /*public bool CanSpawnOn(PlatformBaseStateSO platformState, ObjectSetting setting)
        {
            return setting.validPlatformTypes.Any(validState => platformState.GetType() == validState.GetType());
        }#1#
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
        /*private bool IsValidPlatform(GameObject platform)
        {
            var pm = platform.GetComponent<PlatformManager>();
            return pm != null && pm.data != null;
        }#1#

        /// <summary>
        /// Check left of platform is normal
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        /// <summary>
        /// Checks if the two platforms to the left are of the Normal state.
        /// </summary>
        /// <param name="platform">The current platform to check from.</param>
        /// <returns>True only if there are exactly 2 left neighbors and both are normal.</returns>
        /*private bool IsLeftPlatformNormal(GameObject platform)
        {
            var leftNeighbors = SpawnerController.Instance.GetNeighbors(platform, 2).left;
            if (leftNeighbors.Count() != 2)
            {
                return false;
            }
            return leftNeighbors.All(p => p.GetComponent<PlatformManager>().data.state is PlatformNormalStateSO);
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
        }#1#
        #endregion
    }
}
*/

