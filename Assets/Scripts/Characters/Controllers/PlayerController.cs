using System;
using UnityEngine;

namespace Characters.Controllers
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private MovementState movementState;
    }

    [Serializable]
    public enum MovementState
    {
        Idle,
        Jumping,
    }

    [Serializable]
    public enum CombatState
    {
        None,
        Attacking,
        Guarding,
    }
}
  
