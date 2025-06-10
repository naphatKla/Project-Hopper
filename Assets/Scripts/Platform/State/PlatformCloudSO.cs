using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Platform
{
    [CreateAssetMenu(fileName = "CloudState", menuName = "PlatformStates/Cloud")]
    public class PlatformCloudSO : PlatformBaseStateSO
    {
        public override string StateID => "Cloud";
        
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
        }
        public override void UpdateState(PlatformManager manager) { }

        public override void OnStepped(PlatformManager manager, GameObject player) { }
        
        private async UniTaskVoid LoopBehavior(PlatformManager manager, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    //1. Wait
                    await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);
                    
                    //2. Blink
                    if (manager == null ) return;
                    manager.transform.DOShakePosition(0.66f, new Vector3(0.1f, 0f, 0f));
                    await manager.BlinkColor(Color.white, Color.yellow, 0.66f, 3);
                    
                    //3. Dissapear
                    Sequence fadeSequence = DOTween.Sequence();
                    SpriteRenderer renderer = manager.GetComponent<SpriteRenderer>();
                    fadeSequence.Append(renderer.DOFade(0f, 0.1f))
                        .AppendCallback(() => manager.GetComponent<Collider2D>().enabled = false)
                        .AppendInterval(0.33f) 
                        .AppendCallback(() => manager.GetComponent<Collider2D>().enabled = true)
                        .Append(renderer.DOFade(1f, 0.1f));  
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
