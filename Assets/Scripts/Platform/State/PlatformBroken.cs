using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Platform.State;
using UnityEngine;

namespace Platform.Broken
{
    public class PlatformBroken : PlatformBaseState
    {
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
            manager.transform.DOShakePosition(0.33f, new Vector3(0.1f, 0f, 0f));
            manager.RendererPlatform.enabled = false;
            await manager.PlayFeedbackAsync(manager.transform.position);
            await UniTask.Delay(TimeSpan.FromSeconds(0.33f));
            manager.gameObject.SetActive(false);
        }
    }
}
