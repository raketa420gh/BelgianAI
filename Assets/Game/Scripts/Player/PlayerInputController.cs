using UnityEngine;
using UnityEngine.InputSystem;

namespace BelgianAI
{
    public class PlayerInputController : MonoBehaviour
    {
        public Vector3 MoveDirection => _moveDirection;

        private Vector3 _moveDirection;

        private void Update()
        {
            ReadInput();
        }

        private void ReadInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                _moveDirection = Vector3.zero;
                return;
            }

            float horizontal = 0f;
            float vertical = 0f;

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                vertical += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                vertical -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                horizontal += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                horizontal -= 1f;

            _moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        }
    }
}