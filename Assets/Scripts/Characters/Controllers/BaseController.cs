using System;
using Characters.CombatSystems;
using Characters.HealthSystems;
using Characters.InputSystems;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Controllers
{
    /// <summary>
    /// Base class for all character controllers (e.g. Player or Enemy).
    /// Manages shared systems like HealthSystem and acts as a central point of coordination.
    /// </summary>
    public abstract class BaseController : MonoBehaviour
    {
        #region Inspectors & Variables
        
        [PropertyTooltip("The HealthSystem component responsible for managing HP, damage, and death behavior of this character.")]
        [SerializeField] private HealthSystem healthSystem;

        [PropertyTooltip("CombatSystem component of this character.")]
        [SerializeField] private CombatSystem combatSystem;

        [PropertyTooltip("FeedbackSystem component of this character.")]
        [SerializeField] private FeedbackSystem feedbackSystem;

        /// <summary>
        /// Gets the character's HealthSystem, which handles HP, damage, healing, and death.
        /// </summary>
        public HealthSystem HealthSystem => healthSystem;

        /// <summary>
        /// Gets the character's combat system.
        /// </summary>
        public CombatSystem CombatSystem => combatSystem;

        /// <summary>
        /// Gets the character's feedback system.
        /// </summary>
        public FeedbackSystem FeedbackSystem => feedbackSystem;
        
        #endregion
    }

    public abstract class BaseController<T> : BaseController where T : BaseInputSystem
    {
        [Title("Dependencies")] [PropertyOrder(-1)]
        [SerializeField] protected T inputSystem;
        
        /// <summary>
        /// Reset health on awake.
        /// </summary>
        protected virtual void Awake()
        {
            HealthSystem?.Initialize(this);
            CombatSystem?.Initialize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnEnable()
        {
            if (!inputSystem) return;
            inputSystem.OnAttackInputPerform += CombatSystem.Attack;
        }
        
        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnDisable()
        {
            if (!inputSystem) return;
            inputSystem.OnAttackInputPerform += CombatSystem.Attack;
        }
    }

    /// <summary>
    /// Represents the movement state of a character, such as idle or jumping.
    /// </summary>
    [Serializable]
    public enum MovementState
    {
        /// <summary>
        /// Character is idle and not moving.
        /// </summary>
        Idle = 0,

        /// <summary>
        /// Character is currently jumping or in mid-air.
        /// </summary>
        Jumping = 1,
    }

    /// <summary>
    /// Represents the current combat behavior state of the character.
    /// </summary>
    [Serializable]
    public enum CombatState
    {
        /// <summary>
        /// Character is not engaging in any combat action.
        /// </summary>
        None = 0,

        /// <summary>
        /// Character is attacking or performing an offensive action.
        /// </summary>
        Attacking = 1,

        /// <summary>
        /// Character is guarding or defending against incoming attacks.
        /// </summary>
        Guarding = 2,
    }
}