using System;
using UnityEngine;

namespace BelgianAI
{
    public sealed class HealthComponent: MonoBehaviour
    {
        public event Action OnHealthEnd;
        public event Action OnHealthMax;
        
        public int CurrentHealth => _currentHealth;
        
        [SerializeField]
        private int _maxHealth = 100;
        
        private int _currentHealth;

        public void Initialize(int maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public void ChangeHealth(int amount)
        {
            _currentHealth += amount;

            if (_currentHealth <= 0)
            {
                _currentHealth = 0;
                OnHealthEnd?.Invoke();
            }

            if (_currentHealth >= _maxHealth)
            {
                _currentHealth = _maxHealth;
                OnHealthMax?.Invoke();
            }
        }
    }
}