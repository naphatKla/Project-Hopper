using System;
using System.Collections;
using System.Collections.Generic;
using Characters.Controllers;
using Characters.MovementSystems;
using Cysharp.Threading.Tasks;
using PoolingSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Platform
{
    public class PlatformSpawner : MonoBehaviour
    {
        #region Inspector & Value

        [FoldoutGroup("Platform Context")] [Tooltip("Platform prefab template")]
        public GameObject platformPrefab;

        [FoldoutGroup("Platform Context")] [Tooltip("Platform data list")]
        public List<PlatformDataSO> platformDatas;

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

        [FoldoutGroup("Control")] [Tooltip("How many first platforms must be Normal")] [SerializeField]
        private int initialNormalPlatformCount = 7;

        [FoldoutGroup("Control")] [SerializeField] [Tooltip("Prewarm the platform")]
        private int prewarmCount = 10;

        [FoldoutGroup("Control")] [SerializeField] [Tooltip("Start position")]
        private Vector3 spawnStartPosition = Vector3.zero;
        
        [FoldoutGroup("Control")] [SerializeField] [Tooltip("Distance between platform")]
        private float distancePlatform;

        [FoldoutGroup("Control")] [SerializeField] [Tooltip("Platform parent")]
        private Transform parent;

        private readonly Queue<GameObject> activePlatforms = new();
        private Vector3 lastSpawnPosition;
        private int spawnedPlatformCount;

        private const int minStep = 1;
        private const int maxStep = 8;
        private const float stepHeight = 0.2f;
        private const int maxActivePlatformCount = 10;

        #endregion

        #region Unity Methods
        private void Awake()
        {
            PoolingManager.Instance.PreWarm(platformPrefab, prewarmCount, parent);
            
            lastSpawnPosition = spawnStartPosition;

            // Start spawn 7 platform
            var normalSO = platformDatas.Find(data => data.platformType == PlatformType.Normal);
            for (var i = 0; i < initialNormalPlatformCount; i++)
            {
                var newStep = CalculateNextStep();
                lastSpawnPosition.x += distancePlatform;
                lastSpawnPosition.y = newStep * stepHeight;
                lastSpawnPosition = SnapToGrid(lastSpawnPosition, 0.1f);
                SpawnPlatform(lastSpawnPosition, normalSO);
            }
        }

        private void OnEnable()
        {
            if (!PlayerController.Instance?.MovementSystem) return;
            PlayerController.Instance.MovementSystem.OnJumpUp += SpawnNextPlatform;
        }

        private void OnDisable()
        {
            if (!PlayerController.Instance?.MovementSystem) return;
            PlayerController.Instance.MovementSystem.OnJumpUp -= SpawnNextPlatform;
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Spawn next platform
        /// </summary>
        public void SpawnNextPlatform()
        {
            currentStep = nextStep;
            var newStep = CalculateNextStep();
          
            lastSpawnPosition.x += distancePlatform;
            lastSpawnPosition.y = newStep * stepHeight;
            lastSpawnPosition = SnapToGrid(lastSpawnPosition, 0.1f);

            SpawnPlatform(lastSpawnPosition);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Spawn Platform and Initialize
        /// </summary>
        /// <param name="position"></param>
        private void SpawnPlatform(Vector3 position, PlatformDataSO platformData = null)
        {
            if (platformData == null) platformData = GetRandomWeightedPlatform(platformDatas);

            var platformGO = PoolingManager.Instance.Spawn(platformPrefab, position, Quaternion.identity, parent);
            activePlatforms.Enqueue(platformGO);

            // Set Sprite
            var sr = platformGO.GetComponent<SpriteRenderer>();
            sr.sprite = platformData.GetRandomSprite();

            // Set State
            var context = platformGO.GetComponent<PlatformManager>();
            context.SetState(platformData.state);

            spawnedPlatformCount++;
            CheckDespawnPlatform();
        }


        /// <summary>
        /// Despawn old platform if it more than max count
        /// </summary>
        private void CheckDespawnPlatform()
        {
            while (activePlatforms.Count > maxActivePlatformCount)
            {
                var oldPlatform = activePlatforms.Dequeue();
                PoolingManager.Instance.Despawn(oldPlatform);
            }
        }

        /// <summary>
        /// Calculate height platform algorithm
        /// </summary>
        /// <returns></returns>
        private int CalculateNextStep()
        {
            if (currentStep == targetStep)
            {
                if (retainStep <= 0)
                {
                    int direction = Random.Range(0, 2) == 0 ? -1 : 1;
                    int magnitude = Random.Range(1, 4);
                    int offset = direction * magnitude;
                    targetStep = Mathf.Clamp(currentStep + offset, minStep, maxStep);
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

        
        /// <summary>
        /// Calculate weight of platform
        /// </summary>
        /// <param name="platformDataList"></param>
        /// <returns></returns>
        private static PlatformDataSO GetRandomWeightedPlatform(List<PlatformDataSO> platformDataList)
        {
            if (platformDataList == null || platformDataList.Count == 0) return null;
            var totalWeight = 0f;

            foreach (var data in platformDataList) totalWeight += data.weight;
            var randomValue = Random.Range(0f, totalWeight);

            var cumulative = 0f;
            foreach (var data in platformDataList)
            {
                cumulative += data.weight;
                if (randomValue <= cumulative)
                    return data;
            }

            return platformDataList[platformDataList.Count - 1];
        }
        
        /// <summary>
        /// Snap to grid
        /// </summary>
        /// <param name="position"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        private Vector3 SnapToGrid(Vector3 position, float gridSize = 0.5f)
        {
            position.x = Mathf.Round(position.x / gridSize) * gridSize;
            position.y = Mathf.Round(position.y / gridSize) * gridSize;
            return position;
        }

        
        #endregion

        #region Inspector Control

        [Button] [Tooltip("Test spawn next platform")]
        private void TestSpawnPlatform()
        {
            SpawnNextPlatform();
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(spawnStartPosition, 0.1f);
        }

        #endregion
    }
}