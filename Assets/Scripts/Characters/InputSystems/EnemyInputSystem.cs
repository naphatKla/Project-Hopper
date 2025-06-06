using System;
using System.Collections;
using Characters.Controllers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.InputSystems
{
    /// <summary>
    /// Handles enemy AI input or simulated input for testing purposes.
    /// This system listens to specific game events (like player landing) to evaluate
    /// attack conditions and invoke attack input events based on proximity.
    /// </summary>
    public class EnemyInputSystem : BaseInputSystem
    {
        [PropertyTooltip("The distance within which the enemy can detect and respond to the player landing.")]
        public float attackRage = 1.1f;

        /// <summary>
        /// Called when the enemy performs an attack input.
        /// This is typically invoked by AI behavior or simulation logic.
        /// </summary>
        public override Action OnAttackInputPerform { get; set; }

        /// <summary>
        /// Coroutine that repeatedly performs attack inputs over time.
        /// Used for continuous attack when conditions are met.
        /// </summary>
        private Coroutine updateCoroutine;

        private void OnEnable()
        {
            updateCoroutine = StartCoroutine(AttackCoroutine());
        }

        private void OnDisable()
        {
            if (updateCoroutine == null) return;
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }

        /// <summary>
        /// Coroutine that repeatedly invokes attack input on a fixed interval.
        /// </summary>
        private IEnumerator AttackCoroutine()
        {
            float tickTime = 0.2f;
            yield return new WaitUntil(() => PlayerController.Instance);
            
            while (gameObject.activeSelf)
            {
                yield return new WaitForSeconds(tickTime);
                float horizontalDistance = Math.Abs(transform.position.x - PlayerController.Instance.transform.position.x);
                if (horizontalDistance > attackRage) continue;
                PerformAttack();
            }
        }

        /// <summary>
        /// Invokes the enemy attack input event manually or through AI logic.
        /// </summary>
        [Button("Perform Attack")]
        protected override void PerformAttack()
        {
            OnAttackInputPerform?.Invoke();
        }
    }
}
