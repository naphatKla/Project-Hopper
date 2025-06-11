using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Platform;
using PoolingSystem;
using Sirenix.OdinInspector;
using Spawner.Controller;
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

        private readonly Queue<GameObject> activePlatforms = new();
        
        private readonly Dictionary<PlatformDataSO, GameObject> feedbackList = new();
        private Vector3 lastSpawnPosition;

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
            var normalSo = platformDatas.Find(data => data.platformSO.state is PlatformNormalStateSO);
            for (var i = 0; i < initialNormalPlatformCount; i++)
            {
                var newStep = CalculateWeight();
                lastSpawnPosition.x += distancePlatform;
                lastSpawnPosition.y = newStep * stepHeight;
                lastSpawnPosition = SnapToGrid(lastSpawnPosition, 0.1f);
                Spawn(lastSpawnPosition, normalSo.platformSO);
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
            lastSpawnPosition = SnapToGrid(lastSpawnPosition, 0.05f);

            Spawn(lastSpawnPosition, GetRandomWeightedPlatform(platformDatas));
        }

        
        ///<summary>
        /// Create feedback for platform
        /// </summary>
        public void PreWarmFeedback()
        {
            foreach (var data in platformDatas)
                if (data.platformSO.feedback != null)
                {
                    var platformFeedback = Instantiate(data.platformSO.feedback, transform.position,
                        Quaternion.identity, feedbackParent);
                    feedbackList.Add(data.platformSO, platformFeedback);
                }
        }

        /// <summary>
        /// Assign feedback to platform
        /// </summary>
        /// <param name="data"></param>
        public GameObject AssignFeedback(PlatformDataSO data)
        {
            return feedbackList.TryGetValue(data, out var feedback) ? feedback : null;
        }
        
        /// <summary>
        /// Spawn Platform and Initialize
        /// </summary>
        /// <param name="position"></param>
        public void Spawn(Vector3 position, PlatformDataSO platformData)
        {
            var platformGO = PoolingManager.Instance.Spawn(platformPrefab, parent, position, Quaternion.identity);
           
            //Set Sprite
            var sr = platformGO.GetComponent<SpriteRenderer>();
            sr.sprite = platformData.GetRandomSprite();

            //Set State
            var context = platformGO.GetComponent<PlatformManager>();
            context.SetState(platformData.state);
            context.SetFeedback(AssignFeedback(platformData));
            context.OnSpawned();
            context.data = platformData;
            
            OnSpawned?.Invoke(platformGO);
            activePlatforms.Enqueue(platformGO);
            CheckDespawn();
            CheckPosition(platformGO, position).Forget();
        }

        private async UniTask CheckPosition(GameObject platform, Vector3 position)
        {
            await UniTask.WaitForSeconds(0.3f);
            if (!IsSamePosition(platform.transform.position, position)) 
                platform.transform.position = position;
        }
        
        bool IsSamePosition(Vector3 a, Vector3 b, float tolerance = 0.01f)
        {
            return Vector3.Distance(a, b) < tolerance;
        }

        /// <summary>
        /// Check old platform if it more than max count
        /// </summary>
        public void CheckDespawn()
        {
            while (activePlatforms.Count > maxActivePlatformCount)
            {
                var oldPlatform = activePlatforms.Dequeue();
                Despawn(oldPlatform);
            }
        }

        
        /// <summary>
        /// Despawn platform
        /// </summary>
        public void Despawn(GameObject obj)
        {
            if (obj == null) return;

            var manager = obj.GetComponent<PlatformManager>();
            if (manager != null)
            {
                manager.OnDespawned();
                OnDespawned?.Invoke(obj);
            }
            PoolingManager.Instance.Despawn(obj);
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
        private PlatformDataSO GetRandomWeightedPlatform(List<PlatformSetting> platformDataList)
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