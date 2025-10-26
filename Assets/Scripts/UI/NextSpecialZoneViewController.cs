using Core.Game;
using TMPro;
using UnityEngine;

namespace UI
{
    public sealed class NextSpecialZoneViewController : MonoBehaviour
    {
        [Header("Binding")]
        [SerializeField] private ZoneService zones;

        [Header("Safe Banner")]
        [SerializeField] private TMP_Text safeZoneValue;
        // [SerializeField] private Image safeIcon;        // optional

        [Header("Super Banner")]
        [SerializeField] private TMP_Text superZoneValue;
        // [SerializeField] private Image superIcon;       // optional

        [Header("Options")]
        [SerializeField] private bool strictlyGreater = true;

        private void OnEnable()
        {
            if (!zones) zones = FindObjectOfType<ZoneService>(true);
            if (zones) zones.ZoneChanged += HandleZoneChanged;
            Refresh();
        }

        private void OnDisable()
        {
            if (zones) zones.ZoneChanged -= HandleZoneChanged;
        }

        private void HandleZoneChanged(int _)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (!zones) return;

            int nextSafe  = zones.GetNextSafeZone(zones.CurrentZone,  strictlyGreater);
            int nextSuper = zones.GetNextSuperZone(zones.CurrentZone, strictlyGreater);

            if (safeZoneValue)  safeZoneValue.text  = nextSafe.ToString();
            if (superZoneValue) superZoneValue.text = nextSuper.ToString();
        }
    }
}