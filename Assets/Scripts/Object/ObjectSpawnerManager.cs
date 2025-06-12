using System;
using System.Collections.Generic;
using Characters.Controllers;
using Platform;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Object
{
    public class ObjectSpawnerManager : MonoBehaviour
    {
        [Serializable]
        public class SpawnOption : ISpawnOption
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
            public int Weight => weight;
            public int Chance => chance;
            public bool TryPassChance() => Random.Range(0, 100) < chance;
        }

        [ListDrawerSettings(Expanded = false, ShowPaging = true)]
        [SerializeField] private List<SpawnOption> objectPrefabs;
        
        [FoldoutGroup("Control")] [SerializeField] [Tooltip("Object parent")]
        private Transform parent;
        
        private readonly Queue<GameObject> _activeObject = new();
        private Dictionary<GameObject, string> _spawnedObjectToIdMap = new();
        
        private void Awake()
        {
            foreach (var config in objectPrefabs)
            {
                SpawnerController.Instance.ClearAll(config.id);
                SpawnerController.Instance.Prewarm(config.id, config.prefab, config.prewarmCount, parent);
                config.prefab.GetComponent<ObjectPoolData>().SpawnId = config.id;
            }
        }
        
        private void OnEnable()
        {
            PlatformSpawnerManager.OnPlatformSpawned += HandlePlatformSpawned;
        }

        private void OnDisable()
        {
            PlatformSpawnerManager.OnPlatformSpawned -= HandlePlatformSpawned;
        }
        
        private void HandlePlatformSpawned(GameObject platform)
        {
            // Spawn object บน platform
            Vector3 objectPos = platform.transform.position + Vector3.up * 1f;
            SpawnerController.SpawnByOptions(objectOptions, objectPos);
        }
    }
}

