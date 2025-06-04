using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Platform
{
    public class PlatformManager : MonoBehaviour
    {
        public PlatformBaseStateSO currentState;
        public CancellationTokenSource blinkCts;

        [FoldoutGroup("Object Effect")]
        public GameObject spear;

        private void Start()
        {
            currentState?.EnterState(this);
        }

        private void Update()
        {
            currentState?.UpdateState(this);
        }

        public void OnStepped(GameObject player)
        {
            currentState?.OnStepped(this, player);
        }

        public void OnSpawned()
        {
            currentState?.OnSpawned(this);
        }
        
        public void OnDespawned()
        {
            currentState?.OnDespawned(this);
        }

        public void SetState(PlatformBaseStateSO newState)
        {
            currentState = newState;
            currentState.EnterState(this);
        }

        public void ResetPlatform()
        {
            blinkCts?.Cancel();
            blinkCts?.Dispose();
            blinkCts = null;
            
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
        public async UniTask BlinkColor(Color colorA, Color colorB, float totalDuration, int blinkCount, CancellationToken? token = null)
        {
            SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
            float singleDuration = totalDuration / (blinkCount * 2f);

            for (int i = 0; i < blinkCount; i++)
            {
                token?.ThrowIfCancellationRequested();
                await renderer.DOColor(colorB, singleDuration).ToUniTask(cancellationToken: token ?? CancellationToken.None);

                token?.ThrowIfCancellationRequested();
                await renderer.DOColor(colorA, singleDuration).ToUniTask(cancellationToken: token ?? CancellationToken.None);
            }
        }
        
        /// <summary>
        /// Play Animator with desired speed
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="speed"></param>
        public async UniTask PlayAndWait(Animator animator, string name, float desiredDuration)
        {
            var clip = GetClipByName(animator, name);
            if (clip == null)
            { return; }

            float speed = clip.length / desiredDuration;

            animator.Play(name, 0, 0f);
            animator.speed = speed;

            await UniTask.WaitUntil(() =>
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                return state.IsName(name) && state.normalizedTime >= 1f;
            });
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


    }
}
