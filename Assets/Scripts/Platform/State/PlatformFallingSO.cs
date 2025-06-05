using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Platform
{
    [CreateAssetMenu(fileName = "FallingState", menuName = "PlatformStates/Falling")]
    public class PlatformFallingSO : PlatformBaseStateSO
    {
        public override string StateID => "Falling";
        public override void UpdateState(PlatformManager manager) { }

        public override async void OnStepped(PlatformManager manager, GameObject player)
        {
            await manager.BlinkColor(Color.white, Color.red, 0.66f, 3);
            manager.GetComponent<Rigidbody2D>().gravityScale = 1;
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            manager.gameObject.SetActive(false);
        }

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
