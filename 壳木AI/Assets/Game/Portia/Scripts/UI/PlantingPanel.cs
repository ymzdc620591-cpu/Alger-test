using UnityEngine;
using UnityEngine.UI;

namespace Game.Portia
{
    public class PlantingPanel : MonoBehaviour
    {
        static PlantingPanel _current;

        PlantingBox _box;
        Font        _font;

        // ── 静态入口 ───────────────────────────────────────────────────────────

        public static void Show(PlantingBox box, CropData[] crops)
        {
            if (_current != null) { Destroy(_current.gameObject); _current = null; }

            var go    = new GameObject("PlantingPanel");
            _current  = go.AddComponent<PlantingPanel>();
            _current._box = box;
            _current.Build(crops);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        // ── 构建 UI ────────────────────────────────────────────────────────────

        void Build(CropData[] crops)
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                 ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            var canvas          = gameObject.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            MakeImage(transform, "Overlay", Vector2.zero, Vector2.one)
                .color = new Color(0f, 0f, 0f, 0.6f);

            var panel = MakeImage(transform, "Panel",
                new Vector2(0.18f, 0.12f), new Vector2(0.82f, 0.92f));
            panel.color = new Color(0.06f, 0.1f, 0.07f, 0.97f);
            var pt = panel.transform;

            MakeText(pt, "Title", "选择种植作物", 24,
                new Vector2(0f, 0.91f), Vector2.one);
            MakeImage(pt, "Divider", new Vector2(0.04f, 0.9f), new Vector2(0.96f, 0.905f))
                .color = new Color(1f, 1f, 1f, 0.18f);

            // 作物网格（每行 3 个）
            var gridGo = new GameObject("Grid");
            gridGo.transform.SetParent(pt, false);
            var gridRect = gridGo.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.02f, 0.1f);
            gridRect.anchorMax = new Vector2(0.98f, 0.88f);
            gridRect.offsetMin = gridRect.offsetMax = Vector2.zero;
            gridGo.AddComponent<Image>().color = Color.clear;

            var glg             = gridGo.AddComponent<GridLayoutGroup>();
            glg.cellSize        = new Vector2(160f, 115f);
            glg.spacing         = new Vector2(12f, 12f);
            glg.padding         = new RectOffset(8, 8, 8, 8);
            glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;
            glg.childAlignment  = TextAnchor.UpperLeft;

            if (crops != null)
                foreach (var c in crops)
                    if (c != null) AddCropButton(gridGo.transform, c);

            MakeText(pt, "Hint", "[ Esc ] 关闭", 13,
                Vector2.zero, new Vector2(1f, 0.08f),
                new Color(1f, 1f, 1f, 0.35f));
        }

        void AddCropButton(Transform parent, CropData crop)
        {
            var go  = new GameObject($"Btn_{crop.cropName}");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var bg  = go.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.22f, 0.12f, 0.95f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.18f, 0.38f, 0.2f);
            colors.pressedColor     = new Color(0.12f, 0.28f, 0.14f);
            btn.colors = colors;

            // 作物名
            MakeTextInGo(go.transform, "Name", crop.cropName, 19,
                new Vector2(0f, 0.52f), Vector2.one);

            // 生长时间
            MakeTextInGo(go.transform, "Time", $"生长 {crop.growTime:F0}s", 13,
                Vector2.zero, new Vector2(1f, 0.52f),
                new Color(0.85f, 1f, 0.85f, 0.7f));

            var captured = crop;
            btn.onClick.AddListener(() => OnCropSelected(captured));
        }

        void OnCropSelected(CropData crop)
        {
            _box.StartGrowing(crop);
            Close();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        void Close()
        {
            if (_current == this) _current = null;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            Destroy(gameObject);
        }

        // ── UI 工具 ────────────────────────────────────────────────────────────

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
            return ApplyText(r.gameObject, text, size, color);
        }

        Text MakeTextInGo(Transform parent, string name, string text, int size,
            Vector2 anchorMin, Vector2 anchorMax, Color? color = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = r.offsetMax = Vector2.zero;
            return ApplyText(go, text, size, color);
        }

        Text ApplyText(GameObject go, string text, int size, Color? color)
        {
            var t = go.AddComponent<Text>();
            t.text      = text;
            t.fontSize  = size;
            t.color     = color ?? Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            if (_font != null) t.font = _font;
            return t;
        }
    }
}
