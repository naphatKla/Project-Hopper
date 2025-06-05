using System;
using UnityEngine;

namespace Characters.InputSystems
{
    public abstract class BaseInputSystem : MonoBehaviour
    {
        public abstract Action OnAttackInputPerform { get; set; }
        protected abstract void PerformAttack();
    }
}
