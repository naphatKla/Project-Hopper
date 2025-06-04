using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.MovementSystems
{
    public class MovementSystem : MonoBehaviour
    {
        #region Inspectors & Variables
        
        [Title("Configs")] [PropertyTooltip("Offset in the grid. Depends on the pivot point of transform.")]
        [SerializeField] private Vector2 gridOffset = new Vector2(0.5f, 0);
        
        [PropertyTooltip("Ground layer to detected and perform collision with.")]
        [SerializeField] private LayerMask groundLayerMask;
        
        [PropertyTooltip("Obstacle layer to block the player movement.")]
        [SerializeField] private LayerMask obstacleLayerMask;
        
        [PropertyTooltip("Gravity scale of this entity. the sign is mean the gravity's direction.")]
        [SerializeField] private float gravityScale = -9.81f;

        [Title("Movement Stats")] [PropertyTooltip("Movement distance in horizontal axis (Units)")] 
        [ValidateInput("@moveHorizontalDistance >= 0", "this value can't below than zero.")]
        [SerializeField] private int moveHorizontalDistance = 1;

        [PropertyTooltip("Movement distance in vertical axis (Units)")] 
        [ValidateInput("@moveHorizontalDistance >= 0", "this value can't below than zero.")]
        [SerializeField] private int moveVerticalDistance = 1;

        [PropertyTooltip("The duration of movement action")]  [Unit(Units.Second)] 
        [ValidateInput("@moveHorizontalDistance >= 0", "this value can't below than zero.")]
        [SerializeField] private float moveDuration = 0.25f;

        [PropertySpace]
        [InfoBox("You can adjust movement curve. \nThe start and end point need to be zero. and the highest value should be one.")] 
        [SerializeField] private AnimationCurve moveCurve;
        
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
        
        private void Awake()
        {
            OnLanding += HandleLanding;
        }


        /// <summary>
        /// Initialize and dependency condition.
        /// </summary>
        private void Start()
        {
            if (!TryGetComponent(out _boxCollider2D) && !_boxCollider2D.isTrigger)
                Debug.LogWarning("Need BoxCollider2D with is Trigger");
        }

        /// <summary>
        /// Calculate and control the gravity.
        /// </summary>
        private void Update()
        {
            GravityHandler();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The primary movement of this game. Try to move and jump to the next grid, depend on the movement distance
        /// Move when the condition is true and return ( not do any action ) if the condition is false.
        /// </summary>
        public async void TryMoveAction()
        {
            if (!_isGrounded) return;
            if (CheckObstacle()) return;

            OnJumpUp?.Invoke();
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
            return Physics2D.Raycast(transform.position, Vector2.right, moveHorizontalDistance, obstacleLayerMask);
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
                snappedPos.y = hit.collider.ClosestPoint(hit.point).y;
            
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