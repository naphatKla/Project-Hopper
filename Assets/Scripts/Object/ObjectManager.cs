using System;
using System.Threading;
using Characters.HealthSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using ObjectItem;

namespace ObjectItem
{
    public class ObjectManager : MonoBehaviour
    {
        public ObjectBaseState currentState;
        [ReadOnly] public CancellationTokenSource loopTokenSource;
        
        public Rigidbody2D RigidbodyPlatform { get; private set; }
        public BoxCollider2D ColliderPlatform { get; private set; }
        public SpriteRenderer RendererPlatform { get; private set; }

        private void Awake()
        {
            RigidbodyPlatform = GetComponent<Rigidbody2D>();
            ColliderPlatform = GetComponent<BoxCollider2D>();
            RendererPlatform = GetComponent<SpriteRenderer>();
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
            
            RigidbodyPlatform.gravityScale = 0;
            RendererPlatform.color = Color.white;
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
                await RendererPlatform.DOColor(colorB, singleDuration).ToUniTask();
                await RendererPlatform.DOColor(colorA, singleDuration).ToUniTask();
            }
        }
        
        /// <summary>
        /// Play Animator with desired speed
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="speed"></param>
        public async UniTask PlayAndWait(Animator animator, string name, float desiredDuration)
        {
            if (!animator || !animator.gameObject.activeInHierarchy) return;

            var clip = GetClipByName(animator, name);
            if (clip == null) return;

            float speed = clip.length / desiredDuration;

            animator.Play(name, 0, 0f);
            animator.speed = speed;

            try
            {
                await UniTask.WaitUntil(() =>
                {
                    if (!Application.isPlaying || animator == null || !animator.gameObject.activeInHierarchy) return true;
                    var state = animator.GetCurrentAnimatorStateInfo(0);
                    return state.IsName(name) && state.normalizedTime >= 1f;
                }, cancellationToken: animator.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException) { }
        }
        
        /// <summary>
        /// Get animation clip by name
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="clipName"></param>
        /// <returns></returns>
        private AnimationClip GetClipByName(Animator animator, string clipName)
        {
            if (animator.runtimeAnimatorController == null) { return null; }

            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == clipName)
                    return clip;
            }
            return null;
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
