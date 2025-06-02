using UnityEngine;

public abstract class PlatformBaseStateSO : ScriptableObject
{
    public abstract void EnterState(PlatformManager manager);
    public abstract void UpdateState(PlatformManager manager);
    public abstract void OnStepped(PlatformManager manager, GameObject player);
}