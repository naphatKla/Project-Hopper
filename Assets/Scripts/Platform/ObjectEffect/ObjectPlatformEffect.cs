using UnityEngine;

[System.Serializable]
public class ObjectPlatformEffect : MonoBehaviour
{
    public string name;
    public GameObject gameObject;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public Collider2D collider;

    public void Init()
    {
        spriteRenderer = gameObject?.GetComponent<SpriteRenderer>();
        collider = gameObject?.GetComponent<Collider2D>();
    }
}
