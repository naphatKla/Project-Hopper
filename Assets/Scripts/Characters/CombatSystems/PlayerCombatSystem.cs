using Characters.Controllers;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.CombatSystems
{
    public class PlayerCombatSystem : BaseCombatSystem
    {
        #region Inspectors & Variables

        [PropertyTooltip("")]
        [SerializeField] private float guardDuration = 0.12f;
        
        [PropertyTooltip("")]
        [SerializeField] private float guardCooldown = 0.25f;
        
        /// <summary>
        /// 
        /// </summary>
        private bool _isGuardCooldown;

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        public async void Guard()
        {
            if (!isInitialized) return;
            if (_isGuardCooldown) return;
            if (!owner.HealthSystem) return;
            
            _isGuardCooldown = true;
            owner.HealthSystem.SetInvincible(true);
            await UniTask.WaitForSeconds(guardDuration);
            owner.HealthSystem.SetInvincible(true);

            await UniTask.WaitForSeconds(guardCooldown - guardDuration);
            _isGuardCooldown = false;
        }

        #endregion

    }
}
