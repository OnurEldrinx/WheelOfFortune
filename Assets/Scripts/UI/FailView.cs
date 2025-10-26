using Infra;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FailView:MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private AutoRotation autoRotation;
        
        [Header("UI Elements")]
        public Button giveUpButton;
        
        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            autoRotation = GetComponentInChildren<AutoRotation>();
        }

        public void SetVisibility(bool state)
        {
            canvasGroup.SetState(state);
            if (autoRotation)
            {
                autoRotation.stop = !state;
            }
        }
    }
}