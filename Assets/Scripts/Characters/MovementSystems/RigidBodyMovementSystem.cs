using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.MovementSystems
{
    public class RigidBodyMovementSystem : MonoBehaviour
    {
        [Title("Movement Stats")]
        [PropertyTooltip("(Units)")]
        [SerializeField] private int moveDistance;
        
        [PropertyTooltip("(Units)")]
        [SerializeField] private int jumpHigh;
        
        [Unit(Units.Second)]
        [SerializeField] private float moveDuration;

        [SerializeField] private bool snapGridOnStart;
        
        [Title("Dependency")] 
        [SerializeField] private Rigidbody2D rb2D;

        private bool _isGrounded = true;
        
        private void Start()
        {
            if (rb2D) return;
            TryGetComponent(out rb2D);
            rb2D.position = SnapToGrid(rb2D.position);
        }

        [Button]
        public void Move(MoveDirection direction)
        {
            if (!_isGrounded) return;
            if (!rb2D)
            {
                Debug.LogWarning("No rigid body in movement component!");
                return;
            }
            
            Vector2 startPos = rb2D.position;
        }
        
        public Vector2 SnapToGrid(Vector2 pos, float gridSize = 1f)
        {
            float x = Mathf.Round(pos.x / gridSize) * gridSize;
            float y = Mathf.Round(pos.y / gridSize) * gridSize;
            return new Vector2(x, y);
        }
    }

    public enum MoveDirection
    {
        Right = 0,
        Left = 1,
    }
}
