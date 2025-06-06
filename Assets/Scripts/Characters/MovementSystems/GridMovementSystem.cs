using System;
using Characters.Controllers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.MovementSystems
{
    public class GridMovementSystem : MonoBehaviour
    {
        #region Inspectors & Variables

        [Title("Configs")]
        [PropertyTooltip("Offset in the grid. Depends on the pivot point of transform.")]
        [SerializeField]
        private Vector2 gridOffset = new Vector2(0.5f, 0);

        [PropertyTooltip("Ground layer to detected and perform collision with.")] [SerializeField]
        private LayerMask groundLayerMask;

        [PropertyTooltip("Obstacle layer to block the player movement.")] [SerializeField]
        private LayerMask obstacleLayerMask;

        [PropertyTooltip("Gravity scale of this entity. the sign is mean the gravity's direction.")] [SerializeField]
        private float gravityScale = -9.81f;

        [Title("Movement Stats")]
        [PropertyTooltip("Movement distance in horizontal axis (Units)")]
        [ValidateInput("@moveHorizontalDistance >= 0", "this value can't below than zero.")]
        [SerializeField]
        private int moveHorizontalDistance = 1;

        [PropertyTooltip("Movement distance in vertical axis (Units)")]
        [ValidateInput("@moveHorizontalDistance >= 0", "this value can't below than zero.")]
        [SerializeField]
        private int moveVerticalDistance = 1;

        [PropertyTooltip("The duration of movement action")]
        [Unit(Units.Second)]
        [ValidateInput("@moveHorizontalDistance >= 0", "this value can't below than zero.")]
        [SerializeField]
        private float moveDuration = 0.25f;

        [PropertyTooltip("The duration of movement cooldown")]
        [Unit(Units.Second)]
        [ValidateInput("@moveHorizontalDistance >= 0", "this value can't below than zero.")]
        [SerializeField]
        private float moveCooldown = 0.25f;

        [PropertySpace]
        [InfoBox(
            "You can adjust movement curve. \nThe start and end point need to be zero. and the highest value should be one.")]
        [SerializeField]
        private AnimationCurve moveCurve;

        /// <summary>
        /// is this character initialized by the owner or not.
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Determine this character move is cooldown or not.
        /// </summary>
        private bool _isMoveCooldown;

        /// <summary>
        /// Determine this character is on ground or not.
        /// </summary>
        private bool _isGrounded;

        /// <summary>
        /// Determine this character touch the ground from falling or not. Should be true when touch the ground first time from falling state.
        /// </summary>
        private bool _isLanding;

        /// <summary>
        /// True is mean use gravity calculation.
        /// </summary>
        private bool _ignoreGravity;

        /// <summary>
        /// Owner of this character.
        /// </summary>
        private BaseController _owner;

        /// <summary>
        /// The box collider of this character.
        /// </summary>
        private BoxCollider2D _boxCollider2D;

        /// <summary>
        /// Invoke when the character start move or jump.
        /// </summary>
        public Action OnJumpUp { get; set; }

        /// <summary>
        /// Invoke when the character on landing from the falling state first time on the ground.
        /// GameObject is means the object landed on.
        /// </summary>
        public Action<GameObject> OnLanding { get; set; }

        #endregion

        #region Unity Methods

        /// <summary>
        /// Calculate and control the gravity.
        /// </summary>
        private void Update()
        {
            if (!_isInitialized) return;
            GravityHandler();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Call to assign the dependency from the owner controller.
        /// </summary>
        /// <param name="owner"></param>
        public void Initialize(BaseController owner)
        {
            OnLanding += HandleLanding;
            _owner = owner;

            if (!TryGetComponent(out _boxCollider2D) && !_boxCollider2D.isTrigger)
                Debug.LogWarning("Need BoxCollider2D with is Trigger");

            _isInitialized = true;
        }

        /// <summary>
        /// The primary movement of this game. Try to move and jump to the next grid, depend on the movement distance
        /// Move when the condition is true and return ( not do any action ) if the condition is false.
        /// </summary>
        public async void TryMoveAction()
        {
            if (!_isInitialized) return;
            if (_isMoveCooldown) return;
            if (!_isGrounded) return;
            if (CheckObstacle()) return;

            _isMoveCooldown = true;
            OnJumpUp?.Invoke();
            _ignoreGravity = true;
            _owner.FeedbackSystem.PlayFeedback(FeedbackKey.Jump);
            
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
                .OnComplete(() => { _ignoreGravity = false; });

            await UniTask.WaitForSeconds(Mathf.Max(0, moveCooldown - moveDuration));
            _isMoveCooldown = false;
        }

        /// <summary>
        /// Move position method, every movement need to be call in this method,
        /// Try to snap to the grid and ground in every movement changed.
        /// </summary>
        /// <param name="newPos"></param>
        private void MovePosition(Vector2 newPos)
        {
            if (newPos.y < transform.position.y)
            {
                float deltaY = MathF.Abs(transform.position.y - newPos.y);
                if (CheckGround(transform.position, out RaycastHit2D groundHit, deltaY))
                {
                    if (_isLanding) return;
                    transform.position = SnapToGrid(newPos);
                    OnLanding?.Invoke(groundHit.transform.gameObject);
                    _owner.FeedbackSystem.PlayFeedback(FeedbackKey.Land);
                    _isLanding = true;
                    return;
                }

                _isLanding = false;
            }

            transform.position = newPos;
        }

        /// <summary>
        /// Handle the gravity of this character.
        /// </summary>
        private void GravityHandler()
        {
            if (_ignoreGravity) return;
            //if (_isGrounded) return;

            Vector2 currentPos = transform.position;
            Vector2 newPos = currentPos + Vector2.up * (gravityScale * Time.deltaTime);
            MovePosition(newPos);
        }

        /// <summary>
        /// Check the obstacle in the front direction
        /// Detected ranges are depend on the movement distance.
        /// </summary>
        /// <returns>True if they have the obstacle in front of this character movement</returns>
        private bool CheckObstacle()
        {
            Bounds bounds = _boxCollider2D.bounds;
            Vector2 bottom = new Vector2(bounds.center.x, bounds.min.y);
            Vector2 middle = new Vector2(bounds.center.x, bounds.center.y);
            Vector2 top = new Vector2(bounds.center.x, bounds.max.y);
            
            RaycastHit2D hitBottom = Physics2D.Raycast(bottom, Vector2.right, moveHorizontalDistance, obstacleLayerMask);
            RaycastHit2D hitMiddle = Physics2D.Raycast(middle, Vector2.right, moveHorizontalDistance, obstacleLayerMask);
            RaycastHit2D hitTop = Physics2D.Raycast(top, Vector2.right, moveHorizontalDistance, obstacleLayerMask);
            
            Debug.DrawRay(bottom, Vector2.right * moveHorizontalDistance, Color.red);
            Debug.DrawRay(middle, Vector2.right * moveHorizontalDistance, Color.green);
            Debug.DrawRay(top, Vector2.right * moveHorizontalDistance, Color.blue);
            
            return hitBottom.collider != null || hitMiddle.collider != null || hitTop.collider != null;
        }

        /// <summary>
        /// Check the ground below.
        /// </summary>
        /// <param name="origin">Start position of ray.</param>
        /// <param name="hit">Object or ground hit.</param>
        /// <param name="length">Ray's length</param>
        /// <returns>True if they have the ground and collision object below this character.</returns>
        private bool CheckGround(Vector2 origin, out RaycastHit2D hit, float length)
        {
            hit = Physics2D.Raycast(origin, Vector2.down, length, groundLayerMask);
            _isGrounded = hit.collider;
            Debug.DrawRay(origin, Vector2.down * length, hit.collider ? Color.green : Color.red, 0.1f);
            return _isGrounded;
        }

        /// <summary>
        /// Snap the position in to grid system,
        /// in vertical axis or x-axis will automatically snap.
        /// in horizontal axis or y-axis will snap depends on the ground surface.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private Vector2 SnapToGrid(Vector2 pos)
        {
            float newX = MathF.Floor(pos.x) + gridOffset.x;
            Vector2 snappedPos = new Vector2(newX, pos.y);
            float deltaY = transform.position.y - pos.y;

            if (deltaY > 0 && CheckGround(pos, out RaycastHit2D hit, deltaY))
            {
                float surfaceY = hit.collider.bounds.max.y;
                snappedPos.y = surfaceY;
            }

            return snappedPos;
        }

        /// <summary>
        /// Call OnStepped in platformManager
        /// </summary>
        /// <param name="landedObject"></param>
        private void HandleLanding(GameObject landedObject)
        {
            if (landedObject.TryGetComponent<Platform.PlatformManager>(out var platformManager))
            {
                platformManager.OnStepped(gameObject);
            }
        }

        #endregion
    }
}