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

        [Header("Movement")]
        [SerializeField] 
        private float _moveSpeed = 5f;
        
        [Header("Health")]
        [SerializeField] 
        private int _maxHealth = 100;
        
        private void OnEnable()
        {
            _healthComponent.Initialize(_maxHealth);
            _stageManager.Initialize();

            _healthComponent.OnHealthEnd += HandleDeath;
        }

        private void OnDisable()
        {
            _healthComponent.OnHealthEnd -= HandleDeath;
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(h, 0f, v).normalized;

            if (direction.sqrMagnitude > 0.01f)
            {
                transform.position += direction * (_moveSpeed * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private void HandleDeath()
        {
            Debug.Log("Player died!");
        }
    }
}