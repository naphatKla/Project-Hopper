using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Characters.InputSystems
{
    public class PlayerInputSystem : BaseInputSystem
    {
        [SerializeField] private Button attackButton;
        [SerializeField] private Button moveButton;
        [SerializeField] private Button guardButton;
        
        public override Action OnAttackInputPerform { get; set; }
        public Action OnMoveInputPerform { get; set; }
        public Action OnGuardInputPerform { get; set; }
        
        private void OnEnable()
        {
            moveButton?.onClick.AddListener(PerformMove);
            attackButton?.onClick.AddListener(PerformAttack);
            guardButton?.onClick.AddListener(PerformGuard);
        }

        private void OnDisable()
        {
            moveButton?.onClick.RemoveListener(PerformMove);
            attackButton?.onClick.RemoveListener(PerformAttack);
            guardButton?.onClick.RemoveListener(PerformGuard);
        }
        
        [BoxGroup] [Button(ButtonSizes.Large)]
        protected override void PerformAttack()
        {
            OnAttackInputPerform?.Invoke();
        }
        
        [BoxGroup] [Button(ButtonSizes.Large)] 
        protected void PerformMove()
        {
            OnMoveInputPerform?.Invoke();
        }

        [BoxGroup] [Button(ButtonSizes.Large)]
        protected void PerformGuard()
        {
            OnGuardInputPerform?.Invoke();
        }
    }
}
