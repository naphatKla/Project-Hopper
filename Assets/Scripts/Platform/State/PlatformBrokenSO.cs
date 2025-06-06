using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Platform
{
    [CreateAssetMenu(fileName = "BrokenState", menuName = "PlatformStates/Broken")]
    public class PlatformBrokenSO : PlatformBaseStateSO
    {
        public override string StateID => "Broken";
        public override void UpdateState(PlatformManager manager) { }

        public override async void OnStepped(PlatformManager manager, GameObject player)
        {
            manager.transform.DOShakePosition(0.33f, new Vector3(0.1f, 0f, 0f));
            await UniTask.Delay(TimeSpan.FromSeconds(0.33f));
            manager.PlayFeedbackAsync(manager.feedback, manager.transform.position);
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
