using System;
using System.Numerics;
using System.Threading;
using Characters.HealthSystems;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Platform;
using Sirenix.OdinInspector;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class ObstacleBlock : MonoBehaviour
{
    [FoldoutGroup("Idle Setting")] 
    [SerializeField] private float idleTimer;
    [FoldoutGroup("Idle Setting")] 
    [SerializeField] private float distanceIdle;
    
    [FoldoutGroup("Attack Setting")]
    [SerializeField] private float damage;
    [FoldoutGroup("Attack Setting")]
    [SerializeField] private Vector2 lastAttackBoxSize;
    [FoldoutGroup("Attack Setting")]
    [SerializeField] private Vector2 lastAttackBoxOffset;
    [FoldoutGroup("Attack Setting")]
    [SerializeField] private LayerMask attackLayer;
    
    private CancellationToken _cts;

    [SerializeField]
    [ReadOnly] private Vector2 originalPosition;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Vector2 center = (Vector2)transform.position + lastAttackBoxOffset;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, lastAttackBoxSize, 0f, attackLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out HealthSystem health)) health.TakeDamage(damage);
        }
    }

    private void OnEnable()
    {
        _ = InitializeAsync();
    }

    private async UniTask InitializeAsync()
    {
        await UniTask.Yield();
        originalPosition = transform.position;

        _cts = new CancellationTokenSource().Token;
        //LoopBehavior(_cts).Forget();
    }

    private async UniTaskVoid LoopBehavior(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await transform.DOMoveY(originalPosition.y + distanceIdle, idleTimer).ToUniTask();
                await UniTask.Delay(TimeSpan.FromSeconds(idleTimer/2));
                
                await transform.DOMoveY(originalPosition.y, idleTimer).ToUniTask();
                await UniTask.Delay(TimeSpan.FromSeconds(idleTimer/2));
            }
        }
        catch (OperationCanceledException) { }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + lastAttackBoxOffset, lastAttackBoxSize);
    }

}
