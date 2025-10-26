using System.Collections.Generic;
using Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class HUDView : MonoBehaviour
    {
        [Header("Texts (TMP)")] 
        [SerializeField] public TMP_Text zoneValueTMP;

        [Header("Buttons")] 
        [SerializeField] public Button spinButton;
        [SerializeField] public Button exitButton;

        [Header("Panels")] 
        [SerializeField] public InventoryView inventoryView;
        [SerializeField] public FailView failView;
        [SerializeField] public WheelView wheelView;
        [SerializeField] public CollectedRewardsShowcaseView collectedRewardsShowcaseView;
        
        public void SetZoneValue(int value)
        {
            if (zoneValueTMP) zoneValueTMP.text = value.ToString();
        }

        public void SetButtonsEnabled(bool on)
        {
            if (spinButton) spinButton.interactable = on;
            if (exitButton) exitButton.interactable = on;
        }

        public void SetCashOutVisible(bool on)
        {
            if (exitButton) exitButton.interactable = on; //exitButton.gameObject.SetActive(on);
        }

        public void SetInventory(List<RewardInventory.Entry> entries)
        {
            if (inventoryView) inventoryView.Render(entries);
        }

        public void SetFailViewVisibility(bool state)
        {
            failView.SetVisibility(state);
        }
        
    }
}