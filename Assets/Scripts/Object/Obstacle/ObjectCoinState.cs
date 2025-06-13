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
        
        public override void OnSpawned(ObjectManager manager)
        {
            manager.Loop?.Kill();
            manager.transform.DOKill();
            manager.RigidbodyPlatform.gravityScale = 1;
        }

        public override void OnDespawned(ObjectManager manager)
        {
            manager.Loop?.Kill();
            manager.Loop = null;
        }

        public override void UpdateState(ObjectManager manager) { }

        public override void OnTriggerEnterObject(Collider2D other, ObjectManager manager)
        {
            if (!other.CompareTag("Player")) return;
            if (other.TryGetComponent(out ScoreSystem score)) score.AddScore();
            manager.feedback.PlayFeedbacks();
            manager.gameObject.SetActive(false);
        }
        
    }
}
