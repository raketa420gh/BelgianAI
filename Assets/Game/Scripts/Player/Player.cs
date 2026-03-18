using UnityEngine;

namespace BelgianAI
{
    public class Player : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private HealthComponent _healthComponent;

        [SerializeField]
        private StageManager _stageManager;

        [SerializeField]
        private PlayerInputController _inputController;

        [SerializeField]
        private MoveComponent _moveComponent;
        
        [SerializeField]
        private int _maxHealth = 100;

        private void Start()
        {
            _healthComponent.Initialize(_maxHealth);
            _stageManager.Initialize();

            _healthComponent.OnHealthEnd += HandleDeath;
            _inputController.OnKillRequested += HandleKillRequested;
        }

        private void OnDestroy()
        {
            _healthComponent.OnHealthEnd -= HandleDeath;
            _inputController.OnKillRequested -= HandleKillRequested;
        }

        private void Update()
        {
            _moveComponent.Move(_inputController.MoveDirection);
        }

        private void HandleKillRequested()
        {
            if (_stageManager.TryKillRandomAttacker())
                Debug.Log("Enemy killed!");
            else
                Debug.Log("No enemies left to kill.");
        }

        private void HandleDeath()
        {
            Debug.Log("Player died!");
        }
    }
}