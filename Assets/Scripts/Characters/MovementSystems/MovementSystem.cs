using System;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private float moveHorizontalDuration;
        
        [Unit(Units.Second)]
        [SerializeField] private float movVerticallDuration;

        [SerializeField] private bool snapGridOnStart;

        private bool _isGrounded;
        private BoxCollider2D _boxCollider2D;
        
        private async void Start()
        {
            if (!snapGridOnStart) return;
            if (!TryGetComponent(out _boxCollider2D))
            {
                Debug.LogWarning("Need BoxCollider2D with is Trigger");
                return;
            }

            await UniTask.WaitForSeconds(0.1f);
            transform.position = SnapToGrid(transform.position);
        }
        
        [Button]
        public void TryMove()
        {
            //if (!_isGrounded) return;

            Debug.Log("MOVE");
        }

        private bool CheckCanMove()
        {
            Bounds bounds = _boxCollider2D.bounds;
            Vector2 topCenter = bounds.center + new Vector3(0f, bounds.extents.y);
            Vector2 rayStartPos = topCenter + new Vector2(0, moveVerticalDistance);
            Debug.DrawRay(rayStartPos, Vector2.right * moveHorizontalDistance, Color.red, 0.1f);
            if (Physics2D.Raycast(rayStartPos, Vector2.right, moveHorizontalDistance, collisionLayer)) return false;
            return true;
        }

        private bool CheckGround(Vector2 origin, out RaycastHit2D hit, float length)
        {
            hit = Physics2D.Raycast(origin, Vector2.down, length, collisionLayer);
            Debug.DrawRay(origin, Vector2.down * length, hit.collider ? Color.green : Color.red, 0.1f);
            return hit.collider != null;
        }
        
        private Vector2 SnapToGrid(Vector2 pos)
        {
            float newX = MathF.Floor(pos.x) + gridOffset.x;
            float newY = MathF.Floor(pos.y) + gridOffset.y;

            Vector2 snappedPos = new Vector2(newX, newY);
            float deltaY = pos.y - newY;

            if (deltaY > 0 && CheckGround(pos, out RaycastHit2D hit, deltaY + 0.01f))
            {
                Debug.Log(hit.transform.name);
                snappedPos = hit.point + hit.normal * 0.01f;
            }
               
            
           
            return snappedPos;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log(other.transform.name);
        }
    }
}
