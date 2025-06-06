using System;
using System.Collections.Generic;
using Platform;
using PoolingSystem;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spawner.Platform
{
    [Serializable]
    public class PlatformSetting
    {
        [Tooltip("Data of platform to assign")]
        public PlatformDataSO platformSO;
    }
    public class PlatformSpawner : MonoBehaviour, ISpawner
    {
        #region Inspector & Value

        [FoldoutGroup("Platform Context")] [Tooltip("Platform prefab template")]
        public GameObject platformPrefab;

        [FoldoutGroup("Platform Context")] [Tooltip("Platform data list")]
        public List<PlatformSetting> platformDatas;

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
        
        [FoldoutGroup("Feedback")] [SerializeField] [Tooltip("Platform feedback parent")]
        private Transform feedbackParent;

        private readonly LinkedList<GameObject> activePlatforms = new();
        private readonly Dictionary<PlatformDataSO, GameObject> feedbackList = new();
        private Vector3 lastSpawnPosition;
        private int spawnedPlatformCount;

        private const int minStep = 1;
        private const int maxStep = 8;
        private const float stepHeight = 0.2f;
        private const int maxActivePlatformCount = 15;
        
        public event Action<GameObject> OnSpawned;
        public event Action<GameObject> OnDespawned;

        #endregion
        
        #region Public Methods

        /// <summary>
        /// Clear platform data
        /// </summary>
        public void ClearData()
        {
            activePlatforms.Clear();
            feedbackList.Clear();
            spawnedPlatformCount = 0;
            lastSpawnPosition = spawnStartPosition;
        }

        /// <summary>
        /// Pre create platform pooling
        /// </summary>
        public void PreWarm()
        {
            PoolingManager.Instance.PreWarm(platformPrefab, prewarmCount, parent);
            PreWarmFeedback();
        }
        
        /// <summary>
        /// Start spawning 7 platform
        /// </summary>
        public void SpawnStartPlatform()
        {
            var normalSO = platformDatas.Find(data => data.platformSO.state is PlatformNormalStateSO);
            for (var i = 0; i < initialNormalPlatformCount; i++)
            {
                var newStep = CalculateWeight();
                lastSpawnPosition.x += distancePlatform;
                lastSpawnPosition.y = newStep * stepHeight;
                lastSpawnPosition = SnapToGrid(lastSpawnPosition, 0.1f);
                Spawn(lastSpawnPosition, normalSO.platformSO);
            }
        }
        
        /// <summary>
        /// Spawn next platform
        /// </summary>
        public void SpawnNextPlatform()
        {
            currentStep = nextStep;
            var newStep = CalculateWeight();
          
            lastSpawnPosition.x += distancePlatform;
            lastSpawnPosition.y = newStep * stepHeight;
            lastSpawnPosition = SnapToGrid(lastSpawnPosition, 0.1f);

            Spawn(lastSpawnPosition);
        }
        
        /// <summary>
        /// Check old platform if it more than max count
        /// </summary>
        public GameObject CheckPreviousPlatform(GameObject obj)
        {
            var previousPlatform = activePlatforms.Find(obj).Previous.Value;
            if (previousPlatform == null) return null;
            return previousPlatform;
        }
        
        /// <summary>
        /// Create feedback for platform
        /// </summary>
        public void PreWarmFeedback()
        {
            foreach (PlatformSetting data in platformDatas)
            {
                var platformFeedback = PoolingManager.Instance.Spawn(data.platformSO.feedback,transform.position, Quaternion.identity , feedbackParent);
                feedbackList.Add(data.platformSO, platformFeedback);
            }
        }

        /// <summary>
        /// Assign feedback to platform
        /// </summary>
        /// <param name="data"></param>
        public void AssignFeedback(PlatformDataSO data)
        {
            foreach (var pair in feedbackList)
            { 
                if (pair.Key == data) { data.feedback = pair.Value; }
            };
        }
        
        /// <summary>
        /// Spawn Platform and Initialize
        /// </summary>
        /// <param name="position"></param>
        public void Spawn(Vector3 position, object settings = null)
        {
            var platformData = settings as PlatformDataSO ?? GetRandomWeightedPlatform(platformDatas);
            position = SnapToGrid(position, 0.1f);

            var platformGO = PoolingManager.Instance.Spawn(platformPrefab, position, Quaternion.identity, parent);
            activePlatforms.AddLast(platformGO);
            AssignFeedback(platformData);

            //Set Sprite
            var sr = platformGO.GetComponent<SpriteRenderer>();
            sr.sprite = platformData.GetRandomSprite();

            //Set State
            var context = platformGO.GetComponent<PlatformManager>();
            context.SetState(platformData.state);
            context.SetFeedback(platformData.feedback);
            context.OnSpawned();
            context.data = platformData;

            OnSpawned?.Invoke(platformGO);
            spawnedPlatformCount++;
            CheckDespawn();
        }
        
        /// <summary>
        /// Check old platform if it more than max count
        /// </summary>
        public void CheckDespawn()
        {
            while (activePlatforms.Count > maxActivePlatformCount)
            {
                var oldPlatform = activePlatforms.First.Value;
                activePlatforms.RemoveFirst();
                Despawn(oldPlatform); 
            }
        }
        
        /// <summary>
        /// Despawn platform
        /// </summary>
        public void Despawn(GameObject obj)
        {
            if (obj == null) return;
            obj.GetComponent<PlatformManager>().OnDespawned();
            PoolingManager.Instance.Despawn(obj);
            OnDespawned?.Invoke(obj);
        }

        #endregion
        
        #region Private Methods
        /// <summary>
        /// Calculate height platform algorithm
        /// </summary>
        /// <returns></returns>
        private int CalculateWeight()
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
        private static PlatformDataSO GetRandomWeightedPlatform(List<PlatformSetting> platformDataList)
        {
            if (platformDataList == null || platformDataList.Count == 0) return null;
            var totalWeight = 0f;

            foreach (var data in platformDataList) totalWeight += data.platformSO.weight;
            var randomValue = Random.Range(0f, totalWeight);

            var cumulative = 0f;
            foreach (var data in platformDataList)
            {
                cumulative += data.platformSO.weight;
                if (randomValue <= cumulative)
                    return data.platformSO;
            }

            return platformDataList[platformDataList.Count - 1].platformSO;
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

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere((Vector2)spawnStartPosition + new Vector2(+0.5f,0), 0.1f);
        }

        #endregion
    }
}