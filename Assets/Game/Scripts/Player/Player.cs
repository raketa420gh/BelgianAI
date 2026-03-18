using UnityEngine;

namespace BelgianAI
{
    public class Player : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] 
        private HealthComponent _healthComponent;
        [SerializeField] 
        private MoveComponent _moveComponent;
        [SerializeField] 
        private StageManager _stageManager;
        [SerializeField] 
        private PlayerInputController _inputController;
        
        [SerializeField]
        private int _maxHealth = 100;

        private void Start()
        {
            _healthComponent.Initialize(_maxHealth);
            _stageManager.Initialize();

            _healthComponent.OnHealthEnd += HandleDeath;
        }

        private void OnDestroy()
        {
            _healthComponent.OnHealthEnd -= HandleDeath;
        }

        private void Update()
        {
            _moveComponent.Move(_inputController.MoveDirection);
        }

        private void HandleDeath()
        {
            Debug.Log("Player died!");
        }
    }
}