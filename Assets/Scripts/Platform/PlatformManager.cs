using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Platform
{
    public class PlatformManager : MonoBehaviour
    {
        public PlatformBaseStateSO currentState;

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

        public void SetState(PlatformBaseStateSO newState)
        {
            currentState = newState;
            currentState.EnterState(this);
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
            Debug.Log(renderer);
            float singleDuration = totalDuration / (blinkCount * 2f);

            Sequence seq = DOTween.Sequence();

            for (int i = 0; i < blinkCount; i++)
            {
                seq.Append(renderer.DOColor(colorB, singleDuration));
                seq.Append(renderer.DOColor(colorA, singleDuration));
            }
            await seq.AsyncWaitForCompletion();
        }
    }
}
