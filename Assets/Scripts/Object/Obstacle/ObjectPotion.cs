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
    public class ObjectPotion : ObjectBaseState
    {
        public override string StateID { get; }
        
        public override void OnSpawned(ObjectManager manager)
        {
            manager.Loop?.Kill();
            manager.transform.DOKill();
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
            if (other.TryGetComponent(out HealthSystem health)) health.FullHeal();
            manager.feedback.PlayFeedbacks();
            manager.gameObject.SetActive(false);
        }
        
    }
}
