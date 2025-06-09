using System;
using Characters.Controllers;
using Characters.HealthSystems;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class Lava : MonoBehaviour
{
[SerializeField] private float riseUpDuration = 5f;
    [SerializeField] private Vector2 minMaxHight = new Vector2(-3, 0.5f);
    [SerializeField, Tooltip("ระยะเวลาที่ลดลงเมื่อผู้เล่นกระโดดลงพื้น (ในหน่วย elapsed)")]
    private float reduceElapsedOnLand = 0.25f;
    private float elapsed;

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
    
    void Update()
    {
        if (riseUpDuration > 0)
            elapsed += Time.deltaTime / riseUpDuration;
        else 
            elapsed = 1; 
        
        elapsed = Mathf.Min(1, elapsed);

        float yPos = Mathf.Lerp(minMaxHight.x, minMaxHight.y, elapsed);
        transform.position = new Vector3(transform.position.x, yPos, transform.position.z);
    }

    private async void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out HealthSystem healthSystem)) return;
        await healthSystem.ForceDead();
    }

    // คุณอาจจะเพิ่ม Button เพื่อ Reset ค่า elapsed สำหรับการทดสอบ
    [Button("Reset Elapsed")]
    private void ResetElapsed()
    {
        elapsed = 0;
        transform.position = new Vector3(transform.position.x, minMaxHight.x, transform.position.z);
    }
    
}
