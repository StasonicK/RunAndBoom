using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Services.Input
{
    public class MobileInputService : InputService
    {
        private PlayerInput _playerInput;

        public override event Action<Vector2> Moved;
        public override event Action<Vector2> Looked;
        public override event Action Shot;
        public override event Action OnLeaderBoardButtonClick;
        public override event Action OnEscButtonClick;

        // public override bool IsAttackButtonUp() =>
        //     _playerInput.Player.Shoot.IsPressed();

        // public override bool IsLeaderBoardButtonUp() =>
        //     _playerInput.Player.LeaderBoardWindow.IsPressed();

        // public override bool IsEscButtonUp() =>
        //     _playerInput.Player.ESC.IsPressed();

        public MobileInputService(PlayerInput playerInput)
        {
            _playerInput = playerInput;
            _playerInput.Enable();
            _playerInput.Player.Shoot.performed += Shoot;
            _playerInput.Player.LeaderBoardWindow.performed += OpenLeaderboardWindow;
            _playerInput.Player.ESC.performed += OpenEscWindow;
        }

        public override void Shoot(InputAction.CallbackContext callbackContext) =>
            Shot?.Invoke();

        protected override void OpenLeaderboardWindow(InputAction.CallbackContext callbackContext) =>
            OnLeaderBoardButtonClick?.Invoke();

        protected override void OpenEscWindow(InputAction.CallbackContext callbackContext) =>
            OnEscButtonClick?.Invoke();
    }
}