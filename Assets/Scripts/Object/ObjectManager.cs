using System;
using System.Threading;
using Characters.HealthSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using ObjectItem;

namespace ObjectItem
{
    public class ObjectManager : MonoBehaviour
    {
        public ObjectBaseState currentState;
        public MMF_Player feedback;
        [ReadOnly] public CancellationTokenSource loopTokenSource;
        
        public Rigidbody2D RigidbodyObject { get; private set; }
        public BoxCollider2D ColliderObject { get; private set; }
        public SpriteRenderer RendererObject { get; private set; }

        public Sequence Loop;

        private void Awake()
        {
            RigidbodyObject = GetComponent<Rigidbody2D>();
            ColliderObject = GetComponent<BoxCollider2D>();
            RendererObject = GetComponent<SpriteRenderer>();
        }

        public void OnEnable()
        {
            currentState?.OnSpawned(this);
        }
        
        public void OnDisable()
        {
            currentState?.OnDespawned(this);
        }

        private void Update()
        {
            currentState?.UpdateState(this);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            currentState?.OnTriggerEnterObject(other, this);
        }
        
        public void SetState(ObjectBaseState newState)
        {
            currentState = newState;
            currentState.OnSpawned(this);
        }
   
        public void ResetObject()
        {
            loopTokenSource?.Cancel();
            loopTokenSource?.Dispose();
            loopTokenSource = null;

            ColliderObject.enabled = true;
            RigidbodyObject.gravityScale = 0;
            RendererObject.color = Color.white;
        }
        
        /// <summary>
        /// Blink the game object
        /// </summary>
        /// <param name="colorA"></param>
        /// <param name="colorB"></param>
        /// <param name="totalDuration"></param>
        /// <param name="blinkCount"></param>
        public async UniTask BlinkColor(Color colorA, Color colorB, float totalDuration, int blinkCount)
        {
            float singleDuration = totalDuration / (blinkCount * 2f);

            for (int i = 0; i < blinkCount; i++)
            {
                await RendererObject.DOColor(colorB, singleDuration).ToUniTask();
                await RendererObject.DOColor(colorA, singleDuration).ToUniTask();
            }
        }
        
        /// <summary>
        /// Attack by platform
        /// </summary>
        public void Attack(Vector2 attackBoxSize, Vector2 attackBoxOffset, LayerMask attackLayerMask, float damage)
        {
            Vector2 center = (Vector2)transform.position + attackBoxOffset;
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, attackBoxSize, 0f, attackLayerMask);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out HealthSystem health)) health.TakeDamage(damage);
            }
        }
    }
}
