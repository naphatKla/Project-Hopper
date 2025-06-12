using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Characters.HealthSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Platform.State;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Platform
{
    public class PlatformManager : MonoBehaviour
    {
        #region Inspector & Variable
        [BoxGroup("Platform Setting")]
        public PlatformBaseState currentState;
        [BoxGroup("Platform Setting")]
        public CancellationTokenSource loopTokenSource;
        [BoxGroup("Platform Feedback")]
        public MMF_Player feedback;
        
        private Vector2 _lastAttackBoxSize;
        private Vector2 _lastAttackBoxOffset;
        #endregion

        #region Properties
        public Rigidbody2D RigidbodyPlatform { get; private set; }
        public BoxCollider2D ColliderPlatform { get; private set; }
        public SpriteRenderer RendererPlatform { get; private set; }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            RigidbodyPlatform = GetComponent<Rigidbody2D>();
            ColliderPlatform = GetComponent<BoxCollider2D>();
            RendererPlatform = GetComponent<SpriteRenderer>();
        }
        
        private void Update()
        {
            currentState?.UpdateState(this);
        }
        #endregion

        #region Public Methods
        public void OnEnable()
        {
            currentState?.OnSpawned(this);
        }

        public void OnDisable()
        {
            ResetPlatform();
            currentState?.OnDespawned(this);
        }

        public void OnStepped(GameObject player)
        {
            currentState?.OnStepped(this, player);
        }
        
        public void SetState(PlatformBaseState newState)
        {
            currentState = newState;
            currentState.OnSpawned(this);
        }

        public void ResetPlatform()
        {
            DOTween.Kill(gameObject);
            loopTokenSource?.Cancel();
            loopTokenSource?.Dispose();
            loopTokenSource = null;
            _lastAttackBoxOffset = Vector2.zero;
            _lastAttackBoxSize = Vector2.zero;
            
            ColliderPlatform.enabled = true;
            RigidbodyPlatform.gravityScale = 0;
            RendererPlatform.color = Color.white;
            RendererPlatform.enabled = true;
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
        public async UniTask PlayFeedbackAsync(Vector3 position)
        {
            if (feedback == null) return;
            feedback.transform.position = position;
            feedback.PlayFeedbacks();
        }
        
        /// <summary>
        /// Play particle and destory when done
        /// </summary>
        /// <param name="particlePrefab"></param>
        /// <param name="position"></param>
        public void StopFeedbackAsync()
        {
            if (feedback == null) return;
            feedback.StopFeedbacks();
        }
        #endregion
    }
}
