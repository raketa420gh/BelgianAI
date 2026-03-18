using UnityEngine;

namespace BelgianAI
{
    public class MoveComponent : MonoBehaviour
    {
        [SerializeField] 
        private float _moveSpeed = 5f;
        [SerializeField] 
        private float _rotationSpeed = 720f;

        public void Move(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f)
                return;

            transform.position += direction * (_moveSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime);
        }

        public void MoveTowards(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                return;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                _moveSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime);
        }

        public void SetSpeed(float speed)
        {
            _moveSpeed = speed;
        }
    }
}