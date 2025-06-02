using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.MovementSystems
{
    public class MovementSystem : MonoBehaviour
    {
        [Title("Configs")] 
        [SerializeField] private Vector2 gridOffset = new Vector2(0.5f, 0);
        [SerializeField] private LayerMask collisionLayer;
        [SerializeField] private float gravityScale = -9.81f;

        [Title("Movement Stats")] 
        [PropertyTooltip("(Units)")] 
        [SerializeField] private int moveHorizontalDistance = 1;

        [PropertyTooltip("(Units)")] 
        [SerializeField] private int moveVerticalDistance = 1;

        [Unit(Units.Second)] 
        [SerializeField] private float moveHorizontalDuration = 0.25f;
        
        [Unit(Units.Second)] 
        [SerializeField] private float moveVerticalDuration = 0.1f;

        [SerializeField] private bool snapGridOnStart;

        private bool _isGrounded;
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
        
        private void Update()
        {
            if (_isGrounded) return;
            Vector2 currentPos = transform.position;
            Vector2 newPos = currentPos + Vector2.up * (gravityScale * Time.deltaTime);
            MovePosition(newPos);
        }

        [Button]
        public async void TryMove()
        {
            if (!_isGrounded) return;
            if (CheckObstacle()) return;

            Vector2 verticalPos = (Vector2)transform.position + Vector2.up * moveVerticalDistance;
            await DOTween.To(() => 0f, t =>
            {
                Vector2 newPos = Vector2.Lerp(transform.position, verticalPos, t);
                MovePosition(newPos);
            }, 1f, moveVerticalDuration).AsyncWaitForCompletion();
            
            Vector2 horizontalPos = (Vector2)transform.position + Vector2.right * moveVerticalDistance;
            DOTween.To(() => 0f, t =>
            {
                Vector2 newPos = Vector2.Lerp(transform.position, horizontalPos, t);
                MovePosition(newPos);
            }, 1f, moveHorizontalDuration);
        }

        private void MovePosition(Vector2 newPos)
        {
            if (newPos.y < transform.position.y)
            {
                float deltaY = MathF.Abs(transform.position.y - newPos.y);
                if (!CheckGround(transform.position, out RaycastHit2D groundHit, deltaY))
                {
                    transform.position = SnapToGrid(transform.position);
                    return;
                }
            }
            transform.position = newPos;
        }

        private bool CheckObstacle()
        {
            Bounds bounds = _boxCollider2D.bounds;
            Vector2 topCenter = bounds.center + new Vector3(0f, bounds.extents.y);
            Vector2 rayStartPos = topCenter + new Vector2(0, moveVerticalDistance);
            Debug.DrawRay(rayStartPos, Vector2.right * moveHorizontalDistance, Color.red, 0.1f);
            return Physics2D.Raycast(rayStartPos, Vector2.right, moveHorizontalDistance, collisionLayer);
        }

        private bool CheckGround(Vector2 origin, out RaycastHit2D hit, float length)
        {
            hit = Physics2D.Raycast(origin, Vector2.down, length, collisionLayer);
            _isGrounded = hit.collider;
            Debug.DrawRay(origin, Vector2.down * length, hit.collider ? Color.green : Color.red, 0.1f);
            return _isGrounded;
        }
        
        private Vector2 SnapToGrid(Vector2 pos)
        {
            float newX = MathF.Floor(pos.x) + gridOffset.x;
            float newY = MathF.Floor(pos.y) + gridOffset.y;

            Vector2 snappedPos = new Vector2(newX, newY);
            float deltaY = pos.y - newY;

            if (deltaY > 0 && CheckGround(pos, out RaycastHit2D hit, deltaY + 0.01f))
                snappedPos = hit.point + hit.normal * 0.01f;
            
            return snappedPos;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log(other.transform.name);
        }
    }
}
