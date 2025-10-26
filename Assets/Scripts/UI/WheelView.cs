using Animations;
using Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class WheelView : MonoBehaviour
    {
        private IWheelAnimator animator;
        [SerializeField] private int sliceCount = 8;
        [SerializeField] private float sliceAngleOffset;
        
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI wheelNameTMP;
        [SerializeField] private TextMeshProUGUI wheelDescriptionTMP;
        [SerializeField] private Image wheelImage;
        [SerializeField] private Image wheelIndicatorImage;
        [SerializeField] private RectTransform wheelRoot;

        public RectTransform wheelImageRT;
        
        public CustomCircularLayout CircularLayout{get; private set;}
        
        private void Awake()
        {
            animator = GetComponentInChildren<WheelAnimator>();
            CircularLayout = GetComponentInChildren<CustomCircularLayout>();
            wheelImageRT = wheelImage.GetComponent<RectTransform>();
        }

        public float SpinTo(int sliceIndex, float duration, System.Action onComplete)
        {
            float per = 360f / Mathf.Max(1, sliceCount);
            var rotationValue = (360 - sliceIndex * per);
            float target = (360f * 6) + rotationValue + sliceAngleOffset;
            animator.PlaySpin(target, duration, onComplete);
            return rotationValue;
        }

        public void UpdateConfig(WheelConfig newConfig,bool animate = false)
        {
            if (animate)
            {
                var animationDuration = 0.5f;
                var startDelay = 0.5f;
                wheelRoot.DOScale(0f, animationDuration).SetEase(Ease.InBack).SetDelay(startDelay).OnComplete(ExecuteUpdate);
                wheelRoot.DOScale(1f, animationDuration).SetEase(Ease.OutBack).SetDelay(animationDuration + startDelay);
            }
            else
            {
                ExecuteUpdate();
            }

            return;

            void ExecuteUpdate()
            {
                wheelNameTMP.text = newConfig.wheelName;
                wheelNameTMP.DOColor(newConfig.themeColor,0.5f);
                wheelDescriptionTMP.text = newConfig.wheelInfo;
                wheelDescriptionTMP.DOColor(newConfig.themeColor, 0.5f);
                wheelImage.sprite = newConfig.wheelSprite;
                wheelIndicatorImage.sprite = newConfig.wheelIndicatorSprite;
                CircularLayout.Rebind(newConfig);
            }
            
        }
    }

    public interface IWheelAnimator { void PlaySpin(float targetAngle, float duration, System.Action onComplete); }
}
