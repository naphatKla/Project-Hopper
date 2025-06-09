using Sirenix.OdinInspector;
using UnityEngine;

namespace Cameras
{
    public class ParallaxLayer : MonoBehaviour
    {
        [PropertyTooltip("0, parallax does not move, 1, parallax move instantly along with camera position")]
        [SerializeField] private float parallaxEffectMultiplier;
        [SerializeField] private bool loopParallax = true;
        
        private float startPos;
        private float length;
        
        void Start()
        {
            startPos = transform.position.x;
            length = GetComponent<SpriteRenderer>().bounds.size.x;
        }
        
        void FixedUpdate()
        {
            Vector2 camPos = Camera.main.transform.position;
            float distance = camPos.x * parallaxEffectMultiplier;
            float movement = camPos.x * (1 - parallaxEffectMultiplier);

            transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

            if (movement > startPos + length)
                startPos += length;
            else if (movement < startPos - length)
                startPos -= length;
        }
    }
}
