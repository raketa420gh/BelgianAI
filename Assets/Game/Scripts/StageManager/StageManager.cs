using System.Collections.Generic;
using UnityEngine;

namespace BelgianAI
{
    public class StageManager : IStageManager
    {
        private readonly Transform _playerTransform;
        private readonly int _gridCapacityMax;
        private readonly int _attackCapacityMax;

        private int _currentGridCapacity;
        private int _currentAttackCapacity;

        private List<GridSlot> _gridSlots;
        private Dictionary<IAttacker, GridSlot> _occupiedSlots = new();
        private Dictionary<IAttacker, Attack> _assignedAttacks = new();

        private List<IAttacker> _waitingAttackers = new();

        private readonly float _outerRadius = 3.5f;
        private readonly float _innerRadius = 2.0f;

        public StageManager(Transform playerTransform, int gridCapacityMax, int attackCapacityMax)
        {
            _playerTransform = playerTransform;
            _gridCapacityMax = gridCapacityMax;
            _attackCapacityMax = attackCapacityMax;
            _currentGridCapacity = gridCapacityMax;
            _currentAttackCapacity = attackCapacityMax;

            InitializeGridSlots();
        }

        private void InitializeGridSlots()
        {
            _gridSlots = new List<GridSlot>(8);
            
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8;
                Vector3 pos = _playerTransform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * _outerRadius;
                _gridSlots.Add(new GridSlot(pos));
            }
        }

        public bool RequestSlot(IAttacker attacker)
        {
            if (_occupiedSlots.ContainsKey(attacker))
                return true;

            if (attacker.GridWeight > _currentGridCapacity)
                return false;
            
            GridSlot bestSlot = null;
            float bestDistance = float.MaxValue;

            foreach (var slot in _gridSlots)
            {
                if (slot.IsOccupied) 
                    continue;

                float dist = Vector3.Distance(attacker.CurrentPosition, slot.WorldPosition);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    bestSlot = slot;
                }
            }

            if (bestSlot == null)
                return false;
            
            _occupiedSlots[attacker] = bestSlot;
            bestSlot.AssignOccupant(attacker);
            _currentGridCapacity -= attacker.GridWeight;

            attacker.OnSlotAssigned(bestSlot);
            return true;
        }

        public void ReleaseSlot(IAttacker attacker)
        {
            if (_occupiedSlots.TryGetValue(attacker, out var slot))
            {
                slot.Clear();
                _occupiedSlots.Remove(attacker);
                _currentGridCapacity += attacker.GridWeight;
                attacker.OnSlotReleased();
            }
        }

        public bool RequestAttack(IAttacker attacker, Attack attack)
        {
            if (!_occupiedSlots.ContainsKey(attacker))
                return false;

            if (attack.AttackWeight > _currentAttackCapacity)
                return false;

            _assignedAttacks[attacker] = attack;
            _currentAttackCapacity -= attack.AttackWeight;
            attacker.SetCurrentAttack(attack);
            return true;
        }

        public void ReleaseAttack(IAttacker attacker)
        {
            if (_assignedAttacks.TryGetValue(attacker, out var attack))
            {
                _currentAttackCapacity += attack.AttackWeight;
                _assignedAttacks.Remove(attacker);
                attacker.SetCurrentAttack(null);
            }
        }
        
        public void UpdateAssignments()
        {
            for (int i = 0; i < _gridSlots.Count; i++)
            {
                float angle = i * Mathf.PI * 2f / _gridSlots.Count;
                Vector3 pos = _playerTransform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * _outerRadius;
                _gridSlots[i] = new GridSlot(pos);
            }
        }
    }
}