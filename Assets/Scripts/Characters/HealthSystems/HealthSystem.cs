using System;
using Characters.Controllers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.HealthSystems
{
    /// <summary>
    /// Handles the health-related logic of a character, including taking damage,
    /// applying invincibility, and managing death state.
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        #region Inspectors & Variables

        [PropertyTooltip("Maximum health points this character can have.")]
        [SerializeField] private float maxHp = 1;

        [PropertyTooltip("Duration of invincibility frames after taking damage.")]
        [SerializeField, Unit(Units.Second)] 
        private float iframePerHitDuration;

        [SerializeField, Unit(Units.Second)] 
        private float disappearDurationAfterDead = 0.1f;

        /// <summary>
        /// Current health points.
        /// </summary>
        private float _currentHp;

        /// <summary>
        /// Whether the character is currently dead.
        /// </summary>
        private bool _isDead;

        /// <summary>
        /// Whether the character is currently under invincibility frames from a hit.
        /// </summary>
        private bool _isIframePerHit;

        /// <summary>
        /// Whether the character is completely invincible (e.g., during guarding or special states).
        /// </summary>
        private bool _isInvincible;

        /// <summary>
        /// owner of this health system.
        /// </summary>
        private BaseController _owner;

        /// <summary>
        /// Invoked whenever health changes.
        /// </summary>
        public Action OnHealthChange { get; set; }

        /// <summary>
        /// Invoked when damage is successfully taken.
        /// </summary>
        [Title("Events")]
        public UnityEvent OnTakeDamage;

        /// <summary>
        /// Invoked when the character's HP reaches zero.
        /// </summary>
        public UnityEvent OnDead; 
        
        /// <summary>
        /// Whether the character is currently dead.
        /// </summary>
        public bool IsDead => _isDead;

        public float MaxHp => maxHp;

        public float CurrentHp => _currentHp;

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the owner on start and reset current health.
        /// </summary>
        /// <param name="owner"></param>
        public void Initialize(BaseController owner)
        {
            _owner = owner;
            ResetHealth();
        }
        
        private Tween feedbackTest;

        /// <summary>
        /// Applies damage to the character. Includes invincibility checks and feedback effects.
        /// </summary>
        /// <param name="damage">The amount of damage to apply.</param>
        public async UniTask TakeDamage(float damage)
        {
            if (_isDead || _isIframePerHit) return;

            feedbackTest.Kill();

            if (_isInvincible)
            {
                feedbackTest = GetComponent<SpriteRenderer>()
                    .DOColor(Color.blue, 0.1f)
                    .SetLoops(2, LoopType.Yoyo);
                return;
            }

            ModifyHealth(-damage);
            OnTakeDamage?.Invoke();
            _owner?.FeedbackSystem?.PlayFeedback(FeedbackKey.Hurt);


            if (_currentHp > 0)
            {
                _isIframePerHit = true;
                await UniTask.WaitForSeconds(iframePerHitDuration);
                _isIframePerHit = false;
                return;
            }

            await Dead();
        }

        private async UniTask Dead()
        {
            _isDead = true;
            OnDead?.Invoke();
            _owner?.FeedbackSystem?.PlayFeedback(FeedbackKey.Dead);
            _owner.CharacterCollider2D.enabled = false;
            await UniTask.WaitForSeconds(disappearDurationAfterDead);
            gameObject.SetActive(false);
        }
        
        public async UniTask ForceDead()
        {
            ModifyHealth(-_currentHp);
            await Dead();
        }

        /// <summary>
        /// Modifies the current HP and triggers health change events.
        /// </summary>
        /// <param name="amount">The amount to change health by (positive or negative).</param>
        private void ModifyHealth(float amount)
        {
            _currentHp += amount;
            OnHealthChange?.Invoke();
        }

        /// <summary>
        /// Sets whether the character is currently invincible.
        /// </summary>
        /// <param name="value">True to make invincible, false to disable invincibility.</param>
        public void SetInvincible(bool value)
        {
            _isInvincible = value;
            Debug.Log("Guard");
        }

        /// <summary>
        /// Resets the character's health to full and revives if dead.
        /// </summary>
        public void ResetHealth()
        {
            _currentHp = maxHp;
            _isDead = false;
            _owner.CharacterCollider2D.enabled = true;
            OnHealthChange?.Invoke();
        }

        #endregion
    }
}
