using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Controllers;
using MoreMountains.Tools;
using Object;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Platform
{
    public class PlatformSpawnerManager : MonoBehaviour
    {
        #region Option
        [Serializable]
        public class PlatformSpawnOption : ISpawnOption
        {
            [FoldoutGroup("$id")]
            public string id;
            [FoldoutGroup("$id")]
            public GameObject prefab;
            [FoldoutGroup("$id")]
            public int prewarmCount = 5;
            [FoldoutGroup("$id")]
            public int weight = 1;
            [FoldoutGroup("$id")]
            [Range(0, 100)] public int chance = 100;
            
            public int Weight => weight;
            public int Chance => chance;
            public bool TryPassChance() => Random.Range(0, 100) < chance;
        }
        #endregion

        #region Inspector & Value
        [FoldoutGroup("List")] [ListDrawerSettings(Expanded = false, ShowPaging = true)]
        [SerializeField] private List<PlatformSpawnOption> platformPrefabs;
        
        [FoldoutGroup("Control")] [Tooltip("How many first platforms must be Normal")] [SerializeField]
        private int initialNormalPlatformCount = 7;
        
        [FoldoutGroup("Control")] [SerializeField] [Tooltip("Start position")]
        private Vector3 spawnStartPosition = Vector3.zero;
        
        [FoldoutGroup("Control")] [SerializeField] [Tooltip("Distance between platform")]
        private float distancePlatform;
        
        [FoldoutGroup("Control")] [SerializeField] [Tooltip("Platform parent")]
        private Transform parent;
        
        [FoldoutGroup("Height Propertie")] [Tooltip("Current platform height value")] [SerializeField]
        private int currentStep = 4;

        [FoldoutGroup("Height Propertie")] [Tooltip("Next platform height value")] [SerializeField]
        private int nextStep = 4;

        [FoldoutGroup("Height Propertie")] [Tooltip("Platform height value")] [SerializeField]
        private int targetStep = 4;

        [FoldoutGroup("Height Propertie")]
        [Tooltip("How much to retain the platform before it random height")]
        [SerializeField]
        private int retainStep = 7;
        
        private readonly Queue<GameObject> _activePlatforms = new();
        private Dictionary<GameObject, string> _spawnedPlatformToIdMap = new();
        
        public List<PlatformSpawnOption> PlatformPrefabs => platformPrefabs;
        public static event Action<GameObject> OnPlatformSpawned;
        public static event Action<GameObject> OnPlatformDespawned;
        
        private Vector3 _lastSpawnPosition;
        private const int _minStep = 1;
        private const int _maxStep = 8;
        private const float _stepHeight = 0.2f;
        private const int _maxActivePlatformCount = 15;
        #endregion
        
        #region Unity Methods
        private void Awake()
        {
            SpawnerController.Instance._allPlatform.Clear();
            _activePlatforms.Clear();
            _spawnedPlatformToIdMap.Clear();
            foreach (var config in platformPrefabs)
            {
                SpawnerController.Instance.Prewarm(config.id, config.prefab, config.prewarmCount, parent);
            }
        }
        
        private void Start()
        {
            _lastSpawnPosition = spawnStartPosition;
            SpawnStart7Platform();
            
            if (!PlayerController.Instance?.GridMovementSystem) return;
            PlayerController.Instance.GridMovementSystem.OnJumpUp += SpawnNextPlatform;
        }
        #endregion
        
        #region Methods
        public void SpawnStart7Platform()
        {
            string normalID = "PlatformNormal";
            for (var i = 0; i < initialNormalPlatformCount; i++)
            {
                var newStep = CalculateWeight();
                _lastSpawnPosition.x += distancePlatform;
                _lastSpawnPosition.y = newStep * _stepHeight;
                _lastSpawnPosition = SnapToGrid(_lastSpawnPosition, 0.1f);
                var platform = SpawnerController.Instance.Spawn(normalID, _lastSpawnPosition);
                
                _activePlatforms.Enqueue(platform);
                _spawnedPlatformToIdMap[platform] = normalID;
            }
        }
        
        public void SpawnNextPlatform()
        {
            currentStep = nextStep;
            var newStep = CalculateWeight();

            _lastSpawnPosition.x += distancePlatform;
            _lastSpawnPosition.y = newStep * _stepHeight;
            _lastSpawnPosition = SnapToGrid(_lastSpawnPosition, 0.05f);
            
            var option = SpawnerController.Instance.GetRandomOption(platformPrefabs, true);
            var platform = SpawnerController.Instance.Spawn(option.id, _lastSpawnPosition);
            
            _activePlatforms.Enqueue(platform);
            _spawnedPlatformToIdMap[platform] = option.id;
            SpawnerController.Instance._allPlatform.Add(platform);
            OnPlatformSpawned?.Invoke(platform);
            CheckDespawn();
        }
        
        public void CheckDespawn()
        {
            while (_activePlatforms.Count > _maxActivePlatformCount)
            {
                var oldPlatform = _activePlatforms.Dequeue();
                if (_spawnedPlatformToIdMap.TryGetValue(oldPlatform, out var id))
                {
                    SpawnerController.Instance._allPlatform.Remove(oldPlatform);
                    SpawnerController.Instance.Despawn(id, oldPlatform);
                    OnPlatformDespawned?.Invoke(oldPlatform);
                    _spawnedPlatformToIdMap.Remove(oldPlatform);
                }
            }
        }
    
        private int CalculateWeight()
        {
            if (currentStep == targetStep)
            {
                if (retainStep <= 0)
                {
                    int direction = Random.Range(0, 2) == 0 ? -1 : 1;
                    int magnitude = Random.Range(1, 4);
                    int offset = direction * magnitude;
                    targetStep = Mathf.Clamp(currentStep + offset, _minStep, _maxStep);
                    retainStep = Random.Range(1, 5);
                }

                nextStep = currentStep;
                retainStep--;
            }
            else
            {
                nextStep = currentStep + (currentStep < targetStep ? 1 : -1);
            }

            return nextStep;
        }
        
        private Vector3 SnapToGrid(Vector3 position, float gridSize = 0.5f)
        {
            position.x = Mathf.Round(position.x / gridSize) * gridSize;
            position.y = Mathf.Round(position.y / gridSize) * gridSize;
            position.z = 0f;
            return position;
        }
        #endregion
        
        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere((Vector2)spawnStartPosition + new Vector2(+0.5f,0), 0.1f);
        }

        #endregion
    }
}

