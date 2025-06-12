using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Platform.State;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Platform.TNT
{
    public class PlatformTNT : PlatformBaseState
    {
        [SerializeField] private float explosionOffset;
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
            manager.transform.DOPunchScale(new Vector3(0.3f, 0f, 0f), 0.99f, 5);
            await manager.BlinkColor(Color.white, Color.red, 0.66f, 3);
            //Explosion
            Collider2D[] hits = Physics2D.OverlapCircleAll(manager.transform.position + Vector3.down * explosionOffset, 1f);
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Platform") && hit.gameObject != gameObject)
                {
                    hit.gameObject.SetActive(false);
                }
            }
            manager.PlayFeedbackAsync(manager.transform.position).Forget();
            manager.RendererPlatform.enabled = false;
            await UniTask.Delay(TimeSpan.FromSeconds(0.33f));
            manager.gameObject.SetActive(false);
        }
    }
}
