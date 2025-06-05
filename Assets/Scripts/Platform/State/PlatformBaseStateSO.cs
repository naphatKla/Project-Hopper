using MoreMountains.Feedbacks;
using UnityEngine;

namespace Platform
{
    public abstract class PlatformBaseStateSO : ScriptableObject
    {
        [SerializeField, Tooltip("Object can spawn above platform")]
        protected bool objectCanSpawn = false;

        public virtual bool ObjectCanSpawn => objectCanSpawn;
        
        public abstract void OnSpawned(PlatformManager manager);
        public abstract void OnDespawned(PlatformManager manager);
        public abstract void UpdateState(PlatformManager manager);
        public abstract void OnStepped(PlatformManager manager, GameObject player);
    }
}