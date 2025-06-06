using System.Threading;
using Characters.Controllers;
using Characters.HealthSystems;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.CombatSystems
{
    /// <summary>
    /// base class for combat systems that handle attacking,
    /// damage dealing, and cooldown logic for both player and enemy.
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        #region Inspectors & Variables

        [Title("Configs")]  
        
        [PropertyTooltip("True if character is facing right, false if facing left. Use in pivot point calculation")]
        [SerializeField] private bool facingRight = true;
        
        [PropertyTooltip("The size of the attack area (width x height) used to detect targets.")]
        [SerializeField] private Vector2 attackArea = new Vector2(2, 1);

        [PropertyTooltip("Offset from the character’s position to the bottom-left of the attack box.")]
        [SerializeField] private Vector2 offset = new Vector2(-0.5f, 0);

        [PropertyTooltip("Layer mask used to detect valid targets for attacks.")]
        [SerializeField] private LayerMask targetLayer;

        [Title("Stats")]
        
        [PropertyTooltip("Amount of damage dealt to each target hit by the attack.")]
        [SerializeField] private float damage = 1f;

        [PropertyTooltip("Delay before the attack is executed after pressing the input.")]
        [SerializeField] private float attackDelay = 0.1f;

        [PropertyTooltip("Cooldown duration after each attack before another can be performed.")]
        [SerializeField] private float attackCooldown = 0.1f;

        /// <summary>
        /// Cached position where the attack starts (based on offset).
        /// </summary>
        private Vector2 _attackStartPos;

        /// <summary>
        /// Indicates whether the combat system is currently in cooldown.
        /// </summary>
        private bool _isAttackCooldown;

        /// <summary>
        /// Reference to the controller that owns this combat system.
        /// </summary>
        protected BaseController owner;

        /// <summary>
        /// Whether this system has been initialized with a controller.
        /// </summary>
        protected bool isInitialized;

        /// <summary>
        /// Used to cancel ongoing attack delays if the object is disabled.
        /// </summary>
        private CancellationTokenSource _ct;

        #endregion

        #region Unity Methods
        
        /// <summary>
        /// Draws a red wireframe box in the Scene view to visualize the attack range.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            float direction = facingRight ? 1f : -1f;
            Vector2 start = (Vector2)transform.position + new Vector2(offset.x * direction, offset.y);
            Vector2 boxCenter = start + new Vector2(attackArea.x * 0.5f * direction, attackArea.y * 0.5f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boxCenter, attackArea);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the combat system with the owning controller.
        /// Must be called before using this system.
        /// </summary>
        /// <param name="controller">The controller that owns this combat system.</param>
        public void Initialize(BaseController controller)
        {
            owner = controller;
            isInitialized = true;
            controller.HealthSystem.OnDead += _ct.Cancel;
        }

        /// <summary>
        /// Executes an attack after a delay. Deals damage to all valid targets in the attack area.
        /// Handles attack delay, cooldown, and cancellation.
        /// </summary>
        public async void Attack()
        {
            if (!isInitialized || _isAttackCooldown) return;
            
            _isAttackCooldown = true;
            owner.FeedbackSystem.PlayFeedback(FeedbackKey.Attack);
            _ct = new CancellationTokenSource();
            await UniTask.WaitForSeconds(attackDelay, cancellationToken: _ct.Token);
            if (_ct.IsCancellationRequested)
            {
                _isAttackCooldown = false;
                return;
            }
            
            float direction = facingRight ? 1f : -1f;
            _attackStartPos = (Vector2)transform.position + new Vector2(offset.x * direction, offset.y);
            Vector2 boxCenter = _attackStartPos + new Vector2(attackArea.x * 0.5f * direction, attackArea.y * 0.5f);

            Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, attackArea, 0f, targetLayer);
            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent(out HealthSystem targetHealth)) continue;
                targetHealth.TakeDamage(damage);
            }

            await UniTask.WaitForSeconds(attackCooldown);
            _isAttackCooldown = false;
        }

        #endregion
    }
}
