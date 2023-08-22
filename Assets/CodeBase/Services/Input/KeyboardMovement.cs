using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Services.Input
{
    public class KeyboardMovement
    {
        private PlayerInput _playerInput;

        public event Action<Vector2> Moved;

        public KeyboardMovement(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            OnEnable();
            Subscribe();
        }

        private void OnEnable()
        {
            _playerInput.Enable();
        }

        private void Subscribe()
        {
            _playerInput.Player.Move.performed += MoveStarted;
            _playerInput.Player.Move.canceled += MoveCanceled;
        }

        private void MoveStarted(InputAction.CallbackContext ctx)
        {
            Vector2 moveInput = _playerInput.Player.Move.ReadValue<Vector2>();
            Moved?.Invoke(moveInput);
        }

        private void MoveCanceled(InputAction.CallbackContext ctx) =>
            Moved?.Invoke(Vector2.zero);
    }
}