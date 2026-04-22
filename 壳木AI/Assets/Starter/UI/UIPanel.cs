using System;
using UnityEngine;
using UnityEngine.UI;

namespace Starter.UI
{
    // 挂载在每个 UI 面板 prefab 的根节点上
    // 层级结构要求：CanvasPanel → Content → [本GameObject]
    [DisallowMultipleComponent]
    public class UIPanel : MonoBehaviour
    {
        public static readonly Color BlurColor        = new(0f, 0f, 0f, 0.8f);
        public static readonly Color TransparentColor = new(0f, 0f, 0f, 0f);

        public PanelAttr panelAttr;

        Animator _animator;

        public virtual void OnPop() { }

        public void FadeIn()
        {
            _animator = transform.parent.parent.GetComponent<Animator>();

            if (panelAttr.autoScale && _animator != null)
            {
                _animator.Rebind();
                _animator.Play("PushPanelAutoScale");
            }

            if (!panelAttr.showBlur && !panelAttr.forceClose) return;

            // 在 Content 下插入全屏遮罩（在面板之前）
            var overlayRoot = _animator != null ? _animator.transform : transform.parent.parent;
            var overlay = new GameObject("Overlay");
            overlay.transform.SetParent(overlayRoot, false);
            overlay.transform.SetAsFirstSibling();
            overlay.layer = LayerMask.NameToLayer("UI");

            var rect = overlay.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = overlay.AddComponent<Image>();
            img.raycastTarget = true;
            img.color = panelAttr.showBlur ? BlurColor : TransparentColor;

            if (panelAttr.forceClose)
            {
                var btn = overlay.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(OnCloseButtonClick);
            }
        }

        public virtual void FadeOut()
        {
            // 销毁最外层 CanvasPanel（parent=Content, parent.parent=CanvasPanel）
            Destroy(transform.parent.parent.gameObject);
        }

        public void OnCloseButtonClick()
        {
            UIManager.Inst.PopPanel(this);
        }

        [Serializable]
        public class PanelAttr
        {
            [Tooltip("入场时播放缩放动画（需 CanvasPanel prefab 带 Animator）")]
            public bool autoScale = true;

            [Tooltip("显示半透明黑色遮罩")]
            public bool showBlur = true;

            [Tooltip("点击遮罩自动关闭面板")]
            public bool forceClose = false;
        }
    }
}
