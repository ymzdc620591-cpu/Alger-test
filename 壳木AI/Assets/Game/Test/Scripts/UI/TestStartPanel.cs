using UnityEngine;
using UnityEngine.UI;
using Starter.UI;
using Game.System;

namespace Game.Test
{
    public class TestStartPanel : UIPanel
    {
        [SerializeField, Tooltip("开始游戏按钮")] Button _startButton;

        // 在 Inspector 中添加此组件时设置合理默认值
        void Reset()
        {
            panelAttr.autoScale = false;
            panelAttr.showBlur  = false;
        }

        void Start()
        {
            if (_startButton != null)
                _startButton.onClick.AddListener(OnStartClicked);
        }

        void OnStartClicked()
        {
            UIManager.Inst.PopPanel(this);
            GameManager.Instance.StartGame();
        }
    }
}
