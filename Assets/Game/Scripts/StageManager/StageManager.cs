using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BelgianAI
{
    public class StageManager : MonoBehaviour, IStageManager
    {
        [Header("References")]
        [SerializeField]
        private Transform _playerTransform;

        [Header("Grid Settings")]
        [SerializeField]
        private int _gridCapacityMax = 12;

        [SerializeField]
        private int _attackCapacityMax = 10;

        [SerializeField]
        private float _outerRadius = 5.0f;

        [SerializeField]
        private float _innerRadius = 2.5f;

        [SerializeField]
        private float _waitRadius = 8.0f;

        [SerializeField]
        private int _slotCount = 8;

        [Header("Gizmos")]
        [SerializeField]
        private bool _drawGizmos = true;

        [SerializeField]
        private Color _innerCircleColor = Color.red;

        [SerializeField]
        private Color _outerCircleColor = Color.yellow;

        [SerializeField]
        private Color _waitCircleColor = new Color(1f, 1f, 1f, 0.3f);

        [SerializeField]
        private Color _freeSlotColor = Color.green;

        [SerializeField]
        private Color _occupiedSlotColor = Color.red;

        [SerializeField]
        private Color _lockedSlotColor = Color.magenta;

        public float OuterRadius => _outerRadius;
        public float InnerRadius => _innerRadius;
        public Vector3 PlayerPosition => _playerTransform.position;

        private int _currentGridCapacity;
        private int _currentAttackCapacity;

        private List<GridSlot> _gridSlots;
        private readonly Dictionary<IAttacker, GridSlot> _slotAssignments = new();
        private readonly Dictionary<IAttacker, Attack> _attackAssignments = new();
        private readonly HashSet<IAttacker> _registeredAttackers = new();

        private void Update()
        {
            UpdateSlotPositions();
            ReassignSlots();
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos || _playerTransform == null)
                return;

            Vector3 center = _playerTransform.position;

            Gizmos.color = _innerCircleColor;
            DrawCircle(center, _innerRadius, 40);

            Gizmos.color = _outerCircleColor;
            DrawCircle(center, _outerRadius, 40);

            Gizmos.color = _waitCircleColor;
            DrawCircle(center, _waitRadius, 40);

            if (_gridSlots == null)
                return;

            foreach (var slot in _gridSlots)
            {
                if (slot.IsLocked)
                    Gizmos.color = _lockedSlotColor;
                else if (slot.IsOccupied)
                    Gizmos.color = _occupiedSlotColor;
                else
                    Gizmos.color = _freeSlotColor;

                Gizmos.DrawWireSphere(slot.WorldPosition, 0.3f);

                Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
                Gizmos.DrawLine(center, slot.WorldPosition);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_playerTransform == null) return;

            Vector3 labelPos = _playerTransform.position + Vector3.up * 3f;
            string info = $"Grid: {_currentGridCapacity}/{_gridCapacityMax}\n" +
                          $"Attack: {_currentAttackCapacity}/{_attackCapacityMax}\n" +
                          $"Assigned: {_slotAssignments?.Count ?? 0}";

            UnityEditor.Handles.Label(labelPos, info);
        }
#endif

        public void Initialize()
        {
            _currentGridCapacity = _gridCapacityMax;
            _currentAttackCapacity = _attackCapacityMax;
            _gridSlots = new List<GridSlot>(_slotCount);

            for (int i = 0; i < _slotCount; i++)
            {
                Vector3 pos = CalculateSlotPosition(i);
                _gridSlots.Add(new GridSlot(pos));
            }
        }

        public void RegisterAttacker(IAttacker attacker)
        {
            _registeredAttackers.Add(attacker);
        }

        public void UnregisterAttacker(IAttacker attacker)
        {
            ReleaseAttack(attacker);
            ReleaseSlot(attacker);
            _registeredAttackers.Remove(attacker);
        }

        public bool RequestSlot(IAttacker attacker)
        {
            if (_slotAssignments.ContainsKey(attacker))
                return true;

            if (attacker.GridWeight > _currentGridCapacity)
                return false;

            GridSlot bestSlot = FindClosestFreeSlot(attacker.CurrentPosition);
            if (bestSlot == null)
                return false;

            AssignSlotInternal(attacker, bestSlot);
            return true;
        }

        public void ReleaseSlot(IAttacker attacker)
        {
            if (!_slotAssignments.TryGetValue(attacker, out var slot))
                return;

            slot.Clear();
            _slotAssignments.Remove(attacker);
            _currentGridCapacity += attacker.GridWeight;
            attacker.OnSlotReleased();
        }

        public bool RequestAttack(IAttacker attacker, Attack attack)
        {
            if (!_slotAssignments.ContainsKey(attacker))
                return false;

            if (attack.AttackWeight > _currentAttackCapacity)
                return false;

            _attackAssignments[attacker] = attack;
            _currentAttackCapacity -= attack.AttackWeight;
            attacker.SetCurrentAttack(attack);

            if (_slotAssignments.TryGetValue(attacker, out var slot))
                slot.Lock();

            return true;
        }

        public void ReleaseAttack(IAttacker attacker)
        {
            if (!_attackAssignments.TryGetValue(attacker, out var attack))
                return;

            _currentAttackCapacity += attack.AttackWeight;
            _attackAssignments.Remove(attacker);
            attacker.SetCurrentAttack(null);

            if (_slotAssignments.TryGetValue(attacker, out var slot))
                slot.Unlock();
        }

        public Vector3 GetWaitPosition(IAttacker attacker)
        {
            GridSlot targetSlot = FindClosestFreeSlot(attacker.CurrentPosition);

            if (targetSlot == null)
            {
                float bestDist = float.MaxValue;
                foreach (var slot in _gridSlots)
                {
                    float dist = Vector3.Distance(attacker.CurrentPosition, slot.WorldPosition);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        targetSlot = slot;
                    }
                }
            }

            if (targetSlot == null)
                return attacker.CurrentPosition;

            Vector3 direction = (targetSlot.WorldPosition - _playerTransform.position).normalized;
            return _playerTransform.position + direction * _waitRadius;
        }

        private void UpdateSlotPositions()
        {
            for (int i = 0; i < _gridSlots.Count; i++)
            {
                Vector3 newPos = CalculateSlotPosition(i);
                _gridSlots[i].UpdatePosition(newPos);
            }
        }

        private void ReassignSlots()
        {
            var deadAttackers = _slotAssignments.Keys
                .Where(a => !a.IsAlive)
                .ToList();

            foreach (var dead in deadAttackers)
            {
                ReleaseAttack(dead);
                ReleaseSlot(dead);
                _registeredAttackers.Remove(dead);
            }

            var unlockedAssigned = _slotAssignments
                .Where(kvp => !kvp.Value.IsLocked)
                .Select(kvp => kvp.Key)
                .ToList();

            if (unlockedAssigned.Count == 0)
                return;

            foreach (var attacker in unlockedAssigned)
            {
                var slot = _slotAssignments[attacker];
                slot.Clear();
                _slotAssignments.Remove(attacker);
                _currentGridCapacity += attacker.GridWeight;
            }

            var pending = new List<IAttacker>(unlockedAssigned);

            pending.Sort((a, b) =>
            {
                float distA = Vector3.Distance(a.CurrentPosition, _playerTransform.position);
                float distB = Vector3.Distance(b.CurrentPosition, _playerTransform.position);
                return distA.CompareTo(distB);
            });

            foreach (var attacker in pending)
            {
                if (attacker.GridWeight > _currentGridCapacity)
                {
                    attacker.OnSlotReleased();
                    continue;
                }

                GridSlot bestSlot = FindClosestFreeSlot(attacker.CurrentPosition);
                if (bestSlot == null)
                {
                    attacker.OnSlotReleased();
                    continue;
                }

                AssignSlotInternal(attacker, bestSlot);
            }
        }

        private GridSlot FindClosestFreeSlot(Vector3 position)
        {
            GridSlot bestSlot = null;
            float bestDist = float.MaxValue;

            foreach (var slot in _gridSlots)
            {
                if (slot.IsOccupied)
                    continue;

                float dist = Vector3.Distance(position, slot.WorldPosition);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestSlot = slot;
                }
            }

            return bestSlot;
        }

        private void AssignSlotInternal(IAttacker attacker, GridSlot slot)
        {
            slot.AssignOccupant(attacker);
            _slotAssignments[attacker] = slot;
            _currentGridCapacity -= attacker.GridWeight;
            attacker.OnSlotAssigned(slot);
        }

        private Vector3 CalculateSlotPosition(int index)
        {
            float angle = index * Mathf.PI * 2f / _slotCount;
            return _playerTransform.position +
                   new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * _outerRadius;
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float step = 2f * Mathf.PI / segments;
            Vector3 prevPoint = center + new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)) * radius;

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * step;
                Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }
    }
}