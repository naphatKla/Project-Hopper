using UnityEngine;
using System;
using UnityEngine.Events;


public class PoolingDespawn : MonoBehaviour
{
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
}