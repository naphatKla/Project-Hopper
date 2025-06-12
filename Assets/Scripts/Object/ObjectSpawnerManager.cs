using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Controllers;
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
        

        private readonly Dictionary<GameObject, ObjectSpawnOption> platformObjectMap = new();
        
        public static event Action<GameObject> OnObjectSpawned;
        public static event Action<GameObject> OnObjectDespawned;
        
        private float _currentAttemp = 0;
        private Vector2 _lastPlatformPosition;
        
        private void Awake()
        {
            _currentAttemp = 0;
            platformObjectMap.Clear();
            foreach (var config in objectPrefabs)
            {
                SpawnerController.Instance.Prewarm(config.id, config.prefab, config.prewarmCount, parent);
                config.prefab.GetComponent<ObjectPoolData>().SpawnId = config.id;
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
            
            var option = GetRandomObjectOption();
            if (option == null) return;
            
            if (!option.canSpawnOnType.Contains(platformData.SpawnId)) return;
            
            if (CalculateAttemp(option)) return;
            var obj = SpawnerController.Instance.Spawn(option.id, platform.transform.position);
            platformObjectMap[platform] = option;
            OnObjectSpawned?.Invoke(obj);
        }

        private void HandlePlatformDespawned(GameObject platform)
        {
            if (platformObjectMap.TryGetValue(platform, out var option))
            {
                SpawnerController.Instance.Despawn(option.id, option.prefab);
                platformObjectMap.Remove(platform);
                OnObjectDespawned?.Invoke(option.prefab);
            }
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
        /// Random spawner by passs chance and weight.
        /// </summary>
        /// <param name="options"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private ObjectSpawnOption GetRandomObjectOption()
        {
            var passed = objectPrefabs.Where(o => o.TryPassChance()).ToList();
            if (passed.Count == 0) return null;

            int totalWeight = passed.Sum(o => o.Weight);
            int rand = Random.Range(0, totalWeight);
            int current = 0;

            foreach (var opt in passed)
            {
                current += opt.Weight;
                if (rand < current)
                    return opt;
            }

            return passed[0];
        }
    }
}

