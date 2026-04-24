using UnityEngine;
using UnityEngine.UI;

namespace Game.Portia
{
    public class WorldHarvestBadge : MonoBehaviour
    {
        Camera _cam;
        float  _time;
        float  _baseY;

        public static WorldHarvestBadge Attach(Transform target, float yOffset = 2f)
        {
            var go = new GameObject("[HarvestBadge]");
            go.transform.SetParent(target, false);
            go.transform.localPosition = Vector3.up * yOffset;
            var badge  = go.AddComponent<WorldHarvestBadge>();
            badge._baseY = yOffset;
            badge.Build();
            return badge;
        }

        void Awake() => _cam = Camera.main;

        void LateUpdate()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam != null) transform.rotation = _cam.transform.rotation;

            _time += Time.deltaTime * 1.8f;
            var lp = transform.localPosition;
            lp.y = _baseY + Mathf.Sin(_time) * 0.12f;
            transform.localPosition = lp;
        }

        void Build()
        {
            var canvas        = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var rt       = (RectTransform)transform;
            rt.sizeDelta  = new Vector2(100f, 28f);
            rt.localScale = Vector3.one * 0.025f;

            var bg  = new GameObject("BG").AddComponent<Image>();
            bg.transform.SetParent(transform, false);
            var bgR = (RectTransform)bg.transform;
            bgR.anchorMin = Vector2.zero;
            bgR.anchorMax = Vector2.one;
            bgR.offsetMin = bgR.offsetMax = Vector2.zero;
            bg.color = new Color(0.1f, 0.72f, 0.22f, 0.92f);

            var label = new GameObject("Label").AddComponent<Text>();
            label.transform.SetParent(transform, false);
            var lr = (RectTransform)label.transform;
            lr.anchorMin = Vector2.zero;
            lr.anchorMax = Vector2.one;
            lr.offsetMin = lr.offsetMax = Vector2.zero;
            label.text      = "可领取";
            label.fontSize  = 18;
            label.color     = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                    ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null) label.font = font;
        }
    }
}
