using Sirenix.OdinInspector;
using UnityEngine;

namespace Cameras
{
    public class Parallax : MonoBehaviour
    {
        [PropertyTooltip("0 = won't move, 1 = move directly with camera")] [SerializeField]
        private Vector2 parallaxEffect;
        private Vector2 startPos;
        
        void Start()
        {
            startPos = transform.position;
        }
        
        void Update()
        {
            Vector2 parallaxDistance = Camera.main.transform.position * parallaxEffect;
            Vector2 parallaxPos = startPos + parallaxDistance;
            transform.position = new Vector3(parallaxPos.x, parallaxPos.y, transform.position.z);
        }
    }
}
