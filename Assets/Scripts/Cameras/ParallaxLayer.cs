using Sirenix.OdinInspector;
using UnityEngine;

namespace Cameras
{
    public class ParallaxLayer : MonoBehaviour
    {
        [PropertyTooltip("0, parallax does not move, 1, parallax move instantly along with camera position")]
        [SerializeField] private float parallaxEffectMultiplierX;

        [SerializeField] private bool useYAxis = false;
        
        [ShowIf(nameof(useYAxis))]
        [PropertyTooltip("0, parallax does not move, 1, parallax move instantly along with camera position")]
        [SerializeField] private float parallaxEffectMultiplierY; 

        [SerializeField] private bool loopParallaxX = true; 
        
        [ShowIf(nameof(useYAxis))]
        [SerializeField] private bool loopParallaxY = false; 
        
        private float startPosX;
        private float lengthX;
        
        private float startPosY; 
        private float lengthY; 
        
        void Start()
        {
            startPosX = transform.position.x;
            lengthX = GetComponent<SpriteRenderer>().bounds.size.x;

            startPosY = transform.position.y; 
            lengthY = GetComponent<SpriteRenderer>().bounds.size.y; 
        }
        
        void FixedUpdate()
        {
            Vector2 camPos = Camera.main.transform.position;
            
            float distanceX = camPos.x * parallaxEffectMultiplierX;
            float movementX = camPos.x * (1 - parallaxEffectMultiplierX);
            
            float distanceY = camPos.y * parallaxEffectMultiplierY; // คำนวณ distance สำหรับ Y
            float movementY = camPos.y * (1 - parallaxEffectMultiplierY); // คำนวณ movement สำหรับ Y
            
            transform.position = new Vector3(
                startPosX + distanceX,
                 useYAxis? startPosY + distanceY : transform.position.y, 
                transform.position.z
            );
            
            if (loopParallaxX)
            {
                if (movementX > startPosX + lengthX)
                    startPosX += lengthX;
                else if (movementX < startPosX - lengthX)
                    startPosX -= lengthX;
            }
            
            if (loopParallaxY)
            {
                if (movementY > startPosY + lengthY)
                    startPosY += lengthY;
                else if (movementY < startPosY - lengthY)
                    startPosY -= lengthY;
            }
        }
    }
}