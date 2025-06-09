using UnityEngine;

[System.Serializable]
public class ObjectPlatformEffect : MonoBehaviour
{
    public string name;
    public GameObject gameObject;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Collider2D collider;

    public void Init()
    {
        animator = gameObject?.GetComponent<Animator>();
        collider = gameObject?.GetComponent<Collider2D>();
    }
}
