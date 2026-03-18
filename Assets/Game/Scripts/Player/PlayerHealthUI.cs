using UnityEngine;

namespace BelgianAI
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [SerializeField] 
        private HealthComponent _health;

        private void OnGUI()
        {
            if (_health == null)
                return;

            float barWidth = 200f;
            float barHeight = 25f;
            float x = 10f;
            float y = 10f;

            float ratio = (float)_health.CurrentHealth / _health.MaxHealth;
            
            GUI.Box(new Rect(x, y, barWidth, barHeight), "");
            
            Color prevColor = GUI.color;
            GUI.color = Color.Lerp(Color.red, Color.green, ratio);
            GUI.Box(new Rect(x, y, barWidth * ratio, barHeight), "");
            GUI.color = prevColor;

            GUI.Label(new Rect(x + 5, y + 3, barWidth, barHeight),
                $"HP: {_health.CurrentHealth}/{_health.MaxHealth}");
        }
    }
}