using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.MovementSystems
{
    public class MovementSystem : MonoBehaviour
    {
        [Title("Configs")] 
        [SerializeField] private Vector2 gridOffset = new Vector2(0.5f, 0);
        [SerializeField] private LayerMask collisionLayer;
        
        [Title("Movement Stats")]
        [PropertyTooltip("(Units)")]
        [SerializeField] private int moveHorizontalDistance;
        
        [PropertyTooltip("(Units)")]
        [SerializeField] private int moveVerticalDistance;
        
        [Unit(Units.Second)]
        [SerializeField] private float moveDuration;

        [SerializeField] private bool snapGridOnStart;

        private bool _isGrounded;
        private bool _canMove;
        private BoxCollider2D _boxCollider2D;
        
        private void Start()
        {
            if (!snapGridOnStart) return;
            if (!TryGetComponent(out _boxCollider2D))
            {
                Debug.LogWarning("Need BoxCollider2D with is Trigger");
                return;
            }
            
            transform.position = SnapToGrid(transform.position);
        }

        [Button]
        public void TryMove()
        {
            //if (!_isGrounded) return;
            Bounds bounds = _boxCollider2D.bounds;
            Vector2 topCenter = bounds.center + new Vector3(0f, bounds.extents.y);
            Vector2 rayStartPos = topCenter + new Vector2(0, moveVerticalDistance);
            if(Physics2D.Raycast(rayStartPos, Vector2.right, moveHorizontalDistance, collisionLayer)) return;
            _canMove = true;
            Debug.Log("MOVE");
        }
        
        private Vector2 SnapToGrid(Vector2 pos)
        {
            float x = MathF.Floor(MathF.Abs(pos.x)) + gridOffset.x;
            float y = MathF.Floor(MathF.Abs(pos.y)) + gridOffset.y;
            return new Vector2(MathF.Sign(pos.x) * x, MathF.Sign(pos.y) * y);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log(other.transform.name);
        }
    }
}
