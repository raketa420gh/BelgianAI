using UnityEngine;

namespace BelgianAI
{
    public class GridSlot
    {
        public Vector3 WorldPosition => _position;
        public IAttacker Occupant => _occupant;
        public bool IsOccupied => _occupant != null;
        
        private readonly Vector3 _position;
        private IAttacker _occupant;

        public GridSlot(Vector3 worldPosition)
        {
           _position = worldPosition;
        }

        public void AssignOccupant(IAttacker attacker)
        {
            _occupant = attacker;
        }

        public void Clear()
        {
            _occupant = null;
        }
    }
}