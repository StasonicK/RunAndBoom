using System;
using UnityEngine;

namespace CodeBase.Services.Input
{
    public class DesktopInputService : InputService
    {
        private PlayerInput _playerInput;
        private KeyboardMovement _keyboardMovement;
        private MouseLook _mouseLook;
        private const string MouseX = "Mouse X";
        private const string MouseY = "Mouse Y";

        public override bool IsAttackButtonUp() => UnityEngine.Input.GetMouseButton(0);

        public override event Action<Vector2> Moved;
        public override event Action<Vector2> Looked;

        public DesktopInputService(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            _keyboardMovement = new KeyboardMovement(playerInput);
            _mouseLook = new MouseLook(playerInput);
            Subscribe();
        }

        private void Subscribe()
        {
            _playerInput.Enable();
            _keyboardMovement.Moved += MoveTo;
            _mouseLook.Looked += LookTo;
        }

        private void MoveTo(Vector2 direction) =>
            Moved?.Invoke(direction);

        private void LookTo(Vector2 direction) =>
            Looked?.Invoke(direction);

        public override Vector2 MoveAxis
        {
            get
            {
                Vector2 readValue = _playerInput.Player.Move.ReadValue<Vector2>();
                // Debug.Log($"move axis {readValue}");
                return readValue;
                // Vector2 axis = MoveSimpleInputAxis();
                //
                // if (axis == Vector2.zero)
                //     axis = UnityAxis();
                //
                // return axis;
            }
        }

        public override Vector2 LookAxis
        {
            get
            {
                Vector2 readValue = _playerInput.Player.Look.ReadValue<Vector2>();
                // Debug.Log($"look axis {readValue}");
                return readValue;
            }
            // get { return new(UnityEngine.Input.GetAxisRaw(MouseX), UnityEngine.Input.GetAxisRaw(MouseY)); }
        }

        private static Vector2 UnityAxis() =>
            new(UnityEngine.Input.GetAxis(Horizontal), UnityEngine.Input.GetAxis(Vertical));
    }
}