using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Portia
{
    public class BuildingSelectPanel : MonoBehaviour
    {
        Canvas         _canvas;
        MachineEntry[] _entries;
        Action<int>    _callback;
        Font           _font;

        public void Init(MachineEntry[] entries, Action<int> callback)
        {
            _entries  = entries;
            _callback = callback;
            _font     = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                     ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            BuildUI();
        }

        public void Show() => _canvas.enabled = true;
        public void Hide() => _canvas.enabled = false;

        // ── UI Construction ────────────────────────────────────────────────────

        void BuildUI()
        {
            _canvas              = gameObject.AddComponent<Canvas>();
            _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 80;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            // 半透明遮罩
            MakeImage(transform, "Overlay", Vector2.zero, Vector2.one)
                .color = new Color(0f, 0f, 0f, 0.55f);

            // 中心面板
            var panel = MakeImage(transform, "Panel",
                new Vector2(0.3f, 0.25f), new Vector2(0.7f, 0.75f));
            panel.color = new Color(0.08f, 0.09f, 0.13f, 0.97f);
            var pt = panel.transform;

            // 标题
            MakeText(pt, "Title", "选择建造机器", 24,
                new Vector2(0f, 0.83f), Vector2.one);

            // 分割线
            MakeImage(pt, "Divider", new Vector2(0.04f, 0.825f), new Vector2(0.96f, 0.83f))
                .color = new Color(1f, 1f, 1f, 0.18f);

            // 机器按钮（横向排列）
            int   count = _entries != null ? _entries.Length : 0;
            float step  = count > 0 ? 0.9f / count : 0.9f;
            for (int i = 0; i < count; i++)
            {
                int   idx  = i;
                float xMin = 0.05f + i * step;
                float xMax = xMin + step - 0.02f;
                BuildMachineButton(pt, idx, xMin, xMax);
            }

            // 底部提示
            MakeText(pt, "Hint", "单击选择  |  ESC 取消", 13,
                Vector2.zero, new Vector2(1f, 0.14f),
                new Color(1f, 1f, 1f, 0.35f));
        }

        void BuildMachineButton(Transform parent, int idx, float xMin, float xMax)
        {
            var btnR = MakeRect(parent, $"Btn_{idx}",
                new Vector2(xMin, 0.2f), new Vector2(xMax, 0.8f));
            var img  = btnR.gameObject.AddComponent<Image>();
            img.color = new Color(0.18f, 0.23f, 0.35f, 0.95f);

            var btn = btnR.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var c = btn.colors;
            c.highlightedColor = new Color(0.35f, 0.5f, 0.75f);
            c.pressedColor     = new Color(0.25f, 0.38f, 0.6f);
            btn.colors = c;

            btn.onClick.AddListener(() => _callback?.Invoke(idx));

            // 机器名
            MakeText(btnR, "Name", _entries[idx].displayName, 20,
                new Vector2(0f, 0.5f), Vector2.one);

            // 说明文字
            if (!string.IsNullOrEmpty(_entries[idx].description))
                MakeText(btnR, "Desc", _entries[idx].description, 13,
                    Vector2.zero, new Vector2(1f, 0.5f),
                    new Color(1f, 1f, 1f, 0.5f));
        }

        // ── UI Helpers ─────────────────────────────────────────────────────────

        static RectTransform MakeRect(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = r.offsetMax = Vector2.zero;
            return r;
        }

        static Image MakeImage(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax)
            => MakeRect(parent, name, anchorMin, anchorMax).gameObject.AddComponent<Image>();

        Text MakeText(Transform parent, string name, string text, int size,
            Vector2 anchorMin, Vector2 anchorMax, Color? color = null)
        {
            var r = MakeRect(parent, name, anchorMin, anchorMax);
            var t = r.gameObject.AddComponent<Text>();
            t.text      = text;
            t.fontSize  = size;
            t.color     = color ?? Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            if (_font != null) t.font = _font;
            return t;
        }
    }
}
