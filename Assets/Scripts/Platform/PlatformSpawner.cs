using System;
using System.Collections.Generic;
using PoolingSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlatformSpawner : MonoBehaviour
{
    #region Inspector & Value
    [FoldoutGroup("Platform Context")] [Tooltip("Platform prefab template")]
    public GameObject platformPrefab;
    [FoldoutGroup("Platform Context")] [Tooltip("Platform data list")]
    public List<PlatformDataSO> platformDatas;
    
    [FoldoutGroup("Height Propertie")][Tooltip("Current platform height value")]
    [SerializeField] private int currentStep = 4;
    [FoldoutGroup("Height Propertie")][Tooltip("Next platform height value")]
    [SerializeField] private int nextStep = 4;
    [FoldoutGroup("Height Propertie")][Tooltip("Platform height value")]
    [SerializeField] private int targetStep = 4;
    [FoldoutGroup("Height Propertie")][Tooltip("How much to retain the platform before it random height")]
    [SerializeField] private int retainStep = 7;
    
    [FoldoutGroup("Control")][Tooltip("How many first platforms must be Normal")]
    [SerializeField] private int initialNormalPlatformCount = 7;
    [SerializeField] private int prewarmCount = 10;
    [SerializeField] private Vector3 spawnStartPosition = Vector3.zero;

    private readonly Queue<GameObject> activePlatforms = new Queue<GameObject>();
    private Vector3 lastSpawnPosition;
    private int spawnedPlatformCount = 0;

    private const int minStep = 1;
    private const int maxStep = 8;
    private const float stepHeight = 0.2f;
    private const int maxActivePlatformCount = 10;

    #endregion
    
    private void Start()
    {
        lastSpawnPosition = spawnStartPosition;
        
        // Start spawn 7 platform
        var normalSO = platformDatas.Find(data => data.platformType == PlatformType.Normal);
        for (int i = 0; i < initialNormalPlatformCount; i++)
        {
            SpawnPlatform(lastSpawnPosition, normalSO);
            lastSpawnPosition += Vector3.up * stepHeight * 4;
        }
    }
    
    /// <summary>
    /// Spawn next platform
    /// </summary>
    public void SpawnNextPlatform()
    {
        currentStep = nextStep;
        int newStep = CalculateNextStep();
        lastSpawnPosition += Vector3.up * stepHeight * newStep;

        SpawnPlatform(lastSpawnPosition);
    }

    
    /// <summary>
    /// Spawn Platform and Initialize
    /// </summary>
    /// <param name="position"></param>
    private void SpawnPlatform(Vector3 position, PlatformDataSO platformData = null)
    {
        if (platformData == null) { platformData = GetRandomWeightedPlatform(platformDatas); }

        var platformGO = PoolingManager.Instance.Spawn(platformPrefab, position, Quaternion.identity);
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
            if (retainStep-- <= 0)
            {
                targetStep = Mathf.Clamp(
                    currentStep + Random.Range(-3, 4),
                    minStep,
                    maxStep
                );
                retainStep = Random.Range(1, 6);
            }

            nextStep = currentStep;
        }
        else
        {
            nextStep = currentStep + (targetStep > currentStep ? 1 : -1);
        }

        return nextStep;
    }
    
    /// <summary>
    /// Calculate weight of platform
    /// </summary>
    /// <param name="platformDataList"></param>
    /// <returns></returns>
    public static PlatformDataSO GetRandomWeightedPlatform(List<PlatformDataSO> platformDataList)
    {
        if (platformDataList == null || platformDataList.Count == 0) return null;
        float totalWeight = 0f;

        foreach (var data in platformDataList) totalWeight += data.weight;
        float randomValue = Random.Range(0f, totalWeight);

        float cumulative = 0f;
        foreach (var data in platformDataList)
        {
            cumulative += data.weight;
            if (randomValue <= cumulative)
                return data;
        }
        
        return platformDataList[platformDataList.Count - 1];
    }
}
