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
        Sequence attackSeq = DOTween.Sequence();
        // หมุนไป
        attackSeq.AppendCallback(() => _isAttack = true);
        attackSeq.Append(transform.DORotate(new Vector3(0, 0, -180), 1, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine));
        
        //รอ
        attackSeq.AppendCallback(() => _isAttack = false);
        attackSeq.AppendInterval(0.33f);
        
        //หมุนกลับ
        attackSeq.AppendCallback(() => _isAttack = true);
        attackSeq.Append(transform.DORotate(new Vector3(0, 0, 180), 1, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine));
        
        //รอ
        attackSeq.AppendCallback(() => _isAttack = false);
        attackSeq.AppendInterval(0.33f);
        attackSeq.SetLoops(-1);


    }
    
}
