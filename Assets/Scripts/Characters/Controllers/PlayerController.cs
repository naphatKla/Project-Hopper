using UnityEngine;

namespace Characters.Controllers
{
    public class PlayerController : MonoBehaviour
    {  
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
         
        }
 
        // Update is called once per frame
        void Update()
        {
         
        }
    }

    public enum MovementState
    {
        Idle,
        Jumping,
    }

    public enum CombatState
    {
        None,
        Attacking,
        Guarding,
    }
}
  
