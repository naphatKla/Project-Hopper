using UnityEngine;

namespace Platform
{
    [CreateAssetMenu(fileName = "NormalState", menuName = "PlatformStates/Normal")]
    public class PlatformNormalStateSO : PlatformBaseStateSO
    {
        public override void EnterState(PlatformManager manager)
        {
            manager.GetComponent<Rigidbody2D>().gravityScale = 0;
        }

        public override void UpdateState(PlatformManager manager) { }

        public override void OnStepped(PlatformManager manager, GameObject player) { }
    }
}
