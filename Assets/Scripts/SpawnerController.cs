using System;
using Characters.Controllers;
using Cysharp.Threading.Tasks;
using PoolingSystem;
using Sirenix.OdinInspector;
using Spawner.Object;
using Spawner.Platform;
using UnityEngine;

namespace Spawner.Controller
{
    public class SpawnerController : MonoBehaviour
    {
        [BoxGroup("Dependent Context")] 
        [SerializeField] private PlatformSpawner _platformSpawner;
        
        [BoxGroup("Dependent Context")] 
        [SerializeField] private ObjectSpawner _objectSpawner;
        
        private async void OnEnable()
        {
            await UniTask.WaitUntil(() => PlayerController.Instance != null && PlayerController.Instance.GridMovementSystem != null);
            
            //Subscribe funtion spawn object on platform
            _platformSpawner.OnSpawned += _objectSpawner.TrySpawnObjectOnPlatform;
            
            //Spawn platform when player jump
            if (!PlayerController.Instance?.GridMovementSystem) return;
            PlayerController.Instance.GridMovementSystem.OnJumpUp += _platformSpawner.SpawnNextPlatform;
        }

        private void OnDisable()
        {
            //Unsubscribe funtion spawn object on platform
            _platformSpawner.OnDespawned -= _objectSpawner.TrySpawnObjectOnPlatform;
            
            //Unsubcribe function spawn platform when player jump
            if (!PlayerController.Instance?.GridMovementSystem) return;
            PlayerController.Instance.GridMovementSystem.OnJumpUp -= _platformSpawner.SpawnNextPlatform;
        }
        
        private async void Awake()
        {
            PlayerController.Instance?.gameObject.SetActive(false);
            
            //Clear all pool & data
            PoolingManager.Instance.ClearPool();
            _platformSpawner.ClearData();
            
            //Create pooling and despawn
            _platformSpawner.PreWarm();
            
            //Start spawn platform
            _platformSpawner.SpawnStartPlatform();
            
            await UniTask.DelayFrame(2);
            PlayerController.Instance?.gameObject.SetActive(true);
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

