using System;
using Characters.Controllers;
using UnityEngine;

namespace Characters.InputSystems
{
    public abstract class BaseInputSystem : MonoBehaviour
    {
        protected BaseController owner;
        public abstract Action OnAttackInputPerform { get; set; }
        protected abstract void PerformAttack();

        public virtual void Initialize(BaseController controller)
        {
            owner = controller;
        }
    }
}
