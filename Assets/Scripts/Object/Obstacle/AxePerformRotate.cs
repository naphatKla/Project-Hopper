using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class AxePerformRotate : MonoBehaviour
{
    [SerializeField] private float _rotateSpeed;
    public bool _isAttack;
    
    void Start()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0,0,90));
        Sequence rotate = DOTween.Sequence();
        rotate.SetDelay(0.33f);
        rotate.AppendCallback(() => _isAttack = true);
        rotate.Append(transform.DORotate(new Vector3(0, 0, -90), 1).SetEase(Ease.InOutSine));
        rotate.AppendCallback(() => _isAttack = false);
        rotate.AppendInterval(0.33f);
        rotate.SetLoops(-1, LoopType.Yoyo);
    }
    
}
