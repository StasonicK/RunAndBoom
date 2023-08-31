using System;
using UnityEngine;

namespace CodeBase.Services.Input
{
    public class MobileInputService : InputService
    {
        private PlayerInput _playerInput;

        public MobileInputService(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            _playerInput.Enable();
        }

        public override bool IsAttackButtonUp() => _playerInput.Player.Shoot.IsPressed();
        public override event Action<Vector2> Moved;
        public override event Action<Vector2> Looked;
    }
}