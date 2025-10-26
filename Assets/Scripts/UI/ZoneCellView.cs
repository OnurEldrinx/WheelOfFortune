using TMPro;
using UnityEngine;

namespace UI
{
    public sealed class ZoneCellView : MonoBehaviour
    {
        [SerializeField] TMP_Text labelTMP;
        
        [Header("Colors")]
        [SerializeField] Color normal = Color.white;
        [SerializeField] Color current = new Color(1f,1f,1f,1f);
        [SerializeField] Color safe = new Color(0.3f, 1f, 0.3f, 1f);
        [SerializeField] Color super = new Color(1f, 0.84f, 0f, 1f);

        int zoneIndex;
        public int ZoneIndex => zoneIndex;

        public void Bind(int zone, bool isCurrent)
        {
            zoneIndex = Mathf.Max(1, zone);
            if (labelTMP) labelTMP.text = zoneIndex.ToString();

            // style
            var col = normal;
            if (zoneIndex % 30 == 0) col = super;           
            else if (zoneIndex % 5 == 0) col = safe;       

            if (labelTMP) labelTMP.color = col;
        }
        
        public RectTransform RectTransform => transform as RectTransform;
        
    }
}