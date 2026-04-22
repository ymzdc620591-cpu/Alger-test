using UnityEngine;
using Starter.Core;

namespace Game.System
{
    public enum GameState { MainMenu, Loading, Playing, Paused, GameOver }

    public struct GameStateChangedEvent
    {
        public GameState Previous;
        public GameState Current;
    }

    public class GameManager : Singleton<GameManager>
    {
        GameState _state = GameState.MainMenu;
        public GameState State => _state;

        public void ChangeState(GameState next)
        {
            if (_state == next) return;
            var evt = new GameStateChangedEvent { Previous = _state, Current = next };
            _state = next;
            ApplyState(next);
            EventBus.Emit(evt);
        }

        void ApplyState(GameState state)
        {
            Time.timeScale = state switch
            {
                GameState.Paused   => 0f,
                GameState.GameOver => 0f,
                _                  => 1f
            };
        }

        public void StartGame()  => ChangeState(GameState.Playing);
        public void PauseGame()  => ChangeState(GameState.Paused);
        public void ResumeGame() => ChangeState(GameState.Playing);
        public void GameOver()   => ChangeState(GameState.GameOver);
    }
}
