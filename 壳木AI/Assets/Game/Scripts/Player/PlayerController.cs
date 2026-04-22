using UnityEngine;
using Game.System;

namespace Game.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Tooltip("移动速度")] float _moveSpeed = 5f;
        [SerializeField, Tooltip("跳跃力度")] float _jumpForce = 5f;
        [SerializeField, Tooltip("重力倍数")] float _gravityScale = 2f;

        [Header("Camera")]
        [SerializeField, Tooltip("相机Transform，留空自动取 Camera.main")] Transform _cameraTransform;

        CharacterController _cc;
        Vector3 _velocity;

        const float Gravity = -9.81f;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        void Update()
        {
            if (GameManager.Instance.State != GameState.Playing) return;
            HandleGround();
            HandleMovement();
            HandleJump();
            ApplyGravity();
        }

        void HandleGround()
        {
            if (_cc.isGrounded && _velocity.y < 0f)
                _velocity.y = -2f;
        }

        void HandleMovement()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            Vector3 forward = _cameraTransform ? _cameraTransform.forward : Vector3.forward;
            Vector3 right   = _cameraTransform ? _cameraTransform.right   : Vector3.right;
            forward.y = 0f; forward.Normalize();
            right.y   = 0f; right.Normalize();

            Vector3 move = forward * v + right * h;
            if (move.magnitude > 1f) move.Normalize();

            _cc.Move(move * (_moveSpeed * Time.deltaTime));

            if (move.sqrMagnitude > 0.01f)
                transform.forward = Vector3.Slerp(transform.forward, move, 10f * Time.deltaTime);
        }

        void HandleJump()
        {
            if (Input.GetButtonDown("Jump") && _cc.isGrounded)
                _velocity.y = Mathf.Sqrt(_jumpForce * -2f * Gravity * _gravityScale);
        }

        void ApplyGravity()
        {
            _velocity.y += Gravity * _gravityScale * Time.deltaTime;
            _cc.Move(_velocity * Time.deltaTime);
        }
    }
}
