using UnityEngine;

namespace BelgianAI
{
    public class Player : MonoBehaviour
    {
        [SerializeField]
        private HealthComponent _healthComponent;
        
        [SerializeField]
        private StageManager _stageManager;
    }
}