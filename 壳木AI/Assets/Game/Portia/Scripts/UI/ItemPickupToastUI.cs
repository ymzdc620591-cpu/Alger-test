using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Starter.Core;
using Game.System;

namespace Game.Portia
{
    public class ItemPickupToastUI : MonoBehaviour
    {
        const float Lifetime = 2f;
        const float FadeDuration = 0.3f;
        const float MoveSpeed = 12f;
        const float EntryHeight = 34f;
        const float EntrySpacing = 8f;

        readonly List<ToastEntry> _entries = new List<ToastEntry>();

        Canvas _canvas;
        RectTransform _stackRoot;

        void Awake()
        {
            BuildUI();
            EventBus.On<ItemReceivedEvent>(OnItemReceived);
            EventBus.On<GameStateChangedEvent>(OnGameStateChanged);
        }

        void OnDestroy()
        {
            EventBus.Off<ItemReceivedEvent>(OnItemReceived);
            EventBus.Off<GameStateChangedEvent>(OnGameStateChanged);
        }

        void Update()
        {
            if (_entries.Count == 0) return;

            float deltaTime = Time.unscaledDeltaTime;
            bool removedAny = false;
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                var entry = _entries[i];
                if (entry.Root == null)
                {
                    _entries.RemoveAt(i);
                    removedAny = true;
                    continue;
                }

                entry.Elapsed += deltaTime;

                var pos = entry.Root.anchoredPosition;
                pos.y = Mathf.Lerp(pos.y, entry.TargetY, deltaTime * MoveSpeed);
                entry.Root.anchoredPosition = pos;

                float remaining = Lifetime - entry.Elapsed;
                if (remaining <= 0f)
                {
                    Destroy(entry.Root.gameObject);
                    _entries.RemoveAt(i);
                    removedAny = true;
                    continue;
                }

                if (entry.Group != null)
                    entry.Group.alpha = remaining < FadeDuration ? remaining / FadeDuration : 1f;
            }

            if (removedAny)
                RefreshTargets();
        }

        void OnItemReceived(ItemReceivedEvent e)
        {
            if (e.Count <= 0) return;

            var entry = CreateEntry($"获得 {InventoryManager.GetItemName(e.Gid)} x{e.Count}");
            _entries.Add(entry);
            RefreshTargets();
        }

        void OnGameStateChanged(GameStateChangedEvent e)
        {
            if (_canvas == null) return;

            _canvas.enabled = e.Current == GameState.Playing;
            if (e.Current != GameState.Playing)
                ClearEntries();
        }

        void BuildUI()
        {
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 25;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            _stackRoot = UIHelper.MakeRect(transform, "ToastStack",
                new Vector2(0f, 0.18f), new Vector2(0f, 0.18f));
            _stackRoot.pivot = new Vector2(0f, 0f);
            _stackRoot.sizeDelta = new Vector2(420f, 320f);
            _stackRoot.anchoredPosition = new Vector2(28f, 0f);
        }

        ToastEntry CreateEntry(string message)
        {
            var root = new GameObject("Toast");
            root.transform.SetParent(_stackRoot, false);

            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(320f, EntryHeight);
            rect.anchoredPosition = Vector2.zero;

            var group = root.AddComponent<CanvasGroup>();

            var bg = root.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.58f);

            var accent = UIHelper.MakeImage(root.transform, "Accent",
                new Vector2(0f, 0f), new Vector2(0f, 1f));
            accent.rectTransform.sizeDelta = new Vector2(4f, 0f);
            accent.color = new Color(0.95f, 0.82f, 0.24f, 0.9f);

            var label = UIHelper.MakeText(root.transform, "Label", message, 18,
                Vector2.zero, Vector2.one, Color.white);
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.rectTransform.offsetMin = new Vector2(16f, 0f);
            label.rectTransform.offsetMax = new Vector2(-12f, 0f);

            return new ToastEntry
            {
                Root = rect,
                Group = group,
                TargetY = 0f,
                Elapsed = 0f
            };
        }

        void RefreshTargets()
        {
            for (int i = 0; i < _entries.Count; i++)
                _entries[_entries.Count - 1 - i].TargetY = i * (EntryHeight + EntrySpacing);
        }

        void ClearEntries()
        {
            foreach (var entry in _entries)
                if (entry.Root != null)
                    Destroy(entry.Root.gameObject);

            _entries.Clear();
        }

        class ToastEntry
        {
            public RectTransform Root;
            public CanvasGroup Group;
            public float TargetY;
            public float Elapsed;
        }
    }
}
