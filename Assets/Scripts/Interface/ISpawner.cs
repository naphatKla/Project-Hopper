using UnityEngine;
using System.Collections.Generic;

public interface ISpawner
{
    void PreWarm();
    void ClearData();
    void Spawn(Vector3 position, object settings = null);
    void Despawn(GameObject obj);
    event System.Action<GameObject> OnSpawned;
    event System.Action<GameObject> OnDespawned;
}