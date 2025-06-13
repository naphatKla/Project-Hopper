using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Platform.State;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace Platform.Spear
{
    public class PlatformSpear : PlatformBaseState
    {
        [FoldoutGroup("Idle Settings")]
        [SerializeField] private float waitTime = 1f;
        [FoldoutGroup("Idle Settings")]
        [SerializeField] private int blinkCount = 3;
        [FoldoutGroup("Idle Settings")]
        [SerializeField] private float flashDuration = 0.33f;
        [FoldoutGroup("Idle Settings")]
        [SerializeField] private float strikeDuration = 0.33f;
        
        [FoldoutGroup("Attack Settings")]
        [SerializeField] private Vector2 attackBoxSize = new Vector2(1f, 1f);

        [FoldoutGroup("Attack Settings")]
        [SerializeField] private Vector2 attackBoxOffset = Vector2.up;

        [FoldoutGroup("Attack Settings")]
        [SerializeField] private LayerMask attackLayerMask;
        
        [FoldoutGroup("Spear Setting")]
        [SerializeField] private Sprite[] sprites;
        [FoldoutGroup("Spear Setting")]
        [SerializeField] private GameObject spear;
        
        private SpriteRenderer _spriteSpear;
        
        public override void UpdateState(PlatformManager manager) {}

        public override void OnStepped(PlatformManager manager, GameObject player) { }

        public override void OnSpawned(PlatformManager manager)
        {
            manager.ResetPlatform();
            spear.gameObject.SetActive(true);
            _spriteSpear = spear.GetComponent<SpriteRenderer>();
            _spriteSpear.sprite = sprites[0];
            
            manager.loopTokenSource = new CancellationTokenSource();
            LoopBehavior(manager, manager.loopTokenSource.Token).Forget();
        }

        public override void OnDespawned(PlatformManager manager)
        {
            manager.ResetPlatform();
            spear.gameObject.SetActive(false);
            
            manager.loopTokenSource?.Cancel();
            manager.loopTokenSource?.Dispose();
            manager.loopTokenSource = null;
        }
        
        private async UniTask LoopBehavior(PlatformManager manager, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (token.IsCancellationRequested || manager == null) return; 
                    //1. Wait
                    await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);
                    
                    //2. Blink
                    if (manager == null || spear == null) return;
                    manager.transform.DOShakePosition(0.33f, new Vector3(0.1f, 0f, 0f));
                    await manager.BlinkColor(Color.white, Color.red, flashDuration, blinkCount);
                    manager.PlayFeedbackAsync(manager.transform.position).Forget();
                    
                    //3. Attack
                    manager.Attack(attackBoxSize, attackBoxOffset, attackLayerMask, 1);

                    //4. Strike
                    await StrikePhase(manager, token);
                    Hide(manager);
                }
                catch (Exception a)
                {
                    Console.WriteLine(a);
                    throw;
                }
            }
        }
        
        private void Hide(PlatformManager manager)
        {
            if (spear == null || !spear) return;
            manager.StopFeedbackAsync();
            _spriteSpear.sprite = sprites[0];
        }

        private async UniTask StrikePhase(PlatformManager manager, CancellationToken token)
        {
            if (spear == null) return;
            float frameTime = strikeDuration / sprites.Length;
            var seq = DOTween.Sequence();
            foreach (var sprite in sprites)
            {
                seq.AppendCallback(() => _spriteSpear.sprite = sprite)
                    .AppendInterval(frameTime);
            }

            await seq.ToUniTask(cancellationToken: token);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector2 center = (Vector2)transform.position + attackBoxOffset;
            Gizmos.DrawWireCube(center, attackBoxSize);
        }
    }
}
