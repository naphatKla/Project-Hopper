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
        public override bool ObjectCanSpawn { get; }
        
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
        
        public override void UpdateState(PlatformManager manager) {}

        public override void OnStepped(PlatformManager manager, GameObject player) { }

        public override void OnSpawned(PlatformManager manager)
        {
            manager.ResetPlatform();
            manager.spear.SetActive(true);
            
            if (manager.spear.TryGetComponent(out Animator animator))
            { animator.Play("Spike", 0, 0f); animator.speed = 0; }
            
            manager.attackLooping = true;
            manager.attackLoopTokenSource = new CancellationTokenSource();
            LoopBehavior(manager, manager.attackLoopTokenSource.Token).Forget();
        }

        public override void OnDespawned(PlatformManager manager)
        {
            manager.ResetPlatform();
            manager.spear.SetActive(false);
        }
        
        private async UniTaskVoid LoopBehavior(PlatformManager manager, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    //1. Wait
                    await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);
                    
                    //2. Blink
                    if (manager == null || manager.spear == null) return;
                    await manager.BlinkColor(Color.white, Color.red, flashDuration, blinkCount);
                    
                    //3. Attack
                    manager.Attack(attackBoxSize, attackBoxOffset, attackLayerMask, 1);

                    if (manager.spear != null && manager.spear.TryGetComponent(out Animator animator) && animator != null && animator.gameObject.activeInHierarchy) 
                    {
                        await manager.PlayAndWait(animator, "Spike", strikeDuration);
                        
                        //4. Hide
                        Hide(manager);
                    }
                }
            }
            catch (OperationCanceledException) { }
        }
        
        private void Hide(PlatformManager manager)
        {
            if (manager?.spear == null || !manager.spear) return;

            if (manager.spear.TryGetComponent(out Animator animator)) 
                animator.speed = 0;
        }

    }
}
