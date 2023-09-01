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
            Subscribe();
        }


        private void Subscribe()
        {
            _playerInput.Player.Move.performed += MoveStarted;
            _playerInput.Player.Move.canceled += MoveCanceled;
        }

        private void MoveStarted(InputAction.CallbackContext ctx) =>
            Moved?.Invoke(ctx.ReadValue<Vector2>());

        private void MoveCanceled(InputAction.CallbackContext ctx) =>
            Moved?.Invoke(Vector2.zero);
    }
}