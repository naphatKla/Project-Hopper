using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Platform
{
    [CreateAssetMenu(fileName = "BrokenState", menuName = "PlatformStates/Broken")]
    public class PlatformBrokenSO : PlatformBaseStateSO
    {
        public override void UpdateState(PlatformManager manager) { }

        public override async void OnStepped(PlatformManager manager, GameObject player)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.33f));
            manager.gameObject.SetActive(false);
        }

        public override void OnSpawned(PlatformManager manager)
        {
            manager.ResetPlatform();
        }

        public override void OnDespawned(PlatformManager manager)
        {
            manager.ResetPlatform();
        }
    }
}
