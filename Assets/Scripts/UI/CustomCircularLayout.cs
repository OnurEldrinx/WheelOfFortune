using System.Collections.Generic;
using Data;
using UnityEngine;

namespace UI
{
    public sealed class CustomCircularLayout : MonoBehaviour
    {
        [Header("Data")] 
        [SerializeField] private WheelConfig wheelConfig;

        [Header("Prefabs/Parents")] 
        [SerializeField] private RectTransform container;
        [SerializeField] private GameObject uiWheelSlicePrefab;

        [Header("Optional: exact hole anchors (preferred)")]
        [Tooltip("If provided and count == slices, each slice will be parented to the matching anchor.")]
        [SerializeField] private List<RectTransform> holeAnchors = new();

        [Header("Parametric radial (used if holeAnchors mismatch)")] 
        [SerializeField] private float radius = 180f;

        [Tooltip("0° = right. Use 90° if your pointer is at top.")] 
        [SerializeField] private float startAngleDeg = 90f;

        [Tooltip("Clockwise placement if true.")] 
        [SerializeField] private bool clockwise = true;

        [Tooltip("Rotate slice to face outward (e.g., label upright toward rim).")] 
        [SerializeField] private bool faceOutward = true;


        [ContextMenu("Rebuild Now")]
        public void Rebuild()
        {
            if (!wheelConfig || !uiWheelSlicePrefab || !container)
            {
                Debug.LogWarning("RadialWheelBuilder: assign config/prefab/container.");
                return;
            }

            Clear();

            int n = Mathf.Max(0, wheelConfig.slices?.Count ?? 0);
            if (n == 0) return;

            bool useAnchors = holeAnchors != null && holeAnchors.Count == n;

            for (int i = 0; i < n; i++)
            {
                var def = wheelConfig.slices![i];
                GameObject go;
                RectTransform rt;

                if (useAnchors)
                {
                    // Instantiate under exact hole anchor
                    var anchor = holeAnchors[i];
                    go = Instantiate(uiWheelSlicePrefab, anchor);
                    rt = go.transform as RectTransform;
                    rt!.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = Vector2.zero;
                    rt.localRotation = Quaternion.identity; // keep prefab rotation unless you want to face outward
                }
                else
                {
                    // Parametric placement
                    go = Instantiate(uiWheelSlicePrefab, container);
                    rt = go.transform as RectTransform;

                    float step = 360f / n;
                    float signed = clockwise ? -1f : 1f;
                    float angle = startAngleDeg + signed * step * i;
                    float rad = angle * Mathf.Deg2Rad;

                    rt!.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

                    if (faceOutward)
                        rt.localRotation = Quaternion.Euler(0, 0, angle - 90f); // label points outward
                }

                // Bind data to the prefab’s view
                var view = go.GetComponent<WheelSliceView>();
                if (!view) view = go.AddComponent<WheelSliceView>();
                view.Bind(def);
            }
        }

        private void Clear()
        {
            if (!container) return;

            // If using anchors, spawn under anchors; clear both places
            if (holeAnchors is { Count: > 0 })
                foreach (var a in holeAnchors)
                    if (a)
                        for (int i = a.childCount - 1; i >= 0; i--)
                            DestroyImmediate(a.GetChild(i).gameObject);

            for (int i = container.childCount - 1; i >= 0; i--)
                DestroyImmediate(container.GetChild(i).gameObject);
        }

        // Call this after you edit the ScriptableObject from code at runtime
        public void Rebind(WheelConfig newConfig)
        {
            wheelConfig = newConfig;
            Rebuild();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying && wheelConfig && uiWheelSlicePrefab && container)
            {
                //Rebuild();
            }
        }
#endif
    }
}