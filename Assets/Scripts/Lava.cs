using Characters.Controllers;
using Characters.HealthSystems;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class Lava : MonoBehaviour
{
    [SerializeField] private float riseUpDuration = 7f;
    [SerializeField] private Vector2 minMaxHight = new Vector2(-3, 0.5f);
    [SerializeField] private float reduceElapsedOnLand = 0.1f;
    private float elapsed;

    [Title("Lava Oscillation Settings")] 
    [SerializeField] private float oscillateAmplitude = 0.2f; 
    [SerializeField] private float oscillateFrequency = 1f;
    
    private void Start()
    {
        if (PlayerController.Instance != null && PlayerController.Instance.GridMovementSystem != null)
            PlayerController.Instance.GridMovementSystem.OnLandingAfterJump += OnPlayerLanded;
        
        transform.position = new Vector3(transform.position.x, minMaxHight.x, transform.position.z);
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null && PlayerController.Instance.GridMovementSystem != null)
            PlayerController.Instance.GridMovementSystem.OnLandingAfterJump -= OnPlayerLanded;
    }

    private void OnPlayerLanded()
    {
        elapsed -= reduceElapsedOnLand;
        elapsed = Mathf.Max(0, elapsed);
    }
    
    void FixedUpdate()
    {
        if (riseUpDuration > 0)
            elapsed += Time.deltaTime / riseUpDuration;
        else 
            elapsed = 1; 
        
        elapsed = Mathf.Min(1, elapsed);
        
        float targetYPos = Mathf.Lerp(minMaxHight.x, minMaxHight.y, elapsed);
        float oscillation = Mathf.Sin(Time.time * oscillateFrequency) * oscillateAmplitude; 
        float finalYPos = targetYPos + oscillation;
        Vector2 newPos = new Vector3(transform.position.x, finalYPos, transform.position.z);
        transform.position = newPos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Call");
        if (!other.TryGetComponent(out HealthSystem healthSystem)) return;
        healthSystem.ForceDead().Forget(); 
    }

    [Button("Reset Elapsed")]
    private void ResetElapsed()
    {
        elapsed = 0;
        transform.position = new Vector3(transform.position.x, minMaxHight.x, transform.position.z);
        Debug.Log("Lava elapsed reset to 0.");
    }
}