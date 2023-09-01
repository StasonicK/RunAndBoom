using System;
using UnityEngine;

namespace CodeBase.Services.Input
{
    public class MobileInputService : InputService
    {
        private PlayerInput _playerInput;

        public override event Action<Vector2> Moved;
        public override event Action<Vector2> Looked;

        public override bool IsAttackButtonUp() => _playerInput.Player.Shoot.IsPressed();

        public MobileInputService(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            _playerInput.Enable();
        }
    }
}