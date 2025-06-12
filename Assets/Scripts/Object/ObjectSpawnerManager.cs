using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Controllers;
using Characters.HealthSystems;
using MoreMountains.Tools;
using Platform;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Object
{
    public class ObjectSpawnerManager : MonoBehaviour
    {
        [Serializable]
        public class ObjectSpawnOption : ISpawnOption
        {
            [FoldoutGroup("$id")]
            public string id;
            [FoldoutGroup("$id")]
            public GameObject prefab;
            [FoldoutGroup("$id")]
            public int prewarmCount = 10;
            [FoldoutGroup("$id")]
            public int weight = 1;
            [FoldoutGroup("$id")]
            [Range(0, 100)] public int chance = 100;
            [FoldoutGroup("$id")]
            public bool bypassAttemp;
            [FoldoutGroup("$id")]
            public List<string> canSpawnOnType;
            
            [FoldoutGroup("$id")]
            [Tooltip("if this enable mean it will not spawn near falling or danger platform")]
            public bool mustSafeBeforeSpawn;

            public int Weight => weight;
            public int Chance => chance;
            public bool TryPassChance() => Random.Range(0, 100) < chance;
            
        }

        [ListDrawerSettings(Expanded = false, ShowPaging = true)]
        [SerializeField] private List<ObjectSpawnOption> objectPrefabs;
        
        [FoldoutGroup("Control")] [SerializeField] [Tooltip("Object parent")]
        private Transform parent;
        
        [FoldoutGroup("Control")] [Tooltip("Attemp object to wait before spawn again")]
        [SerializeField] private float attempObject;
        

        private readonly Dictionary<GameObject, GameObject> platformObjectMap = new();
        private readonly Dictionary<string, int> activeObjectCount = new();
        
        public event Action<GameObject> OnSpawned;
        public event Action<GameObject> OnDespawned;
        
        private float _currentAttemp = 0;
        private Vector2 _lastPlatformPosition;
        
        private void Awake()
        {
            _currentAttemp = 0;
            platformObjectMap.Clear();
            foreach (var config in objectPrefabs)
            {
                SpawnerController.Instance.Prewarm(config.id, config.prefab, config.prewarmCount, parent);
            }
        }
        
        private void OnEnable()
        {
            PlatformSpawnerManager.OnPlatformSpawned += HandlePlatformSpawned;
            PlatformSpawnerManager.OnPlatformDespawned += HandlePlatformDespawned;
        }

        private void OnDisable()
        {
            PlatformSpawnerManager.OnPlatformSpawned -= HandlePlatformSpawned;
            PlatformSpawnerManager.OnPlatformDespawned -= HandlePlatformDespawned;
        }
        
        private void HandlePlatformSpawned(GameObject platform)
        {
            var platformData = platform.GetComponent<ObjectPoolData>();
            if (platformData == null) return;
            
            var option = SpawnerController.Instance.GetRandomOption(objectPrefabs);
            if (option == null) return;
            
            if (!option.canSpawnOnType.Contains(platformData.SpawnId)) return;
            
            //Count check
            if (!CanSpawn(option)) return;
            //Attemp
            if (CalculateAttemp(option)) return;
            var obj = SpawnerController.Instance.Spawn(option.id, platform.transform.position);
            
            platformObjectMap[platform] = obj;
            if (!activeObjectCount.ContainsKey(option.id)) activeObjectCount[option.id] = 0;
            activeObjectCount[option.id]++;
            OnSpawned?.Invoke(obj);
        }

        private void HandlePlatformDespawned(GameObject platform)
        {
            if (platformObjectMap.TryGetValue(platform, out var option))
            {
                DespawnObject(platform, option);
            }
        }
        
        private void DespawnObject(GameObject platform, GameObject obj)
        {
            var id = obj.GetComponent<ObjectPoolData>()?.SpawnId;
            if (string.IsNullOrEmpty(id)) return;

            SpawnerController.Instance.Despawn(id, obj);
            platformObjectMap.Remove(platform);

            if (activeObjectCount.ContainsKey(id)) activeObjectCount[id] = Mathf.Max(0, activeObjectCount[id] - 1);
            OnDespawned?.Invoke(obj);
        }

        
        private bool CalculateAttemp(ObjectSpawnOption option)
        {
            if (option.bypassAttemp) return false;
            if (_currentAttemp > 0)
            {
                _currentAttemp-- ; return true;
            }
            _currentAttemp = attempObject;
            return false;
        }
        
        /// <summary>
        /// Check is it can spawn
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private bool CanSpawn(ObjectSpawnOption option)
        {
            return activeObjectCount.GetValueOrDefault(option.id, 0) < option.prewarmCount;
        }
    }
}

