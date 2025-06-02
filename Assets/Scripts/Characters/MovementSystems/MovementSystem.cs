using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.MovementSystems
{
    public class MovementSystem : MonoBehaviour
    {
        [Title("Configs")] [SerializeField] private Vector2 gridOffset = new Vector2(0.5f, 0);
        [SerializeField] private LayerMask collisionLayer;
        [SerializeField] private float gravityScale = -9.81f;

        [Title("Movement Stats")] [PropertyTooltip("(Units)")] [SerializeField]
        private int moveHorizontalDistance = 1;

        [PropertyTooltip("(Units)")] [SerializeField]
        private int moveVerticalDistance = 1;

        [Unit(Units.Second)] [SerializeField] private float moveDuration = 0.25f;

        [SerializeField] private AnimationCurve moveCurve;
        
        [SerializeField] private bool snapGridOnStart;

        private bool _isGrounded;
        private bool _isLanding;
        private bool _ignoreGravity;
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

        private void Update()
        {
            GravityHandler();
        }

        [Button]
        public async void TryMoveAction()
        {
            if (!_isGrounded) return;
            if (CheckObstacle()) return;

            _ignoreGravity = true;

            Vector2 startPos = transform.position;
            Vector2 horizontalOffset = Vector2.right * moveVerticalDistance;

            await DOTween.To(() => 0f, t =>
                {
                    Vector2 horizontal = Vector2.Lerp(startPos, startPos + horizontalOffset, t);

                    float arc = moveCurve.Evaluate(t);
                    float yOffset = moveVerticalDistance * arc + 0.1f;

                    Vector2 curvedPos = new Vector2(horizontal.x, startPos.y + yOffset);
                    MovePosition(curvedPos);

                }, 1f, moveDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    _ignoreGravity = false;
                });
        }

        private void MovePosition(Vector2 newPos)
        {
            if (newPos.y < transform.position.y)
            {
                float deltaY = MathF.Abs(transform.position.y - newPos.y);
                if (CheckGround(transform.position, out RaycastHit2D groundHit, deltaY))
                {
                    if (_isLanding) return;
                    transform.position = SnapToGrid(transform.position);
                    _isLanding = true;
                    return;
                }

                _isLanding = false;
            }

            transform.position = newPos;
        }

        private void GravityHandler()
        {
            if (_ignoreGravity) return;
            Vector2 currentPos = transform.position;
            Vector2 newPos = currentPos + Vector2.up * (gravityScale * Time.deltaTime);
            MovePosition(newPos);
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
            float newY = MathF.Round(pos.y) + gridOffset.y;

            Vector2 snappedPos = new Vector2(newX, newY);
            float deltaY = pos.y - newY;

            if (deltaY > 0 && CheckGround(pos, out RaycastHit2D hit, deltaY))
                snappedPos = hit.point + hit.normal;

            return snappedPos;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log(other.transform.name);
        }
    }
}