using System;
using System.Threading;
using Characters.HealthSystems;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private float damage = 1f;
        [SerializeField] private float attackDelay = 0.1f;
        [SerializeField] private float attackCooldown = 0.1f;
        private Vector2 _attackStartPos;
        private bool _isAttackCooldown;
        private CancellationTokenSource _ct;

        private void OnDisable()
        {
            _ct?.Cancel();
        }

        public async void Attack()
        {
            if (_isAttackCooldown) return;

            _ct = new CancellationTokenSource();
            await UniTask.WaitForSeconds(attackDelay, cancellationToken: _ct.Token);
            if (_ct.IsCancellationRequested) return;
            
            _isAttackCooldown = true;
            _attackStartPos = (Vector2)transform.position + offset;
            Vector2 boxCenter = _attackStartPos + new Vector2(attackArea.x, attackArea.y) * 0.5f;
            
            Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, attackArea, 0f, targetLayer);
            foreach (var hit in hits)
            {
                if (!hit.TryGetComponent(out HealthSystem targetHealth)) continue;
                targetHealth.TakeDamage(damage);
            }
            
            await UniTask.WaitForSeconds(attackCooldown);
            _isAttackCooldown = false;
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
