using System;
using System.Threading;
using Characters.HealthSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ObjectItem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ObjectItem
{
    public class ObstacleFlyingState : ObjectBaseState
    {
        public override string StateID { get; }
        
        [FoldoutGroup("Idle Setting")] 
        [SerializeField] private float idleTimer;
        [FoldoutGroup("Idle Setting")] 
        [SerializeField] private float distanceIdle;
    
        [FoldoutGroup("Attack Setting")]
        [SerializeField] private float damage;
        [FoldoutGroup("Attack Setting")]
        [SerializeField] private Vector2 lastAttackBoxSize;
        [FoldoutGroup("Attack Setting")]
        [SerializeField] private Vector2 lastAttackBoxOffset;
        [FoldoutGroup("Attack Setting")]
        [SerializeField] private LayerMask attackLayer;
        
        [SerializeField]
        [ReadOnly] private Vector2 originalPosition;
        
        public override void OnSpawned(ObjectManager manager)
        {
            _ = InitializeAsync(manager);
        }

        public override void OnDespawned(ObjectManager manager) { }

        public override void UpdateState(ObjectManager manager) { }

        public override void OnTriggerEnterObject(Collider2D other, ObjectManager manager)
        {
            Vector2 center = (Vector2)manager.transform.position + lastAttackBoxOffset;
            manager.Attack(center, lastAttackBoxSize, attackLayer, damage);
        }
        
        private async UniTask InitializeAsync(ObjectManager manager)
        {
            await UniTask.Yield();
            originalPosition = manager.transform.position;

            manager.loopTokenSource = new CancellationTokenSource();
            await UniTask.WaitUntil(() => originalPosition == (Vector2)manager.transform.position);
            LoopBehavior(manager ,manager.loopTokenSource.Token).Forget();
        }

        /// <summary>
        /// Loop behavier
        /// </summary>
        /// <param name="token"></param>
        private async UniTaskVoid LoopBehavior(ObjectManager manager,CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && manager.gameObject.activeInHierarchy)
                {
                    await manager.transform.DOMoveY(originalPosition.y + distanceIdle, idleTimer).ToUniTask();
                    await UniTask.Delay(TimeSpan.FromSeconds(idleTimer/2));
                
                    await manager.transform.DOMoveY(originalPosition.y, idleTimer).ToUniTask();
                    await UniTask.Delay(TimeSpan.FromSeconds(idleTimer/2));
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
