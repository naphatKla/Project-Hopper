using System;
using Characters.HealthSystems;
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

        [Title("Dependencies")]
        
        [PropertyTooltip("The HealthSystem component responsible for managing HP, damage, and death behavior of this character.")]
        [SerializeField] private HealthSystem healthSystem;

        /// <summary>
        /// Gets the character's HealthSystem, which handles HP, damage, healing, and death.
        /// </summary>
        public HealthSystem HealthSystem => healthSystem;

        /// <summary>
        /// Reset health on awake.
        /// </summary>
        protected virtual void Awake()
        {
            HealthSystem?.ResetHealth();
        }

        #endregion
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