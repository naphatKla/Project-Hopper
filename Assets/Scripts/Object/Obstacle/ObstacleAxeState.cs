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
        public GameObject weaponObject;
        
        public override void OnSpawned(ObjectManager manager)
        {
            _ = InitializeAsync(manager);
        }

        public override void OnDespawned(ObjectManager manager) { }

        public override void UpdateState(ObjectManager manager) { }

        public override void OnTriggerEnterObject(Collider2D other, ObjectManager manager) { }
        
        private async UniTask InitializeAsync(ObjectManager manager)
        {
            await UniTask.Yield();
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
                    
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
