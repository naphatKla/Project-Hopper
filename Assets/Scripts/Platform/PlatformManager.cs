using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Characters.HealthSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.Feedbacks;
using PoolingSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Platform
{
    public class PlatformManager : MonoBehaviour
    {
        [ReadOnly] public PlatformBaseStateSO currentState;
        [ReadOnly] public CancellationTokenSource loopTokenSource;

        [FoldoutGroup("Object Effect")]
        public List<ObjectPlatformEffect> objectEffects;
        
        private Dictionary<string, ObjectPlatformEffect> _effectDict;
        private Vector2 _lastAttackBoxSize;
        private Vector2 _lastAttackBoxOffset;
        public Rigidbody2D RigidbodyPlatform { get; private set; }
        public BoxCollider2D ColliderPlatform { get; private set; }
        public SpriteRenderer RendererPlatform { get; private set; }
        public ObjectPlatformEffect currentObjectEffect;

        [HideInInspector] public GameObject feedback;
        [HideInInspector] public PlatformDataSO data;

        private void Awake()
        {
            RigidbodyPlatform = GetComponent<Rigidbody2D>();
            ColliderPlatform = GetComponent<BoxCollider2D>();
            RendererPlatform = GetComponent<SpriteRenderer>();

            _effectDict = new Dictionary<string, ObjectPlatformEffect>();
            foreach (var effect in objectEffects)
            {
                if (effect != null && !_effectDict.ContainsKey(effect.name))
                {
                    _effectDict.Add(effect.name, effect);
                    effect.Init();
                }
            }
        }

        public void OnSpawned()
        {
            currentState?.OnSpawned(this);
        }
        
        public void OnDespawned()
        {
            data = null;
            StopFeedbackAsync(feedback);
            currentState?.OnDespawned(this);
        }

        private void Update()
        {
            currentState?.UpdateState(this);
        }

        public void OnStepped(GameObject player)
        {
            currentState?.OnStepped(this, player);
        }
        
        public void SetState(PlatformBaseStateSO newState)
        {
            currentState = newState;
            currentState.OnSpawned(this);
        }
        
        public void SetFeedback(GameObject feedbacks)
        {
            feedback = feedbacks;
        }

        public void ResetPlatform()
        {
            loopTokenSource?.Cancel();
            loopTokenSource?.Dispose();
            loopTokenSource = null;
            _lastAttackBoxOffset = Vector2.zero;
            _lastAttackBoxSize = Vector2.zero;
            
            ColliderPlatform.enabled = true;
            RigidbodyPlatform.gravityScale = 0;
            RendererPlatform.color = Color.white;
        }
        
        
        public ObjectPlatformEffect GetObject(string name)
        {
            if (_effectDict.TryGetValue(name, out var result)) return result;
            return null;
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
            Sequence rotate = DOTween.Sequence();
            rotate.Append(RendererPlatform.DOColor(colorB, singleDuration));
            rotate.Append(RendererPlatform.DOColor(colorA, singleDuration));
            rotate.SetLoops(blinkCount, LoopType.Restart);
            await rotate.ToUniTask();
        }
        
        /// <summary>
        /// Attack by platform
        /// </summary>
        public void Attack(Vector2 attackBoxSize, Vector2 attackBoxOffset, LayerMask attackLayerMask, float damage)
        {
            _lastAttackBoxSize = attackBoxSize;
            _lastAttackBoxOffset = attackBoxOffset;
            
            Vector2 center = (Vector2)transform.position + attackBoxOffset;
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, attackBoxSize, 0f, attackLayerMask);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out HealthSystem health)) health.TakeDamage(damage);
            }
        }
        
        /// <summary>
        /// Play particle and destory when done
        /// </summary>
        /// <param name="particlePrefab"></param>
        /// <param name="position"></param>
        public async UniTask PlayFeedbackAsync(GameObject feedbackItem, Vector3 position)
        {
            if (feedbackItem == null) return;
            feedbackItem.transform.position = position;
            var mmf = feedbackItem.GetComponent<MMFeedbacks>();
            if (mmf == null) return;
            mmf.PlayFeedbacks();
        }
        
        /// <summary>
        /// Play particle and destory when done
        /// </summary>
        /// <param name="particlePrefab"></param>
        /// <param name="position"></param>
        public void StopFeedbackAsync(GameObject feedbackItem)
        {
            if (feedbackItem == null) return;
            var mmf = feedbackItem.GetComponent<MMFeedbacks>();
            if (mmf == null) return;
            mmf.StopFeedbacks();
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector2 center = (Vector2)transform.position + _lastAttackBoxOffset;
            Gizmos.DrawWireCube(center, _lastAttackBoxSize);
        }

    }
}
