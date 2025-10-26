using UI;
using UnityEngine;
#if DOTWEEN
using DG.Tweening;
#endif

namespace Animations
{
    public sealed class WheelAnimator : MonoBehaviour, IWheelAnimator
    {
        [SerializeField] private Transform rotator;

        public void PlaySpin(float targetAngle, float duration, System.Action onComplete)
        {

            if (!rotator) rotator = transform;
            rotator.DOKill();
            rotator
                .DORotate(new Vector3(0, 0, -targetAngle), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}