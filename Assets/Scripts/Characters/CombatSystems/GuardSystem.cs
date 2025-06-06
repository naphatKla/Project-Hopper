using Characters.Controllers;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.CombatSystems
{
    public class GuardSystem : MonoBehaviour
    {
        #region Inspectors & Variables

        [PropertyTooltip("Duration (in seconds) the player remains invincible when guarding.")]
        [SerializeField] private float guardDuration = 0.12f;

        [PropertyTooltip("Cooldown time (in seconds) before the player can guard again.")]
        [SerializeField] private float guardCooldown = 0.25f;

        /// <summary>
        /// Tracks whether this initialized or not.
        /// </summary>
        private bool _isInitialized;
        
        /// <summary>
        /// Tracks whether the guard action is currently on cooldown.
        /// </summary>
        private bool _isGuardCooldown;

        /// <summary>
        /// owner of this character.
        /// </summary>
        private BaseController _owner;

        #endregion

        #region Methods

        public void Initialize(BaseController controller)
        {
            _owner = controller;
            _isInitialized = true;
        }
        
        /// <summary>
        /// Executes a guard action, making the player temporarily invincible.
        /// Guard cannot be used again until the cooldown expires.
        /// </summary>
        public async void Guard()
        {
            if (!_isInitialized) return;
            if (_isGuardCooldown) return;
            if (!_owner.HealthSystem) return;

            _owner.FeedbackSystem.PlayFeedback(FeedbackKey.Guard);
            _isGuardCooldown = true;
            _owner.HealthSystem.SetInvincible(true);
            await UniTask.WaitForSeconds(guardDuration);
            _owner.HealthSystem.SetInvincible(false); 

            await UniTask.WaitForSeconds(guardCooldown - guardDuration);
            _isGuardCooldown = false;
        }
        
        #endregion
    }
}
