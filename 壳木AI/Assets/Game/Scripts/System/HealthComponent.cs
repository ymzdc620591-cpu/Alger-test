using System;
using UnityEngine;
using Starter.Core;

namespace Game.System
{
    public struct DamageEvent { public GameObject Source; public GameObject Target; public float Amount; }
    public struct DeathEvent  { public GameObject Target; }

    public class HealthComponent : MonoBehaviour
    {
        [SerializeField, Tooltip("最大生命值")] float _maxHealth = 100f;
        [SerializeField, Tooltip("受击无敌时间（秒）")] float _invincibleDuration = 0f;

        float _current;
        float _invincibleTimer;

        public float Current  => _current;
        public float Max      => _maxHealth;
        public float Ratio    => _current / _maxHealth;
        public bool  IsDead   => _current <= 0f;

        public event Action<float, float> OnHealthChanged;  // (current, max)
        public event Action OnDeath;

        void Awake() => _current = _maxHealth;

        void Update()
        {
            if (_invincibleTimer > 0f)
                _invincibleTimer -= Time.deltaTime;
        }

        public void TakeDamage(float amount, GameObject source = null)
        {
            if (IsDead || _invincibleTimer > 0f) return;
            _current = Mathf.Max(0f, _current - amount);
            _invincibleTimer = _invincibleDuration;
            EventBus.Emit(new DamageEvent { Source = source, Target = gameObject, Amount = amount });
            OnHealthChanged?.Invoke(_current, _maxHealth);
            if (_current <= 0f) Die();
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            _current = Mathf.Min(_maxHealth, _current + amount);
            OnHealthChanged?.Invoke(_current, _maxHealth);
        }

        public void SetMax(float max, bool refill = false)
        {
            _maxHealth = max;
            if (refill) _current = _maxHealth;
            OnHealthChanged?.Invoke(_current, _maxHealth);
        }

        void Die()
        {
            EventBus.Emit(new DeathEvent { Target = gameObject });
            OnDeath?.Invoke();
        }
    }
}
