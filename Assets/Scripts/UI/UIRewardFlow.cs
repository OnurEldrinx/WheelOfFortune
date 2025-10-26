using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIRewardFlow : MonoBehaviour
    {
        [Header("Pool & Visuals")] [SerializeField]
        private Image iconPrefab; // Assign your UIRewardFlowIcon prefab (Image)

        [SerializeField] private int initialPoolSize = 32;
        [SerializeField] private bool expandPool = true;
        [SerializeField] private bool hideOffscreenWhileFlying = true;

        [Header("Defaults")]
        [SerializeField] private float defaultDuration = 0.75f;
        [SerializeField] private float defaultStagger = 0.03f; // delay between coins
        [SerializeField] private float defaultSpread = 80f; // initial radial noise
        [SerializeField] private float defaultArcHeight = 120f; // arc peak offset

        [SerializeField] 
        private AnimationCurve defaultCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Cameras (optional)")]
        [Tooltip("If your UI Canvas is Screen Space - Camera, set its camera here. If null, will try to auto-detect.")]
        [SerializeField]
        private Camera uiCamera;

        private static UIRewardFlow _instance;
        private readonly Queue<Image> pool = new();
        private Canvas canvas;
        private RectTransform canvasRect;

        // ---------- Public Static API ----------
        public static void PlayFromWorld(
            Vector3 worldPos,
            Transform optionalIconLookAt, // can be null
            Camera worldCamera,
            RectTransform target,
            Sprite sprite,
            int count = 10,
            float duration = -1f,
            float arcHeight = -1f,
            float spread = -1f,
            float stagger = -1f,
            AnimationCurve curve = null,
            float startScale = 0.7f,
            float endScale = 1.0f,
            float startAlpha = 1.0f,
            float endAlpha = 1.0f)
        {
            var inst = EnsureInstance();
            inst.InternalPlay(
                SourceType.World, worldPos, null, optionalIconLookAt, worldCamera,
                target, sprite, count, duration, arcHeight, spread, stagger, curve,
                startScale, endScale, startAlpha, endAlpha);
        }

        public static void PlayFromUI(
            RectTransform sourceUI,
            RectTransform target,
            Sprite sprite,
            int count = 10,
            float duration = -1f,
            float arcHeight = -1f,
            float spread = -1f,
            float stagger = -1f,
            AnimationCurve curve = null,
            float startScale = 0.7f,
            float endScale = 1.0f,
            float startAlpha = 1.0f,
            float endAlpha = 1.0f)
        {
            var inst = EnsureInstance();
            inst.InternalPlay(
                SourceType.UIRect, Vector3.zero, sourceUI, null, null,
                target, sprite, count, duration, arcHeight, spread, stagger, curve,
                startScale, endScale, startAlpha, endAlpha);
        }

        public static void PlayFromScreen(
            Vector2 screenPoint,
            RectTransform target,
            Sprite sprite,
            int count = 10,
            float duration = -1f,
            float arcHeight = -1f,
            float spread = -1f,
            float stagger = -1f,
            AnimationCurve curve = null,
            float startScale = 0.7f,
            float endScale = 1.0f,
            float startAlpha = 1.0f,
            float endAlpha = 1.0f)
        {
            var inst = EnsureInstance();
            inst.cachedScreenPoint = screenPoint;
            inst.InternalPlay(
                SourceType.ScreenPoint, Vector3.zero, null, null, null,
                target, sprite, count, duration, arcHeight, spread, stagger, curve,
                startScale, endScale, startAlpha, endAlpha);
        }

        // ---------- Implementation ----------
        private enum SourceType
        {
            World,
            UIRect,
            ScreenPoint
        }

        private Vector2 cachedScreenPoint;

        private static UIRewardFlow EnsureInstance()
        {
            if (_instance != null) return _instance;

            // Find or create a host under the first active Canvas
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var goCanvas = new GameObject("AutoCanvas (UIRewardFlow)");
                canvas = goCanvas.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                goCanvas.AddComponent<CanvasScaler>();
                goCanvas.AddComponent<GraphicRaycaster>();
            }

            var go = new GameObject("UIRewardFlow");
            go.transform.SetParent(canvas.transform, false);
            _instance = go.AddComponent<UIRewardFlow>();
            _instance.Initialize(canvas);
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (canvas == null)
            {
                var c = GetComponentInParent<Canvas>();
                if (c == null) c = FindObjectOfType<Canvas>();
                Initialize(c);
            }
        }

        private void Initialize(Canvas c)
        {
            this.canvas = c;
            canvasRect = this.canvas.transform as RectTransform;

            if (uiCamera == null && this.canvas.renderMode == RenderMode.ScreenSpaceCamera)
                uiCamera = this.canvas.worldCamera;

            // If prefab not assigned in inspector, try to create a simple one
            if (iconPrefab == null)
            {
                var go = new GameObject("UIRewardFlowIcon (Runtime)", typeof(RectTransform), typeof(Image));
                var img = go.GetComponent<Image>();
                img.raycastTarget = false;
                var rt = (RectTransform)go.transform;
                rt.sizeDelta = new Vector2(64, 64);
                iconPrefab = img;
                go.SetActive(false);
                go.transform.SetParent(transform, false);
            }

            // Prewarm pool
            for (int i = 0; i < Mathf.Max(1, initialPoolSize); i++)
                pool.Enqueue(CreateIcon());
        }

        private Image CreateIcon()
        {
            var img = Instantiate(iconPrefab, transform);
            img.gameObject.SetActive(false);
            img.raycastTarget = false;
            return img;
        }

        private Image GetIcon()
        {
            if (pool.Count > 0) return pool.Dequeue();
            if (expandPool) return CreateIcon();
            return null;
        }

        private void ReturnIcon(Image img)
        {
            if (img == null) return;
            img.gameObject.SetActive(false);
            pool.Enqueue(img);
        }

        private void InternalPlay(
            SourceType sourceType,
            Vector3 worldPos,
            RectTransform sourceUI,
            Transform optionalLookAt,
            Camera worldCamera,
            RectTransform target,
            Sprite sprite,
            int count,
            float duration,
            float arcHeight,
            float spread,
            float stagger,
            AnimationCurve curve,
            float startScale,
            float endScale,
            float startAlpha,
            float endAlpha)
        {
            if (target == null || count <= 0) return;

            duration = duration > 0 ? duration : defaultDuration;
            arcHeight = arcHeight > 0 ? arcHeight : defaultArcHeight;
            spread = spread >= 0 ? spread : defaultSpread;
            stagger = stagger >= 0 ? stagger : defaultStagger;
            curve = curve ?? defaultCurve;

            // Resolve source anchored position per icon with a bit of radial noise
            for (int i = 0; i < count; i++)
            {
                var img = GetIcon();
                if (img == null) break;

                img.sprite = sprite;
                //img.SetNativeSize();
                img.preserveAspect = true;
                var rt = (RectTransform)img.transform;
                rt.sizeDelta = target.sizeDelta;
                
                rt.SetParent(transform, false);

                // spawn pos
                Vector2 anchoredStart = sourceType switch
                {
                    SourceType.World => WorldToAnchored(worldPos, worldCamera),
                    SourceType.UIRect => RectCenterAnchored(sourceUI),
                    SourceType.ScreenPoint => ScreenToAnchored(cachedScreenPoint),
                    _ => Vector2.zero
                };

                // add radial noise
                if (spread > 0f)
                {
                    var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    var radius = Random.Range(spread * 0.25f, spread);
                    anchoredStart += new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                }

                rt.anchoredPosition = anchoredStart;
                rt.localScale = Vector3.one * startScale;
                var color = img.color;
                color.a = startAlpha;
                img.color = color;
                img.gameObject.SetActive(true);

                // compute target pos (center of target)
                Vector2 anchoredEnd = RectCenterAnchored(target);

                // choose control point for arc
                Vector2 mid = Vector2.Lerp(anchoredStart, anchoredEnd, 0.5f);
                // perpendicular offset for the arc
                Vector2 dir = (anchoredEnd - anchoredStart);
                Vector2 perp = new Vector2(-dir.y, dir.x).normalized;
                float side = Random.value < 0.5f ? -1f : 1f;
                Vector2 control = mid + perp * arcHeight * side;

                // optional billboard look (only affects rotation visuals if you swap with 3D)
                if (optionalLookAt != null)
                    rt.up = anchoredEnd - anchoredStart;

                float startScaleTweenDuration = 0.25f;
                float delay = i * stagger + startScaleTweenDuration;

                // DOTween path (FastBeyond360 not needed here)
                rt.DOKill();
                rt.transform.DOScale(Vector3.one * endScale, startScaleTweenDuration).SetEase(Ease.OutBack);
                
                // Position along quadratic Bezier via tween value t ∈ [0,1]
                DOVirtual.Float(0f, 1f, duration, (t) =>
                    {
                        float c = curve.Evaluate(t);
                        Vector2 p = Bezier(anchoredStart, control, anchoredEnd, c);
                        rt.anchoredPosition = p;

                        // scale / alpha over time
                        //float sc = Mathf.Lerp(startScale, endScale, c);
                        //rt.localScale = new Vector3(sc, sc, 1f);
                        var col = img.color;
                        col.a = Mathf.Lerp(startAlpha, endAlpha, c);
                        img.color = col;

                        if (hideOffscreenWhileFlying)
                            img.enabled = IsVisibleInCanvas(rt.anchoredPosition);
                    })
                    .SetDelay(delay)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        // optional little pop on arrival
                        DOVirtual.Float(0f, 1f, 0.08f, (t) =>
                        {
                            float pop = 1f + 0.15f * Mathf.Sin(t * Mathf.PI);
                            rt.localScale = new Vector3(endScale * pop, endScale * pop, 1f);
                        }).OnComplete(() => ReturnIcon(img));
                    });
            }
        }

        // ---------- Math & Utils ----------
        private Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            // Quadratic Bezier
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        private Vector2 RectCenterAnchored(RectTransform rt)
        {
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, rt.position);
            return ScreenToAnchored(screen);
        }

        private Vector2 WorldToAnchored(Vector3 worldPos, Camera worldCamera)
        {
            if (worldCamera == null) worldCamera = Camera.main;
            var screen = worldCamera != null
                ? worldCamera.WorldToScreenPoint(worldPos)
                : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
            return ScreenToAnchored(screen);
        }

        private Vector2 ScreenToAnchored(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screen, uiCamera, out var local);
            return local;
        }

        private bool IsVisibleInCanvas(Vector2 anchoredPos)
        {
            // Simple bounds check (no clipping masks considered)
            var rect = canvasRect.rect;
            return rect.Contains(anchoredPos);
        }
    }
}