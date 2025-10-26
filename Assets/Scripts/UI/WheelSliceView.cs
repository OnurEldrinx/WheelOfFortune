using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class WheelSliceView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Image icon;         
        [SerializeField] private TMP_Text amountText;  
        
        public SliceDef SliceDef { get; private set; }
        
        public void Bind(SliceDef def)
        {
            SliceDef = def;
            
            if (icon)  icon.sprite = def.icon;
            
            if (amountText)
            {
                switch (def.type)
                {
                    case SliceType.Reward:
                        amountText.text = $"x{def.value}";
                        amountText.gameObject.SetActive(true);
                        break;

                    case SliceType.Multiplier:
                        amountText.text = $"x{Mathf.Max(1, def.value)}";
                        amountText.gameObject.SetActive(true);
                        break;

                    case SliceType.Bomb:
                        amountText.text = "";
                        amountText.gameObject.SetActive(false);
                        break;
                }
            }
        }
    }
}