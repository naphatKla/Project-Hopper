using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using Platform;
using Pool;
using UnityEngine;

public class SpawnerController : MMSingleton<SpawnerController>
{
    public List<GameObject> _allPlatform = new();
    
    private void OnDisable()
    {
        ClearAll();
    }
    
    public GameObject Spawn(string id, Vector3 position)
    { 
        return PoolingManager.Instance.Spawn(id, position);
    }
    
    public GameObject Spawn(string id, GameObject prefab, Vector3 position)
    {
        return PoolingManager.Instance.Spawn(id, prefab, position);
    }

    public void Despawn(string id, GameObject obj)
    {
        PoolingManager.Instance.Despawn(id, obj);
    }

    public void Prewarm(string id, GameObject prefab, int amount,Transform parent)
    {
        PoolingManager.Instance.Prewarm(id, prefab, amount, parent);
    }

    public void ClearAll()
    {
        PoolingManager.Instance.ClearAll();
    }
    
    public T GetRandomOption<T>(List<T> options, bool useWeight = true) where T : ISpawnOption
    {
        var passed = options.Where(o => o.TryPassChance()).ToList();
        if (passed.Count == 0) return default;

        if (!useWeight)
        {
            return passed[Random.Range(0, passed.Count)];
        }
        int totalWeight = passed.Sum(o => o.Weight);
        int rand = Random.Range(0, totalWeight);
        int current = 0;

        foreach (var opt in passed)
        {
            current += opt.Weight;
            if (rand < current)
                return opt;
        }

        return passed[0];
    }

    
    public List<GameObject> GetPreviousMultiple(List<GameObject> sourceList, GameObject obj, int count)
    {
        int index = sourceList.IndexOf(obj);
        if (index < 0) return new List<GameObject>();

        int start = Mathf.Max(0, index - count);
        return sourceList.GetRange(start, index - start);
    }
} 