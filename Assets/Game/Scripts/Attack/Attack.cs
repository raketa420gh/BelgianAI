using System;

namespace BelgianAI
{
    [Serializable]
    public class Attack
    {
        public string Name => _name;
        public int AttackWeight => _weight;
        public float Duration => _duration;

        private string _name;
        private int _weight;
        private float _duration;
    }
}