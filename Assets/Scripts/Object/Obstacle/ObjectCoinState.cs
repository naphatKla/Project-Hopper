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
    public class ObjectCoinState : ObjectBaseState
    {
        public override string StateID { get; }

        private CancellationTokenSource _despawnCts;

        public override void OnSpawned(ObjectManager manager)
        {
            manager.Loop?.Kill();
            manager.transform.DOKill();
            manager.RigidbodyObject.gravityScale = 1;
            
            _despawnCts = new CancellationTokenSource();
            AutoDespawnAfterDelay(manager, _despawnCts.Token).Forget();
        }

        public override void OnDespawned(ObjectManager manager)
        {
            manager.Loop?.Kill();
            manager.Loop = null;

            _despawnCts?.Cancel();
            _despawnCts?.Dispose();
            _despawnCts = null;
        }

        public override void UpdateState(ObjectManager manager) { }

        public override void OnTriggerEnterObject(Collider2D other, ObjectManager manager)
        {
            if (!other.CompareTag("Player")) return;
            if (other.TryGetComponent(out ScoreSystem score)) score.AddScore();
            manager.feedback.PlayFeedbacks();
            manager.gameObject.SetActive(false);
        }

        private async UniTaskVoid AutoDespawnAfterDelay(ObjectManager manager, CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(10), cancellationToken: token);
                if (!manager.gameObject.activeSelf) return;
                manager.gameObject.SetActive(false);
            }
            catch (OperationCanceledException) { }
        }
    }
}