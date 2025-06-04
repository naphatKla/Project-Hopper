using UnityEngine;

namespace Platform
{
    [CreateAssetMenu(fileName = "NormalState", menuName = "PlatformStates/Normal")]
    public class PlatformNormalStateSO : PlatformBaseStateSO
    {
        public override void EnterState(PlatformManager manager)
        {
            manager.ResetPlatform();
        }

        public override void UpdateState(PlatformManager manager) { }

        public override void OnStepped(PlatformManager manager, GameObject player) { }

        public override void OnSpawned(PlatformManager manager) { }

        public override void OnDespawned(PlatformManager manager)
        {
            manager.ResetPlatform();
        }
    }
}
