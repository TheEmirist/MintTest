using UnityEngine;

namespace MintTest.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        RectTransform panel;
        Rect lastSafeArea;
        Vector2Int lastScreen;

        void Awake()
        {
            panel = GetComponent<RectTransform>();
            Apply();
        }

        void Update()
        {
            if (lastSafeArea != Screen.safeArea
                || lastScreen.x != Screen.width
                || lastScreen.y != Screen.height)
            {
                Apply();
            }
        }

        void Apply()
        {
            Rect safe = Screen.safeArea;
            if (Screen.width <= 0 || Screen.height <= 0)
                return;

            lastSafeArea = safe;
            lastScreen = new Vector2Int(Screen.width, Screen.height);

            RectTransform parent = panel.parent as RectTransform;
            if (parent == null)
            {
                ApplyFullScreen(safe);
                return;
            }

            Canvas canvas = panel.GetComponentInParent<Canvas>();
            Camera camera = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                camera = canvas.worldCamera;

            Vector2 localMin;
            Vector2 localMax;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                new Vector2(safe.xMin, safe.yMin),
                camera,
                out localMin);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent,
                new Vector2(safe.xMax, safe.yMax),
                camera,
                out localMax);

            Rect area = parent.rect;
            if (area.width <= 0f || area.height <= 0f)
                return;

            Vector2 anchorMin = new Vector2(
                (localMin.x - area.xMin) / area.width,
                (localMin.y - area.yMin) / area.height);
            Vector2 anchorMax = new Vector2(
                (localMax.x - area.xMin) / area.width,
                (localMax.y - area.yMin) / area.height);

            // Safe area больше родителя (типично для хедера) — оставляем только пересечение.
            anchorMin = Vector2.Max(anchorMin, Vector2.zero);
            anchorMax = Vector2.Min(anchorMax, Vector2.one);
            if (anchorMin.x > anchorMax.x)
                anchorMin.x = anchorMax.x = (anchorMin.x + anchorMax.x) * 0.5f;
            if (anchorMin.y > anchorMax.y)
                anchorMin.y = anchorMax.y = (anchorMin.y + anchorMax.y) * 0.5f;

            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
        }

        void ApplyFullScreen(Rect safe)
        {
            Vector2 anchorMin = safe.position;
            Vector2 anchorMax = safe.position + safe.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
        }
    }
}
