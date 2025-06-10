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
    public class ObjectFlyingCoinState : ObjectBaseState
    {
        public override string StateID { get; }
        
        [FoldoutGroup("Idle Setting")] 
        [SerializeField] private float idleTimer;
        [FoldoutGroup("Idle Setting")] 
        [SerializeField] private float distanceIdle;
        
        [SerializeField]
        [ReadOnly] private Vector2 originalPosition;
        
        public override void OnSpawned(ObjectManager manager)
        {
            manager.Loop?.Kill();
            manager.transform.DOKill();
            _ = InitializeAsync(manager);
        }

        public override void OnDespawned(ObjectManager manager)
        {
            manager.Loop?.Kill();
            manager.Loop = null;
        }

        public override void UpdateState(ObjectManager manager) { }

        public override void OnTriggerEnterObject(Collider2D other, ObjectManager manager)
        {
            if (other.TryGetComponent(out ScoreSystem score)) score.AddScore();
            manager.feedback.PlayFeedbacks();
            manager.gameObject.SetActive(false);
        }
        
        private async UniTask InitializeAsync(ObjectManager manager)
        {
            await UniTask.Yield();
            originalPosition = manager.transform.position;
            
            await UniTask.NextFrame();
            manager.Loop = DOTween.Sequence();
            manager.Loop.SetDelay(0.33f);
            manager.Loop.Append(manager.transform.DOMoveY(distanceIdle, idleTimer)
                .SetRelative()
                .SetEase(Ease.OutSine));
            manager.Loop.AppendInterval(0.33f);
            manager.Loop.SetLoops(-1, LoopType.Yoyo);
        }
        
    }
}
