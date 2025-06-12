using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using Platform;
using Pool;
using UnityEngine;

public class SpawnerController : MMSingleton<SpawnerController>
{
    private List<GameObject> _allSpawnedObjects = new();
    
    public GameObject Spawn(string id, Vector3 position)
    {
        var obj = PoolingManager.Instance.Spawn(id, position);
        Instance.Register(obj);
        return obj;
    }
    
    public GameObject Spawn(string id, GameObject prefab, Vector3 position)
    {
        var obj = PoolingManager.Instance.Spawn(id, prefab, position);
        Instance.Register(obj);
        return obj;
    }

    public void Despawn(string id, GameObject obj)
    {
        PoolingManager.Instance.Despawn(id, obj);
        Instance.Unregister(obj);
    }

    public void Prewarm(string id, GameObject prefab, int amount,Transform parent)
    {
        PoolingManager.Instance.Prewarm(id, prefab, amount, parent);
    }

    public void ClearAll(string id)
    {
        PoolingManager.Instance.ClearAll(id);
    }
    
    /// <summary>
    /// Random spawner by passs chance and weight.
    /// </summary>
    /// <param name="options"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetRandomOption<T>(List<T> options) where T : ISpawnOption
    {
        var passed = options.Where(o => o.TryPassChance()).ToList();
        if (passed.Count == 0) return default;

        int totalWeight = passed.Sum(o => o.Weight);
        int rand = Random.Range(0, totalWeight);
        int current = 0;

        foreach (var opt in passed)
        {
            Debug.Log($"Checking: {opt} | Chance: {opt.Chance} | Pass: {opt.TryPassChance()}");
            current += opt.Weight;
            if (rand < current)
                return opt;
        }

        return passed[0];
    }

    
    #region Tracking & Query
    private void Register(GameObject obj)
    {
        _allSpawnedObjects.Add(obj);
    }

    private void Unregister(GameObject obj)
    {
        _allSpawnedObjects.Remove(obj);
    }

    public GameObject GetPrevious(int offsetFromLast = 1)
    {
        int idx = _allSpawnedObjects.Count - 1 - offsetFromLast;
        return (idx >= 0) ? _allSpawnedObjects[idx] : null;
    }

    public List<GameObject> GetAllSpawned(System.Func<GameObject, bool> filter = null)
    {
        return filter != null ? _allSpawnedObjects.Where(filter).ToList() : new(_allSpawnedObjects);
    }

    public void ClearTracked()
    {
        _allSpawnedObjects.Clear();
    }
    #endregion
} 