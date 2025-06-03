using System;
using Characters.MovementSystems;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Controllers
{
    public class PlayerController : MMSingleton<PlayerController>
    {
        [Title("Dependencies")]
        [SerializeField] private MovementSystem _movementSystem;
        
        [Title("States")] 
        [SerializeField] private MovementState movementState;
        [SerializeField] private CombatState combatState;

        public MovementSystem MovementSystem => _movementSystem;
        //[Title("Dependencies")]
        
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
  
