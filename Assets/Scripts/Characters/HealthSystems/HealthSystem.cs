using System;
using UnityEngine;

namespace Characters.HealthSystems
{
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] private float maxHp = 1;
        private float _currentHp;
        private bool _isDead;
        
        public Action OnHealthChange { get; set; }
        public Action OnTakeDamage { get; set; }
        public Action OnDead { get; set; }

        private void Start()
        {
            ResetHealth();
        }

        public void TakeDamage(float damage)
        {
            ModifyHealth(-damage);
            if (_currentHp > 0) return;
            if (_isDead) return;
            _isDead = true;
            OnDead?.Invoke();
        }

        public void ModifyHealth(float amount)
        {
            _currentHp += amount;
            OnHealthChange?.Invoke();
        }

        public void ResetHealth()
        {
            ModifyHealth(maxHp);
            _isDead = false;
        }
    }
}
