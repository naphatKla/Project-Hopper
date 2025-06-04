using UnityEngine;

namespace Platform
{
    public abstract class PlatformBaseStateSO : ScriptableObject
    {
        public abstract void EnterState(PlatformManager manager);
        public abstract void UpdateState(PlatformManager manager);
        public abstract void OnStepped(PlatformManager manager, GameObject player);
        public abstract void OnSpawned(PlatformManager manager);
        public abstract void OnDespawned(PlatformManager manager);
    }
}