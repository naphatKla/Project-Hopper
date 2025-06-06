using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Characters.InputSystems
{
    /// <summary>
    /// Handles player input via UI buttons for attack, movement, and guarding.
    /// Dispatches input events to subscribed systems such as combat and movement.
    /// </summary>
    public class PlayerInputSystem : BaseInputSystem
    {
        #region Inspectors & Variables

        [PropertyTooltip("Button to trigger the player's attack action.")]
        [SerializeField] private Button attackButton;

        [PropertyTooltip("Button to trigger the player's movement action.")]
        [SerializeField] private Button moveButton;

        [PropertyTooltip("Button to trigger the player's guard action.")]
        [SerializeField] private Button guardButton;
        
        /// <summary>
        /// Called when the player performs an attack input.
        /// </summary>
        public override Action OnAttackInputPerform { get; set; }

        /// <summary>
        /// Called when the player performs a move input.
        /// </summary>
        public Action OnMoveInputPerform { get; set; }

        /// <summary>
        /// Called when the player performs a guard input.
        /// </summary>
        public Action OnGuardInputPerform { get; set; }

        #endregion

        #region Unity Methods

        /// <summary>
        /// Subscribes button click events when the component is enabled.
        /// </summary>
        private void OnEnable()
        {
            moveButton?.onClick.AddListener(PerformMove);
            attackButton?.onClick.AddListener(PerformAttack);
            guardButton?.onClick.AddListener(PerformGuard);
        }

        /// <summary>
        /// Unsubscribes button click events when the component is disabled.
        /// </summary>
        private void OnDisable()
        {
            moveButton?.onClick.RemoveListener(PerformMove);
            attackButton?.onClick.RemoveListener(PerformAttack);
            guardButton?.onClick.RemoveListener(PerformGuard);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invokes the attack input event. Called when the attack button is pressed.
        /// </summary>
        [BoxGroup("Debug Controls")]
        [Button(ButtonSizes.Large)]
        protected override void PerformAttack()
        {
            OnAttackInputPerform?.Invoke();
        }

        /// <summary>
        /// Invokes the move input event. Called when the move button is pressed.
        /// </summary>
        [BoxGroup("Debug Controls")]
        [Button(ButtonSizes.Large)]
        protected void PerformMove()
        {
            OnMoveInputPerform?.Invoke();
        }

        /// <summary>
        /// Invokes the guard input event. Called when the guard button is pressed.
        /// </summary>
        [BoxGroup("Debug Controls")]
        [Button(ButtonSizes.Large)]
        protected void PerformGuard()
        {
            OnGuardInputPerform?.Invoke();
        }

        #endregion
    }
}
