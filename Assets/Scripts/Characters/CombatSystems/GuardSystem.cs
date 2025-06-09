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
        
        [Title("Input Buffering")]
        [SerializeField]
        private bool useInputBuffering;
        
        [SerializeField] [ShowIf(nameof(useInputBuffering))]
        private float _inputBufferTime = 0.1f;

        private float _inputBufferTimeCount = 0f;

        private bool _isBuffer;

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

        #region Unity Methods

        private void FixedUpdate()
        {
            if (!_isBuffer) return;
            _inputBufferTimeCount += Time.fixedDeltaTime;
            if (_inputBufferTimeCount <_inputBufferTime) return;
            Guard();
        }
        
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
            if (_isGuardCooldown)
            {
                if (_isBuffer)
                {
                    _isBuffer = false;
                    return;
                }
                _isBuffer = useInputBuffering;
                return;
            }
            if (!_owner.HealthSystem) return;

            _isBuffer = false;
            _inputBufferTimeCount = 0;
            
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
