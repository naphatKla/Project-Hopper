using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PoolingSystem
{
    public class PoolingManager
    {
        #region Properties

        public static PoolingManager Instance { get; } = new();
        private readonly Dictionary<string, Queue<GameObject>> _pools = new();

        #endregion

        #region Methods

        /// <summary>
        ///     Spawns a GameObject from the pool or instantiates a new one if none is available.
        /// </summary>
        /// <param name="prefab">The prefab to spawn.</param>
        /// <param name="position">The position to spawn the object at.</param>
        /// <param name="rotation">The rotation to apply to the spawned object.</param>
        /// <param name="parent">The parent Transform for the spawned object (optional).</param>
        /// <returns>The spawned GameObject, or null if the prefab is null.</returns>
        public GameObject Spawn(GameObject prefab, Transform parent = null , Vector3? position = null, Quaternion? rotation = null)
        {
            if (prefab == null) return null;

            var poolKey = prefab.name;
            var pool = _pools.GetOrAdd(poolKey, () => new Queue<GameObject>());
            GameObject obj = null;

            if (pool.Count > 0)
                obj = pool.Dequeue();
            else
                obj = Object.Instantiate(prefab);

            obj.transform.SetParent(parent, false);
            obj.transform.position = position ?? Vector3.zero;
            obj.transform.rotation = rotation ?? Quaternion.identity;
            obj.SetActive(true);
            return obj;
        }




        /// <summary>
        ///     Add object key to pool
        /// </summary>
        /// <param name="obj"></param>
        public void AddToPool(GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);
            var poolKey = obj.name.EndsWith("(Clone)") ? obj.name[..^7] : obj.name;
            _pools.GetOrAdd(poolKey, () => new Queue<GameObject>()).Enqueue(obj);
        }

        /// <summary>
        ///     Returns a GameObject to the pool for reuse.
        /// </summary>
        /// <param name="obj">The GameObject to despawn.</param>
        public void Despawn(GameObject obj)
        {
            if (obj == null) return;
            if (obj.activeInHierarchy)
                obj.SetActive(false);
            var poolKey = obj.name.EndsWith("(Clone)") ? obj.name[..^7] : obj.name;
            _pools.GetOrAdd(poolKey, () => new Queue<GameObject>()).Enqueue(obj);
        }

        
        private string ExtractPrefabName(string name)
        {
            return name.EndsWith("(Clone)") ? name[..^7] : name;
        }


        /// <summary>
        ///     Clears all objects in the pool.
        /// </summary>
        public void ClearPool()
        {
            foreach (var pool in _pools.Values)
                while (pool.Count > 0)
                {
                    var obj = pool.Dequeue();
                    if (obj != null)
                    {
                        if (Application.isPlaying)
                            Object.Destroy(obj);
                        else
                            Object.DestroyImmediate(obj);
                    }
                }

            _pools.Clear();
        }

        /// <summary>
        ///     Gets the number of objects in a specific pool.
        /// </summary>
        /// <param name="poolKey">The key of the pool to query.</param>
        /// <returns>The number of objects in the pool, or 0 if the pool doesn't exist.</returns>
        public int GetPoolCount(string poolKey)
        {
            return _pools.TryGetValue(poolKey, out var pool) ? pool.Count : 0;
        }

        /// <summary>
        ///     Pre-warms a pool by instantiating and pooling a specified number of objects.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="count">The number of objects to create.</param>
        /// <param name="parent">The parent Transform for the objects (optional).</param>
        public void PreWarm(GameObject prefab, float count, Transform parent = null)
        {
            if (prefab == null || count <= 0) return;

            var poolKey = prefab.name;
            var pool = _pools.GetOrAdd(poolKey, () => new Queue<GameObject>());

            while (count-- > 0)
            {
                var obj = Object.Instantiate(prefab);
                obj.SetActive(false);
                obj.transform.SetParent(parent);
                pool.Enqueue(obj);
            }
        }

        /// <summary>
        ///     Destroys all objects in a specific pool and removes it.
        /// </summary>
        /// <param name="poolKey">The key of the pool to destroy.</param>
        private void DestroyPool(string poolKey)
        {
            if (!_pools.TryGetValue(poolKey, out var pool)) return;

            while (pool.Count > 0)
                if (pool.Dequeue() is { } obj)
                    if (Application.isPlaying)
                        Object.Destroy(obj);
                    else
                        Object.DestroyImmediate(obj);

            _pools.Remove(poolKey);
        }

        #endregion
    }

    public static class DictionaryExtensions
    {
        /// <summary>
        ///     Gets a value from the dictionary or adds a new one if it doesn't exist.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary value.</typeparam>
        /// <param name="dict">The dictionary to query.</param>
        /// <param name="key">The key to look up.</param>
        /// <param name="createValue">The function to create a new value if the key is not found.</param>
        /// <returns>The existing or newly created value.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key,
            Func<TValue> createValue)
        {
            if (!dict.TryGetValue(key, out var value)) dict[key] = value = createValue();
            return value;
        }
    }
}