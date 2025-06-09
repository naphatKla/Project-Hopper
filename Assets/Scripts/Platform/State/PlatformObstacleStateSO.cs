using UnityEngine;

namespace Platform
{
    [CreateAssetMenu(fileName = "ObstacleState", menuName = "PlatformStates/Obstacle")]
    public class PlatformObstacleStateSO : PlatformBaseStateSO
    {
        public override string StateID => "Normal";
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
