using UnityEngine;
using UnityEngine.UI;
using Starter.Core;
using Game.System;

namespace Game.Portia
{
    public class InteractPromptUI : MonoBehaviour
    {
        Canvas        _canvas;
        RectTransform _promptRoot;
        Text          _promptLabel;

        void Awake()
        {
            BuildCanvas();
            BuildPrompt();
            SetPromptVisible(false);
            _canvas.enabled = false; // 等进入 Playing 状态后才显示
            EventBus.On<InteractTargetChangedEvent>(OnTargetChanged);
            EventBus.On<GameStateChangedEvent>(OnGameStateChanged);
        }

        void OnDestroy()
        {
            EventBus.Off<InteractTargetChangedEvent>(OnTargetChanged);
            EventBus.Off<GameStateChangedEvent>(OnGameStateChanged);
        }

        void OnTargetChanged(InteractTargetChangedEvent e)
        {
            if (e.Target == null || string.IsNullOrEmpty(e.Target.PromptText))
            { SetPromptVisible(false); return; }
            _promptLabel.text = e.Target.PromptText;
            SetPromptVisible(true);
        }

        void OnGameStateChanged(GameStateChangedEvent e)
        {
            _canvas.enabled = e.Current == GameState.Playing;
            if (e.Current != GameState.Playing) SetPromptVisible(false);
        }

        void SetPromptVisible(bool v) => _promptRoot.gameObject.SetActive(v);

        void BuildCanvas()
        {
            _canvas              = gameObject.AddComponent<Canvas>();
            _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 20;
            gameObject.AddComponent<CanvasScaler>();
        }

        void BuildPrompt()
        {
            var go = new GameObject("PromptRoot");
            go.transform.SetParent(transform, false);
            _promptRoot               = go.AddComponent<RectTransform>();
            _promptRoot.anchorMin     = _promptRoot.anchorMax = new Vector2(0.5f, 0.15f);
            _promptRoot.sizeDelta     = new Vector2(280f, 44f);
            _promptRoot.anchoredPosition = Vector2.zero;
            go.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var tRect  = textGo.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = tRect.offsetMax = Vector2.zero;

            _promptLabel           = textGo.AddComponent<Text>();
            _promptLabel.fontSize  = 20;
            _promptLabel.color     = Color.white;
            _promptLabel.alignment = TextAnchor.MiddleCenter;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                    ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null) _promptLabel.font = font;
        }
    }
}
