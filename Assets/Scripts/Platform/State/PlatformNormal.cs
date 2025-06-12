using Platform.State;
using UnityEngine;

namespace Platform
{
    public class PlatformNormalState : PlatformBaseState
    {
        public override void UpdateState(PlatformManager manager) { }

        public override void OnStepped(PlatformManager manager, GameObject player) { }
        
        public override void OnSpawned(PlatformManager manager)
        {
            manager.ResetPlatform();
        }

        public override void OnDespawned(PlatformManager manager)
        {
            manager.ResetPlatform();
        }
    }
}
