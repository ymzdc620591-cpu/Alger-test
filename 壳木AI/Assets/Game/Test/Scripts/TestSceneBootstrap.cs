using Game.System;
using Starter.Core;
using Starter.UI;
using UnityEngine;

namespace Game.Test
{
    public class TestSceneBootstrap : MonoBehaviour
    {
        [SerializeField, Tooltip("Panel resource path opened on scene start")]
        string _startPanelRes = "UI/MainMenuPresetPanel";

        [SerializeField, Tooltip("Player object kept hidden until gameplay starts")]
        GameObject _playerGo;

        [SerializeField, Tooltip("Optional spawn point, defaults to (0,5,0)")]
        Transform _spawnPoint;

        void Start()
        {
            if (_playerGo != null)
                _playerGo.SetActive(false);

            UIManager.Inst.Init();
            EventBus.On<GameStateChangedEvent>(OnGameStateChanged);
            UIManager.Inst.PushPanel(_startPanelRes);
        }

        void OnDestroy()
        {
            EventBus.Off<GameStateChangedEvent>(OnGameStateChanged);
        }

        void OnGameStateChanged(GameStateChangedEvent evt)
        {
            if (evt.Current != GameState.Playing || _playerGo == null) return;

            var pos = _spawnPoint != null ? _spawnPoint.position : new Vector3(0f, 5f, 0f);
            _playerGo.transform.position = pos;
            _playerGo.SetActive(true);
        }
    }
}
