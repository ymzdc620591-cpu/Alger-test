using UnityEngine;
using Starter.Core;
using Starter.UI;
using Game.System;

namespace Game.Test
{
    public class TestSceneBootstrap : MonoBehaviour
    {
        [SerializeField, Tooltip("场景中的玩家对象（开始前隐藏，点击开始后激活）")]
        GameObject _playerGo;

        [SerializeField, Tooltip("玩家出生点，高于地面使其自由落体（留空则用 (0,5,0)）")]
        Transform _spawnPoint;

        void Start()
        {
            if (_playerGo != null)
                _playerGo.SetActive(false);

            UIManager.Inst.Init();
            EventBus.On<GameStateChangedEvent>(OnGameStateChanged);
            UIManager.Inst.PushPanel("UI/TestStartPanel");
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
