using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Starter.Core;
using Starter.UI;
using Game.System;

namespace Game.Portia
{
    public class InventoryHUD : MonoBehaviour
    {
        Canvas    _canvas;
        Transform _slotContainer;
        bool      _open;

        readonly Dictionary<int, Text> _countTexts = new Dictionary<int, Text>();

        void Awake()
        {
            BuildUI();
            _canvas.gameObject.SetActive(false);
            EventBus.On<InventoryChangedEvent>(OnInventoryChanged);
        }

        void OnDestroy() => EventBus.Off<InventoryChangedEvent>(OnInventoryChanged);

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Tab)) return;
            var state = GameManager.Instance?.State;
            if (!_open && state != GameState.Playing) return;

            if (!_open) OpenBag();
            else        UIManager.Inst.PopPanel(); // UIManager 调用 CloseBag 回调
        }

        void OpenBag()
        {
            _open = true;
            _canvas.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            RefreshAll();
            // 向 UIManager 注册，并拿到正确的层级顺序
            _canvas.sortingOrder = UIManager.Inst.PushExternal(CloseBag);
        }

        // 由 UIManager 调用（ESC 或 Tab 关闭时）
        void CloseBag()
        {
            _open = false;
            _canvas.gameObject.SetActive(false);
            // 仅当所有 UI 都关闭时才锁定鼠标
            if (!UIManager.Inst.HasAnyPanel())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible   = false;
            }
        }

        void OnInventoryChanged(InventoryChangedEvent e)
        {
            if (_open) UpdateOrCreateSlot(e.Gid, e.NewCount);
        }

        void RefreshAll()
        {
            if (InventoryManager.Instance == null) return;
            foreach (var kvp in InventoryManager.Instance.AllItems)
                UpdateOrCreateSlot(kvp.Key, kvp.Value);
        }

        void UpdateOrCreateSlot(int gid, int count)
        {
            if (!_countTexts.TryGetValue(gid, out var t))
            {
                t = CreateSlot(gid);
                _countTexts[gid] = t;
            }
            t.text = $"×{count}";
        }

        Text CreateSlot(int gid)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                    ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            var slotGo = new GameObject($"Slot_{gid}");
            slotGo.transform.SetParent(_slotContainer, false);
            slotGo.AddComponent<RectTransform>();
            var bg   = slotGo.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.17f, 0.22f, 1f);

            var iconGo   = new GameObject("Icon");
            iconGo.transform.SetParent(slotGo.transform, false);
            var iconRect = iconGo.AddComponent<RectTransform>();
            iconRect.anchorMin        = new Vector2(0.1f, 0.42f);
            iconRect.anchorMax        = new Vector2(0.9f, 0.95f);
            iconRect.offsetMin        = iconRect.offsetMax = Vector2.zero;
            iconGo.AddComponent<Image>().color = new Color(0.3f, 0.32f, 0.4f, 0.8f);

            var nameGo = new GameObject("Name");
            nameGo.transform.SetParent(slotGo.transform, false);
            var nameRect = nameGo.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.22f);
            nameRect.anchorMax = new Vector2(1f, 0.46f);
            nameRect.offsetMin = nameRect.offsetMax = Vector2.zero;
            var nameText      = nameGo.AddComponent<Text>();
            nameText.text     = InventoryManager.GetItemName(gid);
            nameText.fontSize = 13;
            nameText.color    = new Color(0.85f, 0.85f, 0.85f);
            nameText.alignment = TextAnchor.MiddleCenter;
            if (font != null) nameText.font = font;

            var countGo = new GameObject("Count");
            countGo.transform.SetParent(slotGo.transform, false);
            var countRect = countGo.AddComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0f, 0f);
            countRect.anchorMax = new Vector2(1f, 0.22f);
            countRect.offsetMin = countRect.offsetMax = Vector2.zero;
            var countText      = countGo.AddComponent<Text>();
            countText.fontSize = 14;
            countText.color    = new Color(1f, 0.85f, 0.3f);
            countText.alignment = TextAnchor.MiddleCenter;
            if (font != null) countText.font = font;

            return countText;
        }

        void BuildUI()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                    ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            var canvasGo = new GameObject("InventoryCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas              = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 10; // 初始值低，OpenBag 时动态设置
            canvasGo.AddComponent<CanvasScaler>();

            var bgGo   = new GameObject("Bg");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = bgRect.offsetMax = Vector2.zero;
            bgGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);

            var panelGo   = new GameObject("Panel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.08f);
            panelRect.anchorMax = new Vector2(0.9f, 0.92f);
            panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
            panelGo.AddComponent<Image>().color = new Color(0.07f, 0.08f, 0.12f, 0.98f);

            AddLabel(panelGo.transform, "Title", "背    包", 30,
                new Vector2(0f, 0.91f), new Vector2(1f, 1f), font);

            AddDivider(panelGo.transform, new Vector2(0.02f, 0.905f), new Vector2(0.98f, 0.908f));

            var scrollGo   = new GameObject("ScrollView");
            scrollGo.transform.SetParent(panelGo.transform, false);
            var scrollRt   = scrollGo.AddComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0.01f, 0.08f);
            scrollRt.anchorMax = new Vector2(0.99f, 0.90f);
            scrollRt.offsetMin = scrollRt.offsetMax = Vector2.zero;
            var sr         = scrollGo.AddComponent<ScrollRect>();
            sr.horizontal  = false;
            sr.vertical    = true;
            sr.scrollSensitivity = 30f;

            var vpGo   = new GameObject("Viewport");
            vpGo.transform.SetParent(scrollGo.transform, false);
            var vpRect = vpGo.AddComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = new Vector2(0f, 0f);
            vpRect.offsetMax = new Vector2(-18f, 0f);
            vpGo.AddComponent<RectMask2D>();
            sr.viewport = vpRect;

            var contentGo   = new GameObject("Content");
            contentGo.transform.SetParent(vpGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot     = new Vector2(0.5f, 1f);
            contentRect.offsetMin = contentRect.offsetMax = Vector2.zero;
            var glg               = contentGo.AddComponent<GridLayoutGroup>();
            glg.cellSize          = new Vector2(120f, 105f);
            glg.spacing           = new Vector2(10f, 10f);
            glg.padding           = new RectOffset(12, 12, 12, 12);
            glg.constraint        = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount   = 6;
            glg.childAlignment    = TextAnchor.UpperLeft;
            var csf               = contentGo.AddComponent<ContentSizeFitter>();
            csf.verticalFit       = ContentSizeFitter.FitMode.PreferredSize;
            sr.content            = contentRect;
            _slotContainer        = contentGo.transform;

            var sbGo   = new GameObject("Scrollbar");
            sbGo.transform.SetParent(scrollGo.transform, false);
            var sbRect = sbGo.AddComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(1f, 0f);
            sbRect.anchorMax = Vector2.one;
            sbRect.offsetMin = new Vector2(-18f, 0f);
            sbRect.offsetMax = Vector2.zero;
            sbGo.AddComponent<Image>().color = new Color(0.12f, 0.13f, 0.17f, 1f);
            var sb            = sbGo.AddComponent<Scrollbar>();
            sb.direction      = Scrollbar.Direction.BottomToTop;

            var handleGo   = new GameObject("Handle");
            handleGo.transform.SetParent(sbGo.transform, false);
            var handleRect = handleGo.AddComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = new Vector2(2f, 2f);
            handleRect.offsetMax = new Vector2(-2f, -2f);
            var handleImg          = handleGo.AddComponent<Image>();
            handleImg.color        = new Color(0.45f, 0.47f, 0.6f, 1f);
            sb.handleRect          = handleRect;
            sb.targetGraphic       = handleImg;

            sr.verticalScrollbar                = sb;
            sr.verticalScrollbarVisibility      = ScrollRect.ScrollbarVisibility.AutoHide;

            AddDivider(panelGo.transform, new Vector2(0.02f, 0.072f), new Vector2(0.98f, 0.075f));
            AddLabel(panelGo.transform, "Hint", "[ Tab ] 关闭背包", 14,
                new Vector2(0f, 0f), new Vector2(1f, 0.07f), font,
                new Color(0.6f, 0.6f, 0.65f, 1f));
        }

        static void AddLabel(Transform parent, string name, string text, int fontSize,
            Vector2 anchorMin, Vector2 anchorMax, Font font, Color? color = null)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var t       = go.AddComponent<Text>();
            t.text      = text;
            t.fontSize  = fontSize;
            t.color     = color ?? Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            if (font != null) t.font = font;
        }

        static void AddDivider(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go   = new GameObject("Divider");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);
        }
    }
}
