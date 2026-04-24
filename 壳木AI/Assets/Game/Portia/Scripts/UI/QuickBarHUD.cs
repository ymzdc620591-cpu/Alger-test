using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Starter.Core;
using Game.System;

namespace Game.Portia
{
    public class QuickBarHUD : MonoBehaviour
    {
        const int SlotCount = 8;

        Canvas _canvas;
        ItemIconConfig _iconConfig;

        readonly List<SlotView> _slots = new List<SlotView>(SlotCount);

        void Awake()
        {
            _iconConfig = Resources.Load<ItemIconConfig>("Game/ItemIconConfig");
            BuildUI();
            EventBus.On<ItemReceivedEvent>(OnItemReceived);
            EventBus.On<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.On<GameStateChangedEvent>(OnGameStateChanged);
        }

        void Start()
        {
            RefreshVisibility();
            SeedFromInventory();
        }

        void OnDestroy()
        {
            EventBus.Off<ItemReceivedEvent>(OnItemReceived);
            EventBus.Off<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Off<GameStateChangedEvent>(OnGameStateChanged);
        }

        void OnItemReceived(ItemReceivedEvent e)
        {
            if (e.Count <= 0) return;

            int totalCount = InventoryManager.Instance != null
                ? InventoryManager.Instance.GetCount(e.Gid)
                : e.Count;

            int existing = FindSlotByGid(e.Gid);
            if (existing >= 0)
            {
                SetSlot(existing, e.Gid, totalCount);
                return;
            }

            int empty = FindFirstEmptySlot();
            if (empty >= 0)
                SetSlot(empty, e.Gid, totalCount);
        }

        void OnInventoryChanged(InventoryChangedEvent e)
        {
            int existing = FindSlotByGid(e.Gid);
            if (existing < 0) return;

            if (e.NewCount > 0) SetSlot(existing, e.Gid, e.NewCount);
            else                ClearSlot(existing);
        }

        void OnGameStateChanged(GameStateChangedEvent e)
        {
            RefreshVisibility();
        }

        void SeedFromInventory()
        {
            if (InventoryManager.Instance == null) return;

            int slotIndex = 0;
            foreach (var kvp in InventoryManager.Instance.AllItems)
            {
                if (slotIndex >= SlotCount) break;
                if (kvp.Value <= 0) continue;

                SetSlot(slotIndex, kvp.Key, kvp.Value);
                slotIndex++;
            }
        }

        void BuildUI()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 22;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var root = UIHelper.MakeRect(transform, "QuickBarRoot",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            root.pivot = new Vector2(0.5f, 0f);
            root.sizeDelta = new Vector2(560f, 82f);
            root.anchoredPosition = new Vector2(0f, 28f);

            var layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            for (int i = 0; i < SlotCount; i++)
                _slots.Add(CreateSlot(root, i));
        }

        SlotView CreateSlot(RectTransform parent, int index)
        {
            var slotGo = new GameObject($"QuickSlot_{index + 1}");
            slotGo.transform.SetParent(parent, false);

            var rect = slotGo.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(60f, 60f);

            var button = slotGo.AddComponent<Button>();
            button.transition = Selectable.Transition.None;

            var bg = slotGo.AddComponent<Image>();
            bg.color = new Color(0.40f, 0.78f, 0.96f, 0.92f);
            button.targetGraphic = bg;

            var border = UIHelper.MakeImage(slotGo.transform, "Border", Vector2.zero, Vector2.one);
            border.color = new Color(0.82f, 0.95f, 1f, 0.85f);
            border.raycastTarget = false;
            border.rectTransform.offsetMin = Vector2.zero;
            border.rectTransform.offsetMax = Vector2.zero;

            var inner = UIHelper.MakeImage(slotGo.transform, "Inner", Vector2.zero, Vector2.one);
            inner.color = new Color(0.28f, 0.68f, 0.92f, 0.95f);
            inner.raycastTarget = false;
            inner.rectTransform.offsetMin = new Vector2(3f, 3f);
            inner.rectTransform.offsetMax = new Vector2(-3f, -3f);

            var icon = UIHelper.MakeImage(slotGo.transform, "Icon",
                new Vector2(0.14f, 0.16f), new Vector2(0.86f, 0.86f));
            icon.color = new Color(1f, 1f, 1f, 0f);
            icon.raycastTarget = false;
            icon.preserveAspect = true;

            var count = UIHelper.MakeText(slotGo.transform, "Count", string.Empty, 14,
                new Vector2(0f, 0f), new Vector2(1f, 0.28f), Color.white);
            count.alignment = TextAnchor.LowerRight;
            count.raycastTarget = false;
            count.rectTransform.offsetMin = new Vector2(4f, 0f);
            count.rectTransform.offsetMax = new Vector2(-6f, -4f);
            var countOutline = count.gameObject.AddComponent<Outline>();
            countOutline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            countOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var label = UIHelper.MakeText(slotGo.transform, "Label", (index + 1).ToString(), 11,
                new Vector2(0f, 0.72f), new Vector2(0.3f, 1f), new Color(1f, 1f, 1f, 0.65f));
            label.alignment = TextAnchor.UpperLeft;
            label.raycastTarget = false;
            label.rectTransform.offsetMin = new Vector2(6f, 0f);
            label.rectTransform.offsetMax = Vector2.zero;

            return new SlotView
            {
                Root = rect,
                Background = bg,
                Inner = inner,
                Icon = icon,
                CountText = count,
                Gid = -1
            };
        }

        void SetSlot(int index, int gid, int count)
        {
            if (index < 0 || index >= _slots.Count) return;

            var slot = _slots[index];
            slot.Gid = gid;

            var sprite = _iconConfig != null ? _iconConfig.GetIcon(gid) : null;
            slot.Icon.sprite = sprite;
            slot.Icon.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
            slot.CountText.text = count > 0 ? count.ToString() : string.Empty;
            slot.Background.color = new Color(0.40f, 0.78f, 0.96f, 0.98f);
            slot.Inner.color = new Color(0.28f, 0.68f, 0.92f, 0.98f);
        }

        void ClearSlot(int index)
        {
            if (index < 0 || index >= _slots.Count) return;

            var slot = _slots[index];
            slot.Gid = -1;
            slot.Icon.sprite = null;
            slot.Icon.color = new Color(1f, 1f, 1f, 0f);
            slot.CountText.text = string.Empty;
            slot.Background.color = new Color(0.40f, 0.78f, 0.96f, 0.92f);
            slot.Inner.color = new Color(0.28f, 0.68f, 0.92f, 0.95f);
        }

        int FindSlotByGid(int gid)
        {
            for (int i = 0; i < _slots.Count; i++)
                if (_slots[i].Gid == gid)
                    return i;

            return -1;
        }

        int FindFirstEmptySlot()
        {
            for (int i = 0; i < _slots.Count; i++)
                if (_slots[i].Gid < 0)
                    return i;

            return -1;
        }

        void RefreshVisibility()
        {
            if (_canvas == null) return;
            _canvas.enabled = GameManager.Instance != null &&
                              GameManager.Instance.State == GameState.Playing;
        }

        class SlotView
        {
            public RectTransform Root;
            public Image Background;
            public Image Inner;
            public Image Icon;
            public Text CountText;
            public int Gid;
        }
    }
}
