using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Platform.State;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Platform.Cloud
{
    public class PlatformCloud : PlatformBaseState
    {
        [FoldoutGroup("Idle Settings")]
        [SerializeField] private float waitTime = 2.5f;
        
        public override void OnSpawned(PlatformManager manager)
        {
            manager.ResetPlatform();
            
            manager.loopTokenSource = new CancellationTokenSource();
            LoopBehavior(manager, manager.loopTokenSource.Token).Forget();
        }

        public override void OnDespawned(PlatformManager manager)
        {
            manager.ResetPlatform();
            manager.loopTokenSource?.Cancel();
            manager.loopTokenSource?.Dispose();
            manager.loopTokenSource = null;
        }
        
        public override void UpdateState(PlatformManager manager) { }

        public override void OnStepped(PlatformManager manager, GameObject player) { }
        
        private async UniTask LoopBehavior(PlatformManager manager, CancellationToken token)
        {
            while (!token.IsCancellationRequested && manager != null)
            {
                try
                {
                    if (token.IsCancellationRequested || manager == null) return; 
                    //1. Wait
                    await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);
                    
                    //2. Blink
                    if (manager == null ) return;
                    manager.transform.DOShakePosition(0.66f, new Vector3(0.1f, 0f, 0f));
                    await manager.BlinkColor(Color.white, Color.yellow, 0.66f, 3);
                    
                    //3. Dissapear
                    await DisappearPhase(manager, token);
                }
                catch (Exception a)
                {
                    Console.WriteLine(a);
                    throw;
                }
            }
        }
        
        private async UniTask DisappearPhase(PlatformManager manager, CancellationToken token)
        {
            var renderer = manager.RendererPlatform;
            var collider = manager.ColliderPlatform;

            await DOTween.Sequence()
                .Append(renderer.DOFade(0f, 0.1f))
                .AppendCallback(() => collider.enabled = false)
                .AppendInterval(0.33f)
                .AppendCallback(() => collider.enabled = true)
                .Append(renderer.DOFade(1f, 0.1f))
                .ToUniTask(cancellationToken: token);
        }
    }
}
