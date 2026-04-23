using UnityEngine;
using UnityEngine.UI;
using Starter.Core;

namespace Game.Portia
{
    public class InteractPromptUI : MonoBehaviour
    {
        RectTransform _promptRoot;
        Text          _promptLabel;

        void Awake()
        {
            BuildCanvas();
            BuildCrosshair();
            BuildPrompt();
            SetPromptVisible(false);
            EventBus.On<InteractTargetChangedEvent>(OnTargetChanged);
        }

        void OnDestroy() => EventBus.Off<InteractTargetChangedEvent>(OnTargetChanged);

        void OnTargetChanged(InteractTargetChangedEvent e)
        {
            if (e.Target == null) { SetPromptVisible(false); return; }
            _promptLabel.text = e.Target.PromptText;
            SetPromptVisible(true);
        }

        void SetPromptVisible(bool v) => _promptRoot.gameObject.SetActive(v);

        void BuildCanvas()
        {
            var c = gameObject.AddComponent<Canvas>();
            c.renderMode   = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 20;
            gameObject.AddComponent<CanvasScaler>();
        }

        void BuildCrosshair()
        {
            float armLen   = 10f;
            float armThick = 2f;
            float gap      = 5f;

            var arms = new[] {
                new Vector2( gap + armLen * 0.5f, 0f),
                new Vector2(-(gap + armLen * 0.5f), 0f),
                new Vector2(0f,  gap + armLen * 0.5f),
                new Vector2(0f, -(gap + armLen * 0.5f)),
            };
            var sizes = new[] {
                new Vector2(armLen, armThick),
                new Vector2(armLen, armThick),
                new Vector2(armThick, armLen),
                new Vector2(armThick, armLen),
            };

            for (int i = 0; i < 4; i++)
            {
                var go   = new GameObject($"CrosshairArm{i}");
                go.transform.SetParent(transform, false);
                var rect = go.AddComponent<RectTransform>();
                rect.anchorMin        = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta        = sizes[i];
                rect.anchoredPosition = arms[i];
                var img  = go.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0.85f);
            }
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
