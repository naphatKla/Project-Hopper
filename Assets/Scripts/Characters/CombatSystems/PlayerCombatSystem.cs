using Characters.Controllers;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.CombatSystems
{
    /// <summary>
    /// Combat system specific to the player character.
    /// Extends base combat with a guard mechanic that grants temporary invincibility.
    /// </summary>
    public class PlayerCombatSystem : BaseCombatSystem
    {
        #region Inspectors & Variables

        [PropertyTooltip("Duration (in seconds) the player remains invincible when guarding.")]
        [SerializeField] private float guardDuration = 0.12f;

        [PropertyTooltip("Cooldown time (in seconds) before the player can guard again.")]
        [SerializeField] private float guardCooldown = 0.25f;

        /// <summary>
        /// Tracks whether the guard action is currently on cooldown.
        /// </summary>
        private bool _isGuardCooldown;

        #endregion

        #region Methods

        /// <summary>
        /// Executes a guard action, making the player temporarily invincible.
        /// Guard cannot be used again until the cooldown expires.
        /// </summary>
        public async void Guard()
        {
            if (!isInitialized) return;
            if (_isGuardCooldown) return;
            if (!owner.HealthSystem) return;

            _isGuardCooldown = true;
            owner.HealthSystem.SetInvincible(true);
            await UniTask.WaitForSeconds(guardDuration);
            owner.HealthSystem.SetInvincible(false); // fixed: should turn off after duration

            await UniTask.WaitForSeconds(guardCooldown - guardDuration);
            _isGuardCooldown = false;
        }

        #endregion
    }
}