using Characters.CombatSystems;
using Characters.InputSystems;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Controllers
{
    /// <summary>
    /// Main controller for enemy characters.
    /// Manages enemy-specific systems such as AI input and combat logic.
    /// </summary>
    public class EnemyController : BaseController
    {
        #region Inspectors & Variables

        [PropertyTooltip("Handles enemy input logic, typically driven by AI.")]
        [SerializeField] private EnemyInputSystem enemyInputSystem;

        [PropertyTooltip("Handles enemy combat behavior, such as attacking.")]
        [SerializeField] private EnemyCombatSystem enemyCombatSystem;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Initializes the enemy combat system and passes a reference to this controller.
        /// </summary>
        protected override void Awake()
        {
            enemyCombatSystem.Initialize(this);
            base.Awake();
        }

        /// <summary>
        /// Subscribes the enemy attack input to its combat system when enabled.
        /// </summary>
        private void OnEnable()
        {
            if (!enemyInputSystem) return;
            enemyInputSystem.OnAttackInputPerform += enemyCombatSystem.Attack;
        }

        /// <summary>
        /// Unsubscribes input events when the enemy is disabled or destroyed.
        /// </summary>
        private void OnDisable()
        {
            if (!enemyInputSystem) return;
            enemyInputSystem.OnAttackInputPerform -= enemyCombatSystem.Attack;
        }

        #endregion
    }
}