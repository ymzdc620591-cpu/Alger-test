using Game.System;
using Starter.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class TestPausePanel : UIPanel
    {
        [SerializeField] Button _resumeButton;
        [SerializeField] Button _endButton;
        [SerializeField] Button _quitButton;

        void Reset()
        {
            panelAttr          ??= new PanelAttr();
            panelAttr.autoScale  = true;
            panelAttr.showBlur   = true;
            panelAttr.forceClose = false;
        }

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;

            _resumeButton?.onClick.AddListener(OnResumeClicked);
            _endButton?.onClick.AddListener(OnEndClicked);
            _quitButton?.onClick.AddListener(OnQuitClicked);
        }

        public override void OnPop()
        {
            GameManager.Instance.ResumeGame();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        void OnResumeClicked() => UIManager.Inst.PopPanel(this);

        void OnEndClicked()
        {
            UIManager.Inst.PopAllPanels();
            GameManager.Instance.GameOver();
        }

        void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
