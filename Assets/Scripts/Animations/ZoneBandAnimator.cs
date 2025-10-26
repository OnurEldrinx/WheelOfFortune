using System.Collections.Generic;
using Core.Game;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Animations
{
    public sealed class ZoneBandAnimator : MonoBehaviour
    {
        [Header("Binding")] 
        [SerializeField] ZoneService zones;

        [Header("Layout")] 
        [SerializeField] RectTransform content;
        [SerializeField] ZoneCellView cellPrefab;
        [SerializeField] int windowSize = 11;//should be odd 
        [SerializeField] float cellWidth = 64f;
        [SerializeField] float spacing = 8f;
        [SerializeField] Image currentZoneBackground;

        [Header("Colors")]
        [SerializeField] Color normalZoneBackgroundColor;
        [SerializeField] Color safeZoneBackgroundColor;
        [SerializeField] Color superZoneBackgroundColor;
        
        [Header("Tween")] 
        [SerializeField] float shiftDuration = 0.25f;
        [SerializeField] Ease ease = Ease.OutQuart;

        readonly List<ZoneCellView> pool = new();
        int firstZoneShown;
        int currentZoneVisual;
        int CenterIndex => windowSize / 2;

        private void OnEnable()
        {
            if (windowSize % 2 == 0) windowSize += 1;

            BuildWindow();

            if (zones) zones.ZoneChanged += HandleZoneChanged;
        }

        private void OnDisable()
        {
            if (zones) zones.ZoneChanged -= HandleZoneChanged;
        }

        private void Start()
        {
            int startZ = zones ? zones.CurrentZone : 1;
            SetWindowAround(startZ);
        }

        private void BuildWindow()
        {
            // init
            for (int i = content.childCount - 1; i >= 0; i--) DestroyImmediate(content.GetChild(i).gameObject);
            pool.Clear();

            float total = windowSize * cellWidth + (windowSize - 1) * spacing;
            content.sizeDelta = new Vector2(total, content.sizeDelta.y);

            for (int i = 0; i < windowSize; i++)
            {
                var cell = Instantiate(cellPrefab, content);
                var rt = (RectTransform)cell.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
                rt.pivot = new Vector2(0, 0.5f);
                rt.anchoredPosition = new Vector2(i * (cellWidth + spacing), 0);
                pool.Add(cell);
            }

            content.pivot = new Vector2(0, 0.5f);
            content.anchoredPosition = new Vector2(-cellWidth/2,0f);
        }

        private void SetWindowAround(int zone)
        {
            currentZoneVisual = zone;
            firstZoneShown = Mathf.Max(1, zone - CenterIndex);

            for (int i = 0; i < windowSize; i++)
            {
                int z = firstZoneShown + i;
                pool[i].Bind(z, isCurrent: z == zone);
            }
        }

        public void Reset()
        {
            BuildWindow();
            SetWindowAround(zones.CurrentZone);
        }
        
        private void HandleZoneChanged(int newZone)
        {
            int delta = newZone - currentZoneVisual;

            if (delta == 1)
            {
                // shift left by one cell
                ShiftLeftOne();
            }
            else
            {
                // reset
                BuildWindow();
                SetWindowAround(newZone);
            }
        }

        private void ShiftLeftOne()
        {
            currentZoneVisual++;

            if (zones.IsSafeZone(zones.CurrentZone))
            {
                currentZoneBackground.DOColor(safeZoneBackgroundColor, shiftDuration);
            }
            else if (zones.IsSuperZone(zones.CurrentZone))
            {
                currentZoneBackground.DOColor(superZoneBackgroundColor, shiftDuration);
            }
            else
            {
                currentZoneBackground.DOColor(normalZoneBackgroundColor, shiftDuration);
            }
            
            float dx = -(cellWidth + spacing);

            content.DOKill();
            content.DOAnchorPosX(content.anchoredPosition.x + dx, shiftDuration)
                .SetEase(ease)
                .OnComplete(RecycleLeftToRight);
        }

        private void RecycleLeftToRight()
        {
            // move first visual to end
            var first = pool[0];

            if (content.anchoredPosition.x >= -450f)
            {
                return;
            }
            
            pool.RemoveAt(0);
            pool.Add(first);

            // position at the end
            var rt = (RectTransform)first.transform;
            float xEnd = pool[^2].RectTransform.anchoredPosition.x + cellWidth;
            rt.anchoredPosition = new Vector2(xEnd, 0);

            // window logically moved by one
            firstZoneShown += 1;

            // update only the moved cell
            int newZoneAtEnd = firstZoneShown + windowSize - 1;
            first.Bind(newZoneAtEnd, isCurrent: newZoneAtEnd == currentZoneVisual);
        }
    }
}