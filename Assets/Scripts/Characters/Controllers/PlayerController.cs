using System;
using Characters.InputSystems;
using Characters.MovementSystems;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Controllers
{
    public class PlayerController : MMSingleton<PlayerController>
    {
        [Title("Dependencies")] 
        [SerializeField] private BaseInputSystem inputSystem;
        [SerializeField] private MovementSystem movementSystem;
        
        [Title("States")] 
        [SerializeField] private MovementState movementState;
        [SerializeField] private CombatState combatState;

        public MovementSystem MovementSystem => movementSystem;

        private void OnEnable()
        {
            if (!inputSystem) return;
            inputSystem.OnMoveInputPerform += MovementSystem.TryMoveAction;
        }

        private void OnDisable()
        {
            if (!inputSystem) return;
            inputSystem.OnMoveInputPerform -= MovementSystem.TryMoveAction;
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
  
