using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SwingAxe : MonoBehaviour
{
    [SerializeField] private float _rotateSpeed;
    
    void Start()
    {
        transform.rotation = Quaternion.Euler(new Vector3(0,0,90));
        Sequence rotate = DOTween.Sequence();
        rotate.SetDelay(0.33f);
        rotate.Append(transform.DORotate(new Vector3(0, 0, -90), 1).SetEase(Ease.InOutSine));
        rotate.AppendInterval(0.33f);
        rotate.SetLoops(-1, LoopType.Yoyo);
        
        /*
        Sequence s = DOTween.Sequence();
        s.SetDelay(1f);
        s.Append(cube.DOLocalRotate(rotacija, duration).SetRelative().SetEase(ease));
        s.AppendInterval(1f);
        s.SetLoops(-1, LoopType.Yoyo);*/
    }
    
}
