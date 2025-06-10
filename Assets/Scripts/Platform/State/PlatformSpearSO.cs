using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PoolingSystem;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace Platform
{
    [CreateAssetMenu(fileName = "SpearState", menuName = "PlatformStates/Spear")]
    public class PlatformSpearSO : PlatformBaseStateSO
    {
        public override string StateID => "Spear";
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
        
        [FoldoutGroup("Sprite")]
        [SerializeField] private Sprite[] sprites;
        
        public override void UpdateState(PlatformManager manager) {}

        public override void OnStepped(PlatformManager manager, GameObject player) { }

        public override void OnSpawned(PlatformManager manager)
        {
            manager.ResetPlatform();
            var spearData = manager.GetObject("Spear");
            spearData.gameObject.SetActive(true);
            SpriteRenderer spriteSpear = manager.GetObject("Spear").spriteRenderer;
            spriteSpear.sprite = sprites[0];
            
            manager.loopTokenSource = new CancellationTokenSource();
            LoopBehavior(manager, manager.loopTokenSource.Token).Forget();
        }

        public override void OnDespawned(PlatformManager manager)
        {
            manager.ResetPlatform();
            manager.GetObject("Spear")?.gameObject.SetActive(false);
            
            manager.loopTokenSource?.Cancel();
            manager.loopTokenSource?.Dispose();
            manager.loopTokenSource = null;
        }
        
        private async UniTaskVoid LoopBehavior(PlatformManager manager, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var spearData = manager.GetObject("Spear");
                    
                    //1. Wait
                    await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);
                    
                    //2. Blink
                    if (manager == null || spearData == null) return;
                    manager.transform.DOShakePosition(0.33f, new Vector3(0.1f, 0f, 0f));
                    await manager.BlinkColor(Color.white, Color.red, flashDuration, blinkCount);
                    manager.PlayFeedbackAsync(manager.feedback, manager.transform.position).Forget();
                    
                    //3. Attack
                    manager.Attack(attackBoxSize, attackBoxOffset, attackLayerMask, 1);

                    //4. Strike
                    SpriteRenderer spriteSpear = manager.GetObject("Spear").spriteRenderer;
                    float frameTime = strikeDuration / sprites.Length;
                    var seq = DOTween.Sequence();
                    foreach (var sprite in sprites)
                    {
                        seq.AppendCallback(() => spriteSpear.sprite = sprite)
                            .AppendInterval(frameTime);
                    }

                    await seq.ToUniTask();
                    
                    Hide(manager, spearData);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                if (manager != null && manager.feedback != null)
                {
                    manager.StopFeedbackAsync(manager.feedback);
                }
            }
        }
        
        private void Hide(PlatformManager manager, ObjectPlatformEffect spearData)
        {
            if (spearData == null || !spearData) return;
            manager.StopFeedbackAsync(manager.feedback);
            SpriteRenderer spriteSpear = manager.GetObject("Spear").spriteRenderer;
            spriteSpear.sprite = sprites[0];
        }

    }
}
