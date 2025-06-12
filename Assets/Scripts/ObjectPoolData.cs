using System;
using Interface;
using Pool;
using UnityEngine;
using UnityEngine.Events;

public class ObjectPoolData : MonoBehaviour, ISpawnable , IPoolable
{
    [SerializeField] private string spawnId;

    public string SpawnId { get => spawnId; set => spawnId = value; }
    public static event Action<ObjectPoolData> OnAnySpawned;
    public static event Action<ObjectPoolData> OnAnyDespawned;

    public UnityEvent onSpawnedEvent;
    public UnityEvent onDespawnedEvent;

    public void OnSpawned()
    {
        onSpawnedEvent?.Invoke();
        OnAnySpawned?.Invoke(this);
    }

    public void OnDespawned()
    {
        onDespawnedEvent?.Invoke();
        OnAnyDespawned?.Invoke(this);
    }
    
    public void OnDisable()
    {
        onDespawnedEvent?.Invoke();
        OnAnyDespawned?.Invoke(this);
    }
}
