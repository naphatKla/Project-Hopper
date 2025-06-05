using System;
using Sirenix.OdinInspector;

namespace Characters.InputSystems
{
    /// <summary>
    /// Handles enemy AI input or simulated input for testing.
    /// Provides an event to trigger enemy attack actions.
    /// </summary>
    public class EnemyInputSystem : BaseInputSystem
    {
        /// <summary>
        /// Called when the enemy performs an attack input.
        /// Typically triggered by AI logic or test buttons.
        /// </summary>
        public override Action OnAttackInputPerform { get; set; }
        
        /// <summary>
        /// Invokes the attack input event for the enemy.
        /// Can be called manually via the editor for testing.
        /// </summary>
        [Button("Perform Attack")]
        protected override void PerformAttack()
        {
            OnAttackInputPerform?.Invoke();
        }
    }
}