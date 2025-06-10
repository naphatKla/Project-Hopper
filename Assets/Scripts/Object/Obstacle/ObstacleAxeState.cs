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
    public class ObstacleAxeState : ObjectBaseState
    {
        public override string StateID { get; }
        public AxePerformRotate attackPerform;
        public Collider2D weaponObject;
        
        public override void OnSpawned(ObjectManager manager) { }

        public override void OnDespawned(ObjectManager manager) { }

        public override void UpdateState(ObjectManager manager) { }

        public override void OnTriggerEnterObject(Collider2D other, ObjectManager manager)
        {
            if (!attackPerform._isAttack) return;
            if (weaponObject.bounds.Intersects(other.bounds))
            {
                if (other.CompareTag("Player"))
                {
                    if (other.TryGetComponent(out HealthSystem health)) health.TakeDamage(1).Forget();
                }
            }
        }
    }
}
