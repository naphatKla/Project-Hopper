using System;
using Sirenix.OdinInspector;

namespace Characters.InputSystems
{
    public class EnemyInputSystem : BaseInputSystem
    {
        public override Action OnMoveInputPerform { get; set; }
        public override Action OnAttackInputPerform { get; set; }
        public override Action OnGuardInputPerform { get; set; }
        
        [PropertySpace] [Button]
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
