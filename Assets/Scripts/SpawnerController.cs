using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Controllers;
using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using PoolingSystem;
using Sirenix.OdinInspector;
using Spawner.Object;
using Spawner.Platform;
using UnityEngine;

namespace Spawner.Controller
{
    public class SpawnerController : MMSingleton<SpawnerController>
    {
        [BoxGroup("Dependent Context")] 
        [SerializeField] private PlatformSpawner _platformSpawner;
        
        [BoxGroup("Dependent Context")] 
        [SerializeField] private ObjectSpawner _objectSpawner;
        
        private readonly List<GameObject> _activePlatformHistory = new();
        
        private async void OnEnable()
        {
            await UniTask.WaitUntil(() => PlayerController.Instance != null && PlayerController.Instance.GridMovementSystem != null);
            
            //Spawn platform when player jump
            if (!PlayerController.Instance?.GridMovementSystem) return;
            PlayerController.Instance.GridMovementSystem.OnJumpUp += _platformSpawner.SpawnNextPlatform;
            
            _platformSpawner.OnSpawned += HandlePlatformSpawned;
            _platformSpawner.OnDespawned += HandlePlatformDespawned;
        }

        private void OnDisable()
        {
            //Unsubcribe function spawn platform when player jump
            if (!PlayerController.Instance?.GridMovementSystem) return;
            PlayerController.Instance.GridMovementSystem.OnJumpUp -= _platformSpawner.SpawnNextPlatform;
            
            _platformSpawner.OnSpawned -= HandlePlatformSpawned;
            _platformSpawner.OnDespawned -= HandlePlatformDespawned;
        }
        
        private void Awake()
        {
            //Clear all pool & data
            PoolingManager.Instance.ClearPool();
            _platformSpawner.ClearData();
            _objectSpawner.ClearData();
            
            //Create pooling and despawn
            _platformSpawner.PreWarm();
            _objectSpawner.PreWarm();
        }

        private void Start()
        {
            //Start spawn platform
            _platformSpawner.SpawnStartPlatform();
            PlayerController.Instance?.gameObject.SetActive(true);
        }
        
        private void HandlePlatformSpawned(GameObject platform)
        {
            _activePlatformHistory.Add(platform);
            _objectSpawner.TrySpawnObjectOnPlatform(platform);
        }

        private void HandlePlatformDespawned(GameObject despawnedPlatform)
        {
            _activePlatformHistory.Remove(despawnedPlatform);
            _objectSpawner.OnPlatformDespawned(despawnedPlatform);
        }
        
        /// <summary>
        /// Search neighbor platform with Linq
        /// </summary>
        /// <param name="currentPlatform"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public (IEnumerable<GameObject> left, IEnumerable<GameObject> right) GetNeighbors(GameObject currentPlatform, int range)
        {
            int currentIndex = _activePlatformHistory.IndexOf(currentPlatform);
            if (currentIndex == -1) return (Enumerable.Empty<GameObject>(), Enumerable.Empty<GameObject>());
            var left = _activePlatformHistory.Take(currentIndex).TakeLast(range);
            var right = _activePlatformHistory.Skip(currentIndex + 1).Take(range);
            return (left, right);
        }

        #region Inspector Control

        [Button("Spawn Platform",ButtonSizes.Large)] [Tooltip("Spawn next platform")]
        private void SpawnPlatform()
        {
            _platformSpawner.SpawnNextPlatform();
        }

        #endregion
    }
}

