using Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CollectedRewardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI rewardTitleTMP;
        [SerializeField] private TextMeshProUGUI rewardAmountTMP;
        [SerializeField] private Image rewardIcon;

        public void Init(RewardInventory.Entry entry)
        {
            rewardTitleTMP.text = entry.Id;
            rewardAmountTMP.text = $"x{entry.Count}";
            rewardIcon.sprite = entry.Icon;
        }
        
    }
}
