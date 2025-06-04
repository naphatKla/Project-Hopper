using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Characters.InputSystems
{
    public class PlayerInputSystem : BaseInputSystem
    {
        [SerializeField] private Button moveButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button guardButton;
        
        public override Action OnMoveInputPerform { get; set; }
        public override Action OnAttackInputPerform { get; set; }
        public override Action OnGuardInputPerform { get; set; }
        
        private void OnEnable()
        {
            moveButton?.onClick.AddListener(PerformMove);
            attackButton?.onClick.AddListener(PerformAttack);
            guardButton?.onClick.AddListener(PerformGuard);
        }
        
        [Button]
        protected override void PerformMove()
        {
            OnMoveInputPerform?.Invoke();
        }

        [Button]
        protected override void PerformAttack()
        {
            OnAttackInputPerform?.Invoke();
        }

        [Button]
        protected override void PerformGuard()
        {
            OnGuardInputPerform?.Invoke();
        }
    }
}
