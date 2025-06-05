using System.Threading;
using Characters.Controllers;
using Characters.HealthSystems;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.CombatSystems
{
    public abstract class BaseCombatSystem : MonoBehaviour
    {
        #region Inspectors & Variables

        [Title("Configs")]  /////////////////////////////////////////////////
        [PropertyTooltip("")]
        [SerializeField] private Vector2 attackArea = new Vector2(2, 1);
        
        [PropertyTooltip("")]
        [SerializeField] private Vector2 offset = new Vector2(-0.5f, 0);
        
        [PropertyTooltip("")]
        [SerializeField] private LayerMask targetLayer;

        [Title("Stats")] /////////////////////////////////////////////////
        [PropertyTooltip("")]
        [SerializeField] private float damage = 1f;
        
        [PropertyTooltip("")]
        [SerializeField] private float attackDelay = 0.1f;
        
        [PropertyTooltip("")]
        [SerializeField] private float attackCooldown = 0.1f;
        
        /// <summary>
        /// 
        /// </summary>
        private Vector2 _attackStartPos;
        
        /// <summary>
        /// 
        /// </summary>
        private bool _isAttackCooldown;
        
        /// <summary>
        /// 
        /// </summary>
        protected BaseController owner;
        
        /// <summary>
        /// 
        /// </summary>
        protected bool isInitialized;
        
        /// <summary>
        /// 
        /// </summary>
        private CancellationTokenSource _ct;

        #endregion

        #region Unity Methods

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            _ct?.Cancel();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Vector2 start = (Vector2)transform.position + offset;
            Vector2 boxCenter = start + new Vector2(attackArea.x, attackArea.y) * 0.5f;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boxCenter, attackArea);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        public void Initialize(BaseController controller)
        {
            owner = controller;
            isInitialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public async void Attack()
        {
            if (!isInitialized) return;
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

            await UniTask.WaitForSeconds(attackCooldown - attackDelay);
            _isAttackCooldown = false;
        }
        
        #endregion
    }
}