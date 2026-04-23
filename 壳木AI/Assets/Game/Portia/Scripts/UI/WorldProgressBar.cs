using UnityEngine;
using UnityEngine.UI;

namespace Game.Portia
{
    public class WorldProgressBar : MonoBehaviour
    {
        RectTransform _fill;
        Camera        _cam;

        public static WorldProgressBar Attach(Transform target, Color fillColor, float yOffset = 3f)
        {
            var go = new GameObject("[ProgressBar]");
            go.transform.SetParent(target, false);
            go.transform.localPosition = Vector3.up * yOffset;
            var bar = go.AddComponent<WorldProgressBar>();
            bar.Build(fillColor);
            return bar;
        }

        void Awake() => _cam = Camera.main;

        void LateUpdate()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam != null) transform.rotation = _cam.transform.rotation;
        }

        public void SetFill(float t)
        {
            if (_fill == null) return;
            _fill.anchorMax = new Vector2(Mathf.Clamp01(t), _fill.anchorMax.y);
        }

        void Build(Color fillColor)
        {
            var canvas        = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var rt        = (RectTransform)transform;
            rt.sizeDelta  = new Vector2(120f, 14f);
            rt.localScale = Vector3.one * 0.025f;

            var bg       = new GameObject("BG").AddComponent<Image>();
            bg.transform.SetParent(transform, false);
            var bgR      = (RectTransform)bg.transform;
            bgR.anchorMin = Vector2.zero;
            bgR.anchorMax = Vector2.one;
            bgR.offsetMin = bgR.offsetMax = Vector2.zero;
            bg.color = new Color(0.12f, 0.12f, 0.12f, 0.88f);

            var fill   = new GameObject("Fill").AddComponent<Image>();
            fill.transform.SetParent(transform, false);
            _fill      = (RectTransform)fill.transform;
            _fill.anchorMin = new Vector2(0f, 0.12f);
            _fill.anchorMax = new Vector2(0f, 0.88f);
            _fill.offsetMin = _fill.offsetMax = Vector2.zero;
            fill.color = fillColor;
        }
    }
}
