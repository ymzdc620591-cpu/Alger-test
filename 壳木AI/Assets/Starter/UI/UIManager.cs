using System;
using System.Collections.Generic;
using UnityEngine;
using Starter.Core;

namespace Starter.UI
{
    // 用法：UIManager.Instance.Register(myHUDPanel);
    //       UIManager.Instance.Show<HUDPanel>();
    //       UIManager.Instance.Hide<HUDPanel>();
    public class UIManager : Singleton<UIManager>
    {
        readonly Dictionary<Type, UIPanel> _panels = new();

        public void Register<T>(T panel) where T : UIPanel
            => _panels[typeof(T)] = panel;

        public void Show<T>() where T : UIPanel
        {
            if (_panels.TryGetValue(typeof(T), out var panel))
                panel.Show();
            else
                Debug.LogWarning($"[UIManager] {typeof(T).Name} 未注册");
        }

        public void Hide<T>() where T : UIPanel
        {
            if (_panels.TryGetValue(typeof(T), out var panel))
                panel.Hide();
        }

        public T Get<T>() where T : UIPanel
        {
            _panels.TryGetValue(typeof(T), out var panel);
            return panel as T;
        }

        public void HideAll()
        {
            foreach (var panel in _panels.Values)
                panel.Hide();
        }
    }
}
