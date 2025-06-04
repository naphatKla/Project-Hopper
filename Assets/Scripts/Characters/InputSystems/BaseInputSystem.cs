using System;
using UnityEngine;

namespace Characters.InputSystems
{
    public abstract class BaseInputSystem : MonoBehaviour
    {
        public abstract Action OnMoveInputPerform { get; set; }
        public abstract Action OnAttackInputPerform { get; set; }
        public abstract Action OnGuardInputPerform { get; set; }

        protected abstract void PerformMove();
        protected abstract void PerformAttack();
        protected abstract void PerformGuard();
    }
}
