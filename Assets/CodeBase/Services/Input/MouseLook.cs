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
            _playerInput.Player.Look.started += LookStarted;
            _playerInput.Player.Look.canceled -= LookCanceled;
            // _playerInput.Player.Look.performed -= Look;
        }

        private void LookStarted(InputAction.CallbackContext ctx)
        {
            float lookInput = ctx.ReadValue<float>();
            // Looked?.Invoke(lookInput);
        }

        private void LookCanceled(InputAction.CallbackContext obj) =>
            Looked?.Invoke(Vector2.zero);
    }
}