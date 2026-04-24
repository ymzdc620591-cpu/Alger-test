using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.System;

namespace Game.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] float _moveSpeed   = 12f;
        [SerializeField] float _sprintSpeed = 20f;

        [Header("Jump & Gravity")]
        [SerializeField] float _jumpHeight = 1.8f;   // 跳跃最大高度（单位）
        [SerializeField] float _gravity    = -20f;   // 重力加速度（地球体感 ≈ -20）

        [Header("Camera")]
        [SerializeField] Transform _cameraTransform;

        [Header("Animation")]
        [SerializeField] Animator _animator;
        // Maps Unity speed (units/s) to Anim_Tall_Man blend tree range [2.4 walk → 6.5 run → 8.5 fastrun]
        [SerializeField] float _animSpeedScale = 0.375f;

        static readonly int _hashSpeed    = Animator.StringToHash("Speed");
        static readonly int _hashOnGround = Animator.StringToHash("OnGround");
        static readonly int _hashVY       = Animator.StringToHash("VY");
        static readonly int _hashJump     = Animator.StringToHash("Jump");
        static readonly int[] _hashAttacks =
        {
            Animator.StringToHash("Attack0"),
            Animator.StringToHash("Attack1"),
            Animator.StringToHash("Attack2"),
        };

        public bool ExternalRotation { get; set; }

        CharacterController _cc;
        Animator[] _animators;
        float _yVelocity;
        float _coyoteTimer;
        int   _attackIndex;
        bool  _isAttacking;

        const float CoyoteTime = 0.15f;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;
            CacheAnimators();
        }

        void CacheAnimators()
        {
            var found = GetComponentsInChildren<Animator>(true);
            var animators = new List<Animator>(found.Length + (_animator != null ? 1 : 0));

            if (_animator != null)
                animators.Add(_animator);

            foreach (var animator in found)
            {
                if (animator == null || animators.Contains(animator))
                    continue;

                animators.Add(animator);
            }

            _animators = animators.ToArray();
            if (_animator == null && _animators.Length > 0)
                _animator = _animators[0];

            foreach (var animator in _animators)
                animator.fireEvents = false;
        }

        void SetAnimatorFloat(int hash, float value)
        {
            if (_animators == null || _animators.Length == 0)
                return;

            foreach (var animator in _animators)
                animator.SetFloat(hash, value);
        }

        void SetAnimatorBool(int hash, bool value)
        {
            if (_animators == null || _animators.Length == 0)
                return;

            foreach (var animator in _animators)
                animator.SetBool(hash, value);
        }

        void SetAnimatorTrigger(int hash)
        {
            if (_animators == null || _animators.Length == 0)
                return;

            foreach (var animator in _animators)
                animator.SetTrigger(hash);
        }

        void Update()
        {
            if (GameManager.Instance.State != GameState.Playing) return;
            HandleMovement();
            HandleVertical();
        }

        void HandleMovement()
        {
            if (_isAttacking) return;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            Vector3 forward = _cameraTransform ? _cameraTransform.forward : Vector3.forward;
            Vector3 right   = _cameraTransform ? _cameraTransform.right   : Vector3.right;
            forward.y = 0f; forward.Normalize();
            right.y   = 0f; right.Normalize();

            Vector3 move = forward * v + right * h;
            if (move.magnitude > 1f) move.Normalize();

            float speed = Input.GetKey(KeyCode.LeftShift) ? _sprintSpeed : _moveSpeed;
            _cc.Move(move * (speed * Time.deltaTime));

            if (move.sqrMagnitude > 0.01f && !ExternalRotation)
                transform.forward = Vector3.Slerp(transform.forward, move, 10f * Time.deltaTime);

            SetAnimatorFloat(_hashSpeed, move.magnitude * speed * _animSpeedScale);
        }

        // SphereCast 覆盖整个脚底面积，斜面/不平地形均能检测
        bool IsGrounded()
        {
            var   origin   = transform.TransformPoint(_cc.center);
            float castDist = _cc.height * 0.5f - _cc.radius + 0.2f;
            return Physics.SphereCast(origin, _cc.radius * 0.9f, Vector3.down, out _,
                castDist, ~LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore);
        }

        void HandleVertical()
        {
            bool grounded = IsGrounded();

            if (grounded)
            {
                _coyoteTimer = CoyoteTime;
                if (_yVelocity < 0f) _yVelocity = -2f;
            }
            else
            {
                _coyoteTimer -= Time.deltaTime;
                _yVelocity   += _gravity * Time.deltaTime;
            }

            // Coyote Time 窗口内均可起跳（含刚走下台阶的瞬间）
            if (_coyoteTimer > 0f && Input.GetKeyDown(KeyCode.Space))
            {
                _yVelocity   = Mathf.Sqrt(-2f * _gravity * _jumpHeight);
                _coyoteTimer = 0f;
                SetAnimatorTrigger(_hashJump);
            }

            SetAnimatorBool(_hashOnGround, grounded);
            SetAnimatorFloat(_hashVY, _yVelocity);

            _cc.Move(Vector3.up * (_yVelocity * Time.deltaTime));
        }

        public void PlayAttack()
        {
            if (_animators == null || _animators.Length == 0) return;
            if (_isAttacking) return;

            _isAttacking = true;
            SetAnimatorTrigger(_hashAttacks[_attackIndex % _hashAttacks.Length]);
            _attackIndex++;

            StartCoroutine(AttackEndRoutine());
        }

        IEnumerator AttackEndRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            _isAttacking = false;
        }
    }
}
