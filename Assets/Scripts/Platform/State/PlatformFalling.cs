using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Platform.State;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = System.Random;

namespace Platform.Falling
{
    public class PlatformFalling : PlatformBaseState
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
            manager.transform.DOShakePosition(0.66f, new Vector3(0.1f, 0f, 0f));
            await manager.BlinkColor(Color.white, Color.red, 0.66f, 3);
            manager.RigidbodyPlatform.DORotate(UnityEngine.Random.Range(-180f,180f),2f);
            manager.RigidbodyPlatform.gravityScale = 1;
            manager.ColliderPlatform.enabled = false;
            manager.PlayFeedbackAsync(manager.transform.position + Vector3.down * 0.5f).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            manager.gameObject.SetActive(false);
        }
    }
}
