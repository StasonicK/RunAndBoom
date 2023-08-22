using System;
using UnityEngine;

namespace CodeBase.Services.Input
{
    public class MobileInputService : InputService
    {
        private const string Button = "Fire";
        private PlayerInput _playerInput;

        public MobileInputService()
        {
            _playerInput = new PlayerInput();
        }

        public override bool IsAttackButtonUp() => SimpleInput.GetButtonDown(Button);
        public override event Action<Vector2> Moved;
        public override event Action<Vector2> Looked;

        public override Vector2 MoveAxis => MoveSimpleInputAxis();

        public override Vector2 LookAxis => LookSimpleInputAxis();
    }
}