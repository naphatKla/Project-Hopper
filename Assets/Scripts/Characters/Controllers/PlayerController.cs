using Characters.CombatSystems;
using Characters.InputSystems;
using Characters.MovementSystems;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Controllers
{
    /// <summary>
    /// Main controller for the player character.
    /// Manages player-specific systems such as input, movement, and combat.
    /// Also implements singleton access for global player references.
    /// </summary>
    public class PlayerController : BaseController
    {
        #region Inspectors & Variables

        [PropertyTooltip("Handles player input from keyboard, gamepad, etc.")]
        [SerializeField] private PlayerInputSystem playerInputSystem;

        [PropertyTooltip("Handles grid-based movement logic and interaction with the world.")]
        [SerializeField] private GridMovementSystem gridMovementSystem;

        [PropertyTooltip("Handles player-specific combat behavior, such as attacking and guarding.")]
        [SerializeField] private PlayerCombatSystem playerCombatSystem;

        /// <summary>
        /// Gets the grid-based movement system used by the player.
        /// </summary>
        public GridMovementSystem GridMovementSystem => gridMovementSystem;

        /// <summary>
        /// Gets the player's combat system which includes actions like attack and guard.
        /// </summary>
        public PlayerCombatSystem PlayerCombatSystem => playerCombatSystem;

        /// <summary>
        /// Singleton reference to the player controller instance in the scene.
        /// </summary>
        public static PlayerController Instance { get; private set; }

        #endregion

        #region Unity Methods

        /// <summary>
        /// Initializes player systems and assigns singleton instance.
        /// Destroys duplicate instances to ensure only one player exists.
        /// </summary>
        protected override void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            gridMovementSystem?.Initialize(this);
            playerCombatSystem?.Initialize(this);
            base.Awake();
        }

        /// <summary>
        /// Subscribes player input events to their corresponding system actions.
        /// </summary>
        private void OnEnable()
        {
            if (!playerInputSystem) return;

            playerInputSystem.OnAttackInputPerform += PlayerCombatSystem.Attack;
            playerInputSystem.OnMoveInputPerform += GridMovementSystem.TryMoveAction;
            playerInputSystem.OnGuardInputPerform += playerCombatSystem.Guard;
        }

        /// <summary>
        /// Unsubscribes input events when the object is disabled.
        /// </summary>
        private void OnDisable()
        {
            if (!playerInputSystem) return;

            playerInputSystem.OnAttackInputPerform -= PlayerCombatSystem.Attack;
            playerInputSystem.OnMoveInputPerform -= GridMovementSystem.TryMoveAction;
            playerInputSystem.OnGuardInputPerform -= playerCombatSystem.Guard;
        }

        #endregion
    }
}
