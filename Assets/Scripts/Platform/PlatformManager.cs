using System;
using System.Threading;
using Characters.HealthSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Platform
{
    public class PlatformManager : MonoBehaviour
    {
        [ReadOnly] public PlatformBaseStateSO currentState;
        [ReadOnly] public CancellationTokenSource attackLoopTokenSource;

        [FoldoutGroup("Object Effect")]
        [SerializeField] public GameObject spear;

        public PlatformDataSO data;
        
        private Vector2 lastAttackBoxSize;
        private Vector2 lastAttackBoxOffset;
        [HideInInspector] public bool attackLooping;
        
        public void OnSpawned()
        {
            currentState?.OnSpawned(this);
        }
        
        public void OnDespawned()
        {
            data = null;
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

        public void ResetPlatform()
        {
            attackLooping = false;
            attackLoopTokenSource?.Cancel();
            attackLoopTokenSource?.Dispose();
            attackLoopTokenSource = null;
            lastAttackBoxOffset = Vector2.zero;
            lastAttackBoxSize = Vector2.zero;
            
            GetComponent<Rigidbody2D>().gravityScale = 0;
            GetComponent<SpriteRenderer>().color = Color.white;
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
            SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
            float singleDuration = totalDuration / (blinkCount * 2f);

            for (int i = 0; i < blinkCount; i++)
            {
                await renderer.DOColor(colorB, singleDuration).ToUniTask();
                await renderer.DOColor(colorA, singleDuration).ToUniTask();
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
            lastAttackBoxSize = attackBoxSize;
            lastAttackBoxOffset = attackBoxOffset;
            
            Vector2 center = (Vector2)transform.position + attackBoxOffset;
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, attackBoxSize, 0f, attackLayerMask);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out HealthSystem health)) health.TakeDamage(damage);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector2 center = (Vector2)transform.position + lastAttackBoxOffset;
            Gizmos.DrawWireCube(center, lastAttackBoxSize);
        }

    }
}
