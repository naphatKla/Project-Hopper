using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace Characters.HealthSystems
{
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] private float maxHp = 1;
        [SerializeField] [Unit(Units.Second)] private float iframePerHitDuration;
        
        private float _currentHp;
        private bool _isDead;
        private bool _isIframePerHit;
        private bool _isInvincible;
        
        public Action OnHealthChange { get; set; }
        public Action OnTakeDamage { get; set; }
        public Action OnDead { get; set; }
        
        private void Start()
        {
            ResetHealth();
        }

        private Tween feedbackTest;
        public async void TakeDamage(float damage)
        {
            if (_isDead || _isIframePerHit) return;
            feedbackTest.Kill();

            if (_isInvincible)
            {
                feedbackTest = GetComponent<SpriteRenderer>().DOColor(Color.blue, 0.1f).SetLoops(2, LoopType.Yoyo);
                return;
            }
               
            
            ModifyHealth(-damage);

        
            feedbackTest = GetComponent<SpriteRenderer>().DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo);
            
            if (_currentHp > 0) return;
            _isDead = true;
            OnDead?.Invoke();
            gameObject.SetActive(false);
            
            _isIframePerHit = true;
            await UniTask.WaitForSeconds(iframePerHitDuration);
            _isIframePerHit = false;
        }

        private void ModifyHealth(float amount)
        {
            _currentHp += amount;
            OnHealthChange?.Invoke();
        }

        public void SetInvincible(bool value)
        {
            _isInvincible = value;
            Debug.Log("Guard");
        }
        
        public void ResetHealth()
        {
            ModifyHealth(maxHp);
            _isDead = false;
        }
    }
}
