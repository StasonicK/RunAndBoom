using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Services.Input
{
    public class MouseLook
    {
        private PlayerInput _playerInput;

        public event Action<Vector2> Looked;

        public MouseLook(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            Subscribe();
        }

        private void Subscribe()
        {
            _playerInput.Player.Look.performed += LookStarted;
            _playerInput.Player.Look.canceled += LookCanceled;
        }

        private void LookStarted(InputAction.CallbackContext ctx) =>
            Looked?.Invoke(ctx.ReadValue<Vector2>());

        private void LookCanceled(InputAction.CallbackContext obj) =>
            Looked?.Invoke(Vector2.zero);
    }
}