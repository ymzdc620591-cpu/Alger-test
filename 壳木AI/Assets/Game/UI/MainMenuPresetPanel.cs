using Game.System;
using Starter.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.UI
{
    public class MainMenuPresetPanel : UIPanel
    {
        [SerializeField] Button _continueButton;
        [SerializeField] Button _newGameButton;
        [SerializeField] Button _loadButton;
        [SerializeField] Button _settingsButton;
        [SerializeField] Button _patchNotesButton;
        [SerializeField] Button _creditsButton;
        [SerializeField] Button _quitButton;
        [SerializeField] Button _expansionButton;

        void Reset()
        {
            panelAttr.autoScale = false;
            panelAttr.showBlur = false;
            panelAttr.forceClose = false;
        }

        void Start()
        {
            Bind(_continueButton, OnContinueClicked);
            Bind(_newGameButton, OnNewGameClicked);
            Bind(_loadButton, () => LogClick("Load"));
            Bind(_settingsButton, () => LogClick("Settings"));
            Bind(_patchNotesButton, () => LogClick("PatchNotes"));
            Bind(_creditsButton, () => LogClick("Credits"));
            Bind(_quitButton, OnQuitClicked);
            Bind(_expansionButton, () => LogClick("Expansion"));
        }

        static void Bind(Button button, UnityAction action)
        {
            if (button == null) return;
            button.onClick.AddListener(action);
        }

        void OnContinueClicked()
        {
            StartGameFromMenu("Continue");
        }

        void OnNewGameClicked()
        {
            StartGameFromMenu("NewGame");
        }

        void OnQuitClicked()
        {
            LogClick("Quit");
            Application.Quit();
        }

        void StartGameFromMenu(string actionName)
        {
            LogClick(actionName);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            GameManager.Instance.StartGame();
            UIManager.Inst.PopPanel(this);
        }

        static void LogClick(string actionName)
        {
            Debug.Log($"[MainMenuPresetPanel] Clicked {actionName}");
        }
    }
}
