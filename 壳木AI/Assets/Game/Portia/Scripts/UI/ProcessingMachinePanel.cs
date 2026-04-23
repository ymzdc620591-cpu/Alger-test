using UnityEngine;
using UnityEngine.UI;

namespace Game.Portia
{
    public class ProcessingMachinePanel : MonoBehaviour
    {
        static ProcessingMachinePanel _current;

        ProcessingMachine _machine;
        Font              _font;

        // ── 静态入口 ───────────────────────────────────────────────────────────

        public static void Show(ProcessingMachine machine, RecipeData[] recipes)
        {
            if (_current != null)
            {
                Destroy(_current.gameObject);
                _current = null;
            }

            var go    = new GameObject("ProcessingMachinePanel");
            _current  = go.AddComponent<ProcessingMachinePanel>();
            _current._machine = machine;
            _current.Build(machine.MachineName, recipes);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        // ── 构建 UI ────────────────────────────────────────────────────────────

        void Build(string title, RecipeData[] recipes)
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                 ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            var canvas            = gameObject.AddComponent<Canvas>();
            canvas.renderMode     = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder   = 100;
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();

            // 半透明遮罩
            MakeImage(transform, "Overlay", Vector2.zero, Vector2.one).color = new Color(0f, 0f, 0f, 0.6f);

            // 面板主体
            var panel = MakeImage(transform, "Panel",
                new Vector2(0.3f, 0.18f), new Vector2(0.7f, 0.88f));
            panel.color = new Color(0.08f, 0.09f, 0.13f, 0.97f);
            var pt = panel.transform;

            MakeText(pt, "Title", title, 24, new Vector2(0f, 0.87f), Vector2.one);

            // 分割线
            MakeImage(pt, "Divider", new Vector2(0.04f, 0.855f), new Vector2(0.96f, 0.86f))
                .color = new Color(1f, 1f, 1f, 0.2f);

            // 配方列表
            var list = MakeRect(pt, "RecipeList", new Vector2(0.04f, 0.12f), new Vector2(0.96f, 0.84f));
            list.gameObject.AddComponent<Image>().color = Color.clear;
            var vlg                    = list.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing                = 6f;
            vlg.padding                = new RectOffset(4, 4, 4, 4);
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight     = false;

            foreach (var r in recipes)
            {
                if (r != null) AddRecipeRow(list, r);
            }

            // 关闭提示
            MakeText(pt, "Hint", "[ Esc ] 关闭", 13,
                Vector2.zero, new Vector2(1f, 0.1f), new Color(1f, 1f, 1f, 0.4f));
        }

        void AddRecipeRow(RectTransform parent, RecipeData recipe)
        {
            var inv      = InventoryManager.Instance;
            bool canCraft = inv != null && inv.GetCount(recipe.inputGid) >= recipe.inputCount;

            var rowGo   = new GameObject($"Row_{recipe.recipeName}");
            rowGo.transform.SetParent(parent, false);
            var rowRect = rowGo.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0f, 58f);

            var bg   = rowGo.AddComponent<Image>();
            bg.color = canCraft
                ? new Color(0.12f, 0.22f, 0.14f, 0.9f)
                : new Color(0.22f, 0.12f, 0.12f, 0.6f);

            string inputName  = InventoryManager.GetItemName(recipe.inputGid);
            string outputName = InventoryManager.GetItemName(recipe.outputGid);
            int    have       = inv?.GetCount(recipe.inputGid) ?? 0;
            string label      = $"{inputName} ×{recipe.inputCount}  →  {outputName} ×{recipe.outputCount}   ({recipe.processTime}s)";
            if (!canCraft) label += $"   [库存 {have}/{recipe.inputCount}]";

            var t   = MakeTextInRect(rowGo.transform, "Label", label, 15,
                new Vector2(0.02f, 0.1f), new Vector2(canCraft ? 0.73f : 0.98f, 0.9f));
            t.color = canCraft ? Color.white : new Color(1f, 1f, 1f, 0.38f);

            if (!canCraft) return;

            // 制作按钮
            var btnGo  = new GameObject("CraftBtn");
            btnGo.transform.SetParent(rowGo.transform, false);
            var btnR   = btnGo.AddComponent<RectTransform>();
            btnR.anchorMin = new Vector2(0.76f, 0.15f);
            btnR.anchorMax = new Vector2(0.97f, 0.85f);
            btnR.offsetMin = btnR.offsetMax = Vector2.zero;
            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.18f, 0.52f, 0.22f, 0.95f);
            var btn    = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var colors        = btn.colors;
            colors.highlightedColor = new Color(0.25f, 0.65f, 0.3f);
            btn.colors = colors;
            MakeTextInRect(btnGo.transform, "BtnLabel", "制  作", 16, Vector2.zero, Vector2.one);

            var captured = recipe;
            btn.onClick.AddListener(() => OnCraftClicked(captured));
        }

        void OnCraftClicked(RecipeData recipe)
        {
            _machine.StartProcessing(recipe);
            Close();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Close();
        }

        void Close()
        {
            if (_current == this) _current = null;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            Destroy(gameObject);
        }

        // ── UI 工具方法 ────────────────────────────────────────────────────────

        static RectTransform MakeRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r    = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = r.offsetMax = Vector2.zero;
            return r;
        }

        static Image MakeImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var r = MakeRect(parent, name, anchorMin, anchorMax);
            return r.gameObject.AddComponent<Image>();
        }

        Text MakeText(Transform parent, string name, string text, int size,
            Vector2 anchorMin, Vector2 anchorMax, Color? color = null)
        {
            var r = MakeRect(parent, name, anchorMin, anchorMax);
            return ApplyText(r.gameObject, text, size, color);
        }

        Text MakeTextInRect(Transform parent, string name, string text, int size,
            Vector2 anchorMin, Vector2 anchorMax, Color? color = null)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent, false);
            var r   = go.AddComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = r.offsetMax = Vector2.zero;
            return ApplyText(go, text, size, color);
        }

        Text ApplyText(GameObject go, string text, int size, Color? color)
        {
            var t       = go.AddComponent<Text>();
            t.text      = text;
            t.fontSize  = size;
            t.color     = color ?? Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            if (_font != null) t.font = _font;
            return t;
        }
    }
}
