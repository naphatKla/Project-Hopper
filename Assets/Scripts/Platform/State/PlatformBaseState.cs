using MoreMountains.Feedbacks;
using UnityEngine;

namespace Platform.State
{
    public abstract class PlatformBaseState : MonoBehaviour
    {
        public abstract void OnSpawned(PlatformManager manager);
        public abstract void OnDespawned(PlatformManager manager);
        public abstract void UpdateState(PlatformManager manager);
        public abstract void OnStepped(PlatformManager manager, GameObject player);
    }
}