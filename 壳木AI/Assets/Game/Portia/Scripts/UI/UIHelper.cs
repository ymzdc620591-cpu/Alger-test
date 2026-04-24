using UnityEngine;
using UnityEngine.UI;

namespace Game.Portia
{
    public static class UIHelper
    {
        static Font _font;

        public static Font Font
        {
            get
            {
                if (_font == null)
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                         ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                return _font;
            }
        }

        public static RectTransform MakeRect(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go      = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r       = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = r.offsetMax = Vector2.zero;
            return r;
        }

        public static Image MakeImage(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax)
            => MakeRect(parent, name, anchorMin, anchorMax).gameObject.AddComponent<Image>();

        public static Text MakeText(Transform parent, string name, string text, int size,
            Vector2 anchorMin, Vector2 anchorMax, Color? color = null)
        {
            var r = MakeRect(parent, name, anchorMin, anchorMax);
            return ApplyText(r.gameObject, text, size, color);
        }

        public static Text ApplyText(GameObject go, string text, int size, Color? color = null)
        {
            var t       = go.AddComponent<Text>();
            t.text      = text;
            t.fontSize  = size;
            t.color     = color ?? Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.font      = Font;
            return t;
        }
    }
}
