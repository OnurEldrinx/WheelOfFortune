using System.Collections.Generic;
using Infra;
using Systems;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CollectedRewardsShowcaseView : MonoBehaviour
    {
        [SerializeField] private CollectedRewardView collectedRewardViewPrefab;
        [SerializeField] private RectTransform scrollContent;
        [SerializeField] private Button continueButton;
        
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void OnDisable()
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        private void OnContinueClicked()
        {
            ResetView();
            SetVisibility(false);
        }

        private void ResetView()
        {
            for (int i = 0; i < scrollContent.transform.childCount; i++)
            {
                Destroy(scrollContent.GetChild(i).gameObject);
            }
        }
        
        private void SetVisibility(bool visible)
        {
            canvasGroup.SetState(visible);
        }

        public void BuildShowcaseView(List<RewardInventory.Entry> inventory)
        {
            SetVisibility(true);
            
            foreach (var entry in inventory)
            {
                var collectedRewardView = Instantiate(collectedRewardViewPrefab, scrollContent);
                collectedRewardView.Init(entry);
            }
        }
        
    }
}
