using System;
using Sirenix.OdinInspector;

namespace Characters.InputSystems
{
    public class EnemyInputSystem : BaseInputSystem
    {
        public override Action OnAttackInputPerform { get; set; }
        
        [Button]
        protected override void PerformAttack()
        {
            OnAttackInputPerform?.Invoke();
        }
    }
}
