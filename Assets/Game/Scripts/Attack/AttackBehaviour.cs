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
        
        [SerializeField] 
        private int _gridWeight = 4;
        [SerializeField] 
        private List<Attack> _availableAttacks = new();
        
        private IStageManager _stageManager;
        private GridSlot _currentSlot;
        private Attack _currentAttack;
        private bool _hasSlot = false;
        private bool _isAttacking = false;

        //[Inject] 
        public void Construct(IStageManager stageManager)
        {
            _stageManager = stageManager;
        }

        private void Update()
        {
            if (!_hasSlot)
                TryRequestSlot();

            if (_hasSlot && !_isAttacking)
            {
                TryRequestAttack();
            }
            
            if (_hasSlot && _currentSlot != null)
            {
                MoveToSlot();
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
                if (_stageManager.RequestAttack(this, attack))
                {
                    _isAttacking = true;
                    StartCoroutine(PerformAttack());
                    break;
                }
            }
        }

        private System.Collections.IEnumerator PerformAttack()
        {
            yield return new WaitForSeconds(_currentAttack.Duration);

            _isAttacking = false;
            _stageManager.ReleaseAttack(this);
            _stageManager.ReleaseSlot(this);
            
            _hasSlot = false;
            _currentSlot = null;
        }

        public void OnSlotAssigned(GridSlot slot)
        {
            _currentSlot = slot;
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

        private void MoveToSlot()
        {
            if (_currentSlot == null) 
                return;
            
            // MOVE
            Vector3 targetPos = _currentSlot.WorldPosition;
            float speed = 3f;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        }
    }
}