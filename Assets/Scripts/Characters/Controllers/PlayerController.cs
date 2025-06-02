using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Controllers
{
    public class PlayerController : MonoBehaviour
    {
        [Title("States")]
        [SerializeField] private MovementState movementState;
        [SerializeField] private CombatState combatState;
        
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
  
