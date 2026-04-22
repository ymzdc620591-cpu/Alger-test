using UnityEngine;

namespace Starter.UI
{
    // 所有游戏UI面板的基类，继承此类并重写 OnShow/OnHide
    public abstract class UIPanel : MonoBehaviour
    {
        public bool IsVisible { get; private set; }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            IsVisible = true;
            OnShow();
        }

        public virtual void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
            IsVisible = false;
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}
