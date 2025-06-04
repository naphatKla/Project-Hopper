using DG.Tweening;
using UnityEngine;

namespace Platform
{
    [CreateAssetMenu(fileName = "FallingState", menuName = "PlatformStates/Falling")]
    public class PlatformFallingSO : PlatformBaseStateSO
    {
        public override void EnterState(PlatformManager manager)
        {
            manager.GetComponent<Rigidbody2D>().gravityScale = 0;
        }

        public override void UpdateState(PlatformManager manager) { }

        public override async void OnStepped(PlatformManager manager, GameObject player)
        {
            manager.BlinkColor(Color.white, Color.red, 0.66f, 3);
        }
    }
}
