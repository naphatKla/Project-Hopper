using System;
using UnityEngine;
using System.Collections.Generic;
using Interface;
using MoreMountains.Tools;
using Unity.VisualScripting;
using IPoolable = Interface.IPoolable;

namespace Pool
{
    public class PoolingManager : MMSingleton<PoolingManager>
    {
        private readonly Dictionary<string, Queue<GameObject>> pool = new();
        private readonly Dictionary<string, GameObject> prefabs = new();
    
        public void Prewarm(string id, GameObject prefab, int amount, Transform parent)
        {
            if (!prefabs.ContainsKey(id)) prefabs[id] = prefab;
            if (!pool.ContainsKey(id)) pool[id] = new Queue<GameObject>();
    
            for (int i = 0; i < amount; i++)
            {
                var obj = Instantiate(prefab, parent);
                obj.SetActive(false);
                pool[id].Enqueue(obj);
            }
        }
    
        public GameObject Spawn(string id, GameObject prefab, Vector3 pos)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null when registering new spawnId.");
    
            if (!prefabs.ContainsKey(id))
                prefabs[id] = prefab;
    
            return Spawn(id, pos);
        }
    
        public GameObject Spawn(string id, Vector3 pos)
        {
            if (!prefabs.ContainsKey(id))
                throw new ArgumentException($"SpawnId '{id}' is not registered. Provide prefab to register it first.");
    
            if (!pool.ContainsKey(id))
                pool[id] = new Queue<GameObject>();
    
            GameObject obj = pool[id].Count > 0
                ? pool[id].Dequeue()
                : Instantiate(prefabs[id]);
    
            obj.transform.position = pos;
            obj.SetActive(true);
    
            obj.GetComponent<ISpawnable>()?.OnSpawned();
            return obj;
        }
    
        public void Despawn(string id, GameObject obj)
        {
            obj.SetActive(false);
            obj.GetComponent<IPoolable>()?.OnDespawned();
            pool[id].Enqueue(obj);
        }
    
        public void ClearAllType(string id)
        {
            if (!pool.ContainsKey(id)) return;
    
            foreach (var obj in pool[id])
                Destroy(obj);
    
            pool[id].Clear();
        }
        
        public void ClearAll()
        {
            pool.Clear();
            prefabs.Clear();
        }
    }
}