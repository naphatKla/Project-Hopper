using System;
using Interface;
using Pool;
using UnityEngine;
using UnityEngine.Events;

public class ObjectPoolData : MonoBehaviour , ISpawnable , IPoolable
{
    [SerializeField] private string spawnId;

    public string SpawnId { get => spawnId; set => spawnId = value; }
    
    [SerializeField] public UnityEvent<GameObject> OnObjectSpawnedEvent;
    [SerializeField] public UnityEvent<GameObject> OnObjectDespawnedEvent;
    
    private bool wasActive = true;

    private void OnEnable()
    {
        OnObjectSpawnedEvent?.Invoke(gameObject);
        wasActive = true;
    }

    private void OnDisable()
    {
        if (wasActive)
        {
            OnObjectDespawnedEvent?.Invoke(gameObject);
            wasActive = false;
        }
    }

    public void OnSpawned()
    {
        OnObjectSpawnedEvent?.Invoke(gameObject);
        wasActive = true;
    }

    public void OnDespawned()
    {
        if (wasActive)
        {
            OnObjectDespawnedEvent?.Invoke(gameObject);
            wasActive = false;
        }
    }
}
