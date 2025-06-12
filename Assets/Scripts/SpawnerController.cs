using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using Platform;
using Pool;
using UnityEngine;

public class SpawnerController : MMSingleton<SpawnerController>
{

    private void OnDisable()
    {
        ClearAll();
    }
    
    public GameObject Spawn(string id, Vector3 position)
    {
        return  PoolingManager.Instance.Spawn(id, position);
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
} 