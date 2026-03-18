using UnityEngine;

namespace BelgianAI
{
    public class GridSlot
    {
        public Vector3 WorldPosition { get; private set; }
        public bool IsOccupied => _occupant != null;
        public IAttacker Occupant => _occupant;
        public bool IsLocked { get; private set; }

        private IAttacker _occupant;

        public GridSlot(Vector3 worldPosition)
        {
            WorldPosition = worldPosition;
        }

        public void UpdatePosition(Vector3 newPosition)
        {
            WorldPosition = newPosition;
        }

        public void AssignOccupant(IAttacker attacker)
        {
            _occupant = attacker;
        }

        public void Clear()
        {
            _occupant = null;
            IsLocked = false;
        }

        public void Lock()
        {
            IsLocked = true;
        }

        public void Unlock()
        {
            IsLocked = false;
        }
    }
}