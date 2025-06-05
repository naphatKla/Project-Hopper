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
        
        private async void OnEnable()
        {
            await UniTask.WaitUntil(() => PlayerController.Instance != null && PlayerController.Instance.MovementSystem != null);
            
            //Spawn platform when player jump
            if (!PlayerController.Instance?.MovementSystem) return;
            PlayerController.Instance.MovementSystem.OnJumpUp += _platformSpawner.SpawnNextPlatform;
            
            //Subscribe funtion spawn object on platform
            _platformSpawner.OnPlatformSpawned += _objectSpawner.TrySpawnObjectOnPlatform;
        }

        private void OnDisable()
        {
            //Unsubcribe function spawn platform when player jump
            if (!PlayerController.Instance?.MovementSystem) return;
            PlayerController.Instance.MovementSystem.OnJumpUp -= _platformSpawner.SpawnNextPlatform;
            
            //Unsubscribe funtion spawn object on platform
            _platformSpawner.OnPlatformSpawned -= _objectSpawner.TrySpawnObjectOnPlatform;
        }
    }
}

