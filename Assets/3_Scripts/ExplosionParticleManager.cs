using DG.Tweening;
using UnityEngine;

public class ExplosionParticleManager : MonoBehaviour
{
    [SerializeField] private float distance;
    [SerializeField] private float time;

    public void Initialize(Vector3 _direction)
    {
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(transform.position + _direction * distance, time).SetEase(Ease.OutSine));
        sequence.Join(transform.DORotate(Vector3.forward * 360f, time, RotateMode.WorldAxisAdd).SetEase(Ease.OutSine));
        sequence.Play().OnComplete(DestroySelf);
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
