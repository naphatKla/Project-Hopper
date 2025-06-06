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

        public override void OnStepped(PlatformManager manager, GameObject player)
        {
            RunAsync(manager).Forget();
        }

        public override void OnSpawned(PlatformManager manager)
        {
            manager.ResetPlatform();
        }

        public override void OnDespawned(PlatformManager manager)
        {
            manager.ResetPlatform();
        }

        private async UniTask RunAsync(PlatformManager manager)
        {
            manager.transform.DOShakePosition(0.66f, new Vector3(0.1f, 0f, 0f));
            await manager.BlinkColor(Color.white, Color.red, 0.66f, 3);
            manager.RigidbodyPlatform.gravityScale = 1;
            manager.ColliderPlatform.enabled = false;
            manager.PlayFeedbackAsync(manager.feedback, manager.transform.position + Vector3.down * 0.5f);
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            manager.gameObject.SetActive(false);
        }
    }
}
