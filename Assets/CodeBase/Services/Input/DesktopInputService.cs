using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Services.Input
{
    public class DesktopInputService : InputService
    {
        private PlayerInput _playerInput;
        private KeyboardMovement _keyboardMovement;
        private MouseLook _mouseLook;

        public override bool IsAttackButtonUp() => _playerInput.Player.Shoot.IsPressed();

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
    }
}