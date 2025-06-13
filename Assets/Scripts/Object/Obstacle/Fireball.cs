using System;
using System.Threading;
using Characters.Controllers;
using Characters.HealthSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ObjectItem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ObjectItem
{
    public class Fireball : ObjectBaseState
    {
        public override string StateID { get; }
        
        [FoldoutGroup("Fireball Setting")] 
        [SerializeField] private GameObject warningIcon;
        [FoldoutGroup("Fireball Setting")] 
        [SerializeField] private float warningTimer;
        [FoldoutGroup("Fireball Setting")] 
        [SerializeField] private float prepareTimer;

        private bool _isWarning;
        
        private Vector2 firePosition;
        private Vector2 warningPosition;
        private Camera mainCamera;
        
        public override void OnSpawned(ObjectManager manager)
        {
            manager.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            manager.Loop?.Kill();
            manager.transform.DOKill();
            manager.RendererObject.enabled = false;
            warningIcon.GetComponent<SpriteRenderer>().enabled = true;
            mainCamera = Camera.main;
            _ = FireballAsync(manager);
        }

        public override void OnDespawned(ObjectManager manager)
        {
            // Kill tween loop
            manager.Loop?.Kill();
            manager.transform.DOKill();
            manager.Loop = null;

            // Reset visual
            warningIcon.GetComponent<SpriteRenderer>().enabled = false;
            warningIcon.transform.position = Vector3.zero;

            manager.RendererObject.enabled = false;
            manager.transform.position = Vector3.zero;

            // Reset logic state
            _isWarning = false;
        }


        public override void UpdateState(ObjectManager manager)
        {
            if (_isWarning)
            {
                manager.transform.position = warningIcon.transform.position;
            }
        }

        public override void OnTriggerEnterObject(Collider2D other, ObjectManager manager)
        {
            if (!other.CompareTag("Player")) return;
            if (other.TryGetComponent(out HealthSystem health)) health.TakeDamage(1);
            manager.feedback.PlayFeedbacks();
            manager.gameObject.SetActive(false);
        }
        
        private async UniTask FireballAsync(ObjectManager manager)
        {
            await UniTask.Yield();
            _isWarning = true;
            float lockedY = await DoWarnPhaseAsync();
            _isWarning = false;
            manager.BlinkColor(Color.white, Color.red, 3f, 3).Forget();
            await DoPreparePhaseAsync(lockedY);
            await DoFirePhaseAsync(manager);
        }

        /// <summary>
        /// Warn phase
        /// </summary>
        /// <returns></returns>
        private async UniTask<float> DoWarnPhaseAsync()
        {
            float warntimer = 0f;
            float lockedY = 0f;

            while (warntimer < warningTimer)
            {
                if (PlayerController.Instance != null)
                {
                    float playerY = PlayerController.Instance.transform.position.y;
                    lockedY = playerY + 0.5f;

                    warningIcon.transform.position = GetScreenRightPosition(lockedY);
                }

                warntimer += Time.deltaTime;
                await UniTask.Yield();
            }

            return lockedY;
        }

        /// <summary>
        /// Prepare phase
        /// </summary>
        /// <param name="lockedY"></param>
        private async UniTask DoPreparePhaseAsync(float lockedY)
        {
            float pretimer = 0f;
            while (pretimer < prepareTimer)
            {
                warningIcon.transform.position = GetScreenRightPosition(lockedY);

                pretimer += Time.deltaTime;
                await UniTask.Yield();
            }
        }

        /// <summary>
        /// Fire phase
        /// </summary>
        /// <param name="manager"></param>
        private async UniTask DoFirePhaseAsync(ObjectManager manager)
        {
            warningIcon.GetComponent<SpriteRenderer>().enabled = false;
            manager.RendererObject.enabled = true;
            manager.ColliderObject.enabled = true;

            firePosition = warningIcon.transform.position;
            manager.transform.position = firePosition;

            Vector3 screenLeft = mainCamera.ViewportToWorldPoint(new Vector3(-0.1f, 0.5f, 0));
            screenLeft.y = firePosition.y;
            screenLeft.z = 0;

            float distance = Vector3.Distance(firePosition, screenLeft);
            float duration = distance / 2f;

            await manager.transform.DOMoveX(screenLeft.x, duration)
                .SetEase(Ease.Linear)
                .ToUniTask();

            manager.ColliderObject.enabled = false;
            manager.gameObject.SetActive(false);
        }

        /// <summary>
        /// Check right screen position
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private Vector3 GetScreenRightPosition(float y)
        {
            Vector3 screenRight = mainCamera.ViewportToWorldPoint(new Vector3(0.9f, 0.5f, mainCamera.nearClipPlane));
            screenRight.y = y;
            screenRight.z = 0;
            return screenRight;
        }
    }
}
