using Characters.HealthSystems;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.CombatSystems
{
    public class CombatSystem : MonoBehaviour
    {
        [Title("Configs")]
        [SerializeField] private Vector2 attackArea = new Vector2(2, 1);
        [SerializeField] private Vector2 offset = new Vector2(-0.5f, 0);
        [SerializeField] private LayerMask targetLayer;

        [Title("Stats")] 
        [SerializeField] private float damage;
        private Vector2 _attackStartPos;

        public void Attack()
        {
            _attackStartPos = (Vector2)transform.position + offset;
            Vector2 boxCenter = _attackStartPos + new Vector2(attackArea.x, attackArea.y) * 0.5f;
            
            Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, attackArea, 0f, targetLayer);
            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent(out HealthSystem targetHealth)) continue;
                targetHealth.TakeDamage(damage);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 start = (Vector2)transform.position + offset;
            Vector2 boxCenter = start + new Vector2(attackArea.x, attackArea.y) * 0.5f;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boxCenter, attackArea);
        }
    }
}
