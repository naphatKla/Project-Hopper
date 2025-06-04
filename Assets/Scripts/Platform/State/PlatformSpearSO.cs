using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PoolingSystem;
using UnityEngine;

namespace Platform
{
    [CreateAssetMenu(fileName = "SpearState", menuName = "PlatformStates/Spear")]
    public class PlatformSpearSO : PlatformBaseStateSO
    {
        [SerializeField] private float waitTime = 1f;
        [SerializeField] private int blinkCount = 3;
        [SerializeField] private float flashDuration = 0.33f;
        [SerializeField] private float strikeDuration = 0.33f;

        public override void EnterState(PlatformManager manager)
        {
            manager.ResetPlatform();
        }

        public override void UpdateState(PlatformManager manager) { }

        public override void OnStepped(PlatformManager manager, GameObject player) { }

        public override void OnSpawned(PlatformManager manager)
        {
            manager.ResetPlatform();
            manager.blinkCts = new CancellationTokenSource();
            manager.spear.SetActive(true);
            var animator = manager.spear.GetComponent<Animator>();
            animator.speed = 0;
            
            RunLoop(manager, manager.blinkCts.Token).Forget();
        }

        public override void OnDespawned(PlatformManager manager)
        {
            manager.ResetPlatform();
            manager.spear.SetActive(false);
        }
        
        private async UniTaskVoid RunLoop(PlatformManager manager, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: token);
                    await manager.BlinkColor(Color.white, Color.red, flashDuration, blinkCount, token);
                    
                    //Strike here
                    
                    Animator animator = manager.spear.GetComponent<Animator>();
                    await manager.PlayAndWait(animator,"Spike", 0.33f);
                    Hide(manager);
                }
            }
            catch (Exception a)
            {
                Console.WriteLine(a);
                throw;
            }
        }
        
        private void Hide(PlatformManager manager)
        {
            var animator = manager.spear.GetComponent<Animator>();
            animator.Play("Spike", 0, 0f);
            animator.speed = 0;
        }
    }
}
