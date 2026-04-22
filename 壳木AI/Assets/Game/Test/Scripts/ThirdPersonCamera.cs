using UnityEngine;
using Starter.Core;
using Game.System;

namespace Game.Test
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField, Tooltip("跟随目标（Player Transform）")] Transform _target;
        [SerializeField, Tooltip("摄像机与目标的距离")] float _distance = 5f;
        [SerializeField, Tooltip("视点高度偏移")] float _pivotHeight = 1.5f;
        [SerializeField, Tooltip("鼠标灵敏度")] float _sensitivity = 3f;
        [SerializeField, Tooltip("俯仰角最小值")] float _minPitch = -15f;
        [SerializeField, Tooltip("俯仰角最大值")] float _maxPitch = 50f;

        float _yaw;
        float _pitch = 20f;
        bool _isPlaying;

        void Start()
        {
            EventBus.On<GameStateChangedEvent>(OnGameStateChanged);
            if (GameManager.Instance != null)
                _isPlaying = GameManager.Instance.State == GameState.Playing;
        }

        void OnDestroy()
        {
            EventBus.Off<GameStateChangedEvent>(OnGameStateChanged);
        }

        void OnGameStateChanged(GameStateChangedEvent evt)
        {
            _isPlaying = evt.Current == GameState.Playing;
            Cursor.lockState = _isPlaying ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible   = !_isPlaying;
        }

        void LateUpdate()
        {
            if (_target == null || !_isPlaying) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible   = true;
            }

            _yaw   += Input.GetAxis("Mouse X") * _sensitivity;
            _pitch -= Input.GetAxis("Mouse Y") * _sensitivity;
            _pitch  = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

            var pivot    = _target.position + Vector3.up * _pivotHeight;
            var rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.position = pivot + rotation * Vector3.back * _distance;
            transform.LookAt(pivot);
        }

        public void SetTarget(Transform target) => _target = target;
    }
}
