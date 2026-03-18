using System;
using UnityEngine;

namespace BelgianAI
{
    [Serializable]
    public class Attack
    {
        public string Name => _name;
        public int AttackWeight => _weight;
        public float Duration => _duration;
        public int Damage => _damage;
        public float Cooldown => _cooldown;

        [SerializeField] 
        private string _name = "Attack";
        [SerializeField] 
        private int _weight = 3;
        [SerializeField] 
        private float _duration = 1.0f;
        [SerializeField] 
        private int _damage = 10;
        [SerializeField] 
        private float _cooldown = 2.0f;
    }
}