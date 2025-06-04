using System;
using Characters.CombatSystems;
using Characters.HealthSystems;
using Characters.InputSystems;
using Characters.MovementSystems;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Controllers
{
    public abstract class BaseController : MonoBehaviour
    {
        [Title("Dependencies")] 
        [SerializeField] private BaseInputSystem inputSystem;
        [SerializeField] private MovementSystem movementSystem;
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private CombatSystem combatSystem;
        
        [Title("States")] 
        [SerializeField] private MovementState movementState;
        [SerializeField] private CombatState combatState;

        public MovementSystem MovementSystem => movementSystem;
        public HealthSystem HealthSystem => healthSystem;
        public CombatSystem CombatSystem => combatSystem;
        
        private void OnEnable()
        {
            if (!inputSystem) return;
            inputSystem.OnMoveInputPerform += MovementSystem.TryMoveAction;
            inputSystem.OnAttackInputPerform += combatSystem.Attack;
            inputSystem.OnGuardInputPerform += combatSystem.Guard;
        }

        private void OnDisable()
        {
            if (!inputSystem) return;
            inputSystem.OnMoveInputPerform -= MovementSystem.TryMoveAction;
            inputSystem.OnAttackInputPerform -= combatSystem.Attack;
            inputSystem.OnGuardInputPerform -= combatSystem.Guard;
        }
    }

    [Serializable]
    public enum MovementState
    {
        Idle = 0,
        Jumping = 1,
    }

    [Serializable]
    public enum CombatState
    {
        None = 0,
        Attacking = 1,
        Guarding = 2,
    }
}