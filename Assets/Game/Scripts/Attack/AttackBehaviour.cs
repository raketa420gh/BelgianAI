using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BelgianAI
{
    public class AttackBehaviour : MonoBehaviour, IAttacker
    {
        public int GridWeight => _gridWeight;
        public IReadOnlyList<Attack> AvailableAttacks => _availableAttacks;
        public Attack CurrentAttack => _currentAttack;
        public Vector3 CurrentPosition => transform.position;
        public bool IsAlive => _isAlive;

        [Header("Grid Settings")]
        [SerializeField]
        private int _gridWeight = 4;

        [Header("Attacks")]
        [SerializeField]
        private List<Attack> _availableAttacks = new();

        [Header("Movement")]
        [SerializeField]
        private float _approachSpeed = 3f;

        [SerializeField]
        private float _attackApproachSpeed = 5f;

        [Header("Visual")]
        [SerializeField]
        private Renderer _renderer;
        
        private StageManager _stageManager;
        private GridSlot _currentSlot;
        private Attack _currentAttack;
        private bool _hasSlot;
        private bool _isAttacking;
        private bool _isAlive = true;

        private Dictionary<Attack, float> _attackCooldowns = new();

        private Color _originalColor;
        private static readonly Color AttackingColor = Color.red;
        private static readonly Color ApproachingColor = Color.yellow;
        private static readonly Color WaitingColor = Color.gray;

        private void Start()
        {
            if (_renderer != null)
                _originalColor = _renderer.material.color;
            
            foreach (var attack in _availableAttacks)
                _attackCooldowns[attack] = 0f;

            if (_stageManager != null)
                _stageManager.RegisterAttacker(this);
        }

        private void OnDestroy()
        {
            if (_stageManager != null)
                _stageManager.UnregisterAttacker(this);
        }

        private void Update()
        {
            if (!_isAlive)
                return;

            UpdateCooldowns();

            if (!_hasSlot)
            {
                TryRequestSlot();
                MoveToWaitPosition();
                SetVisualState(WaitingColor);
                return;
            }

            if (!_isAttacking)
            {
                MoveToOuterSlot();
                SetVisualState(ApproachingColor);
                
                if (_currentSlot != null)
                {
                    float distToSlot = Vector3.Distance(transform.position, _currentSlot.WorldPosition);
                    if (distToSlot < 0.5f)
                    {
                        TryRequestAttack();
                    }
                }
            }
            else
            {
                MoveToInnerCircle();
                SetVisualState(AttackingColor);
            }
        }
        
        public void SetStageManager(StageManager stageManager)
        {
            _stageManager = stageManager;
            _stageManager.RegisterAttacker(this);
        }
        
        public void OnSlotAssigned(GridSlot slot)
        {
            _currentSlot = slot;
            _hasSlot = true;
        }

        public void OnSlotReleased()
        {
            _currentSlot = null;
            _hasSlot = false;
        }

        public void SetCurrentAttack(Attack attack)
        {
            _currentAttack = attack;
        }

        public void Kill()
        {
            _isAlive = false;
            if (_stageManager != null)
                _stageManager.UnregisterAttacker(this);
            gameObject.SetActive(false);
        }

        private void UpdateCooldowns()
        {
            var keys = new List<Attack>(_attackCooldowns.Keys);
            foreach (var key in keys)
            {
                if (_attackCooldowns[key] > 0f)
                    _attackCooldowns[key] -= Time.deltaTime;
            }
        }

        private void TryRequestSlot()
        {
            _hasSlot = _stageManager.RequestSlot(this);
        }

        private void TryRequestAttack()
        {
            foreach (var attack in _availableAttacks)
            {
                if (_attackCooldowns.TryGetValue(attack, out float cd) && cd > 0f)
                    continue;

                if (_stageManager.RequestAttack(this, attack))
                {
                    _isAttacking = true;
                    StartCoroutine(PerformAttackCoroutine());
                    return;
                }
            }
        }

        private IEnumerator PerformAttackCoroutine()
        {
            float elapsed = 0f;
            float duration = _currentAttack.Duration;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                MoveToInnerCircle();
                yield return null;
            }
            
            DealDamage();
            
            if (_currentAttack != null && _attackCooldowns.ContainsKey(_currentAttack))
                _attackCooldowns[_currentAttack] = _currentAttack.Cooldown;
            
            _isAttacking = false;
            _stageManager.ReleaseAttack(this);
            _stageManager.ReleaseSlot(this);
            _hasSlot = false;
            _currentSlot = null;
        }

        private void DealDamage()
        {
            if (_currentAttack == null)
                return;
            
            float dist = Vector3.Distance(transform.position, _stageManager.PlayerPosition);
            if (dist <= _stageManager.InnerRadius + 0.5f)
            {
                var player = FindObjectOfType<Player>();
                if (player != null)
                {
                    var health = player.GetComponent<HealthComponent>();
                    if (health != null)
                        health.ChangeHealth(-_currentAttack.Damage);
                }
            }
        }

        private void MoveToOuterSlot()
        {
            if (_currentSlot == null)
                return;

            transform.position = Vector3.MoveTowards(
                transform.position,
                _currentSlot.WorldPosition,
                _approachSpeed * Time.deltaTime);

            LookAtPlayer();
        }

        private void MoveToInnerCircle()
        {
            Vector3 dirToPlayer = (_stageManager.PlayerPosition - transform.position).normalized;
            Vector3 innerPos = _stageManager.PlayerPosition - dirToPlayer * _stageManager.InnerRadius;

            transform.position = Vector3.MoveTowards(
                transform.position,
                innerPos,
                _attackApproachSpeed * Time.deltaTime);

            LookAtPlayer();
        }

        private void MoveToWaitPosition()
        {
            Vector3 waitPos = _stageManager.GetWaitPosition(this);
            transform.position = Vector3.MoveTowards(
                transform.position,
                waitPos,
                _approachSpeed * Time.deltaTime);

            LookAtPlayer();
        }

        private void LookAtPlayer()
        {
            Vector3 lookDir = _stageManager.PlayerPosition - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }

        private void SetVisualState(Color color)
        {
            if (_renderer != null)
                _renderer.material.color = color;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_currentSlot != null)
            {
                Gizmos.color = _isAttacking ? Color.red : Color.cyan;
                Gizmos.DrawLine(transform.position, _currentSlot.WorldPosition);
                Gizmos.DrawWireSphere(_currentSlot.WorldPosition, 0.2f);
            }

            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2f,
                $"W:{_gridWeight} Slot:{_hasSlot} Atk:{_isAttacking}");
        }
#endif
    }
}