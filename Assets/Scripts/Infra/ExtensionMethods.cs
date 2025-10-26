using DG.Tweening;
using UnityEngine;

namespace Infra
{
    public static class ExtensionMethods
    {
        public static Tweener SetState(this CanvasGroup canvasGroup, bool state, float fadeDuration = 0.25f)
        {
            return canvasGroup.DOFade(state ? 1f : 0f, fadeDuration).OnComplete(() =>
            {
                canvasGroup.interactable = state;
                canvasGroup.blocksRaycasts = state;
            });
        }
    }
}