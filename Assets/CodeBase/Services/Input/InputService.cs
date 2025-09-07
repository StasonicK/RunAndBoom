using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Services.Input
{
    public abstract class InputService : IInputService
    {
        public abstract event Action<Vector2> Moved;
        public abstract event Action<Vector2> Looked;
        public abstract event Action Shot;
        public abstract event Action OnLeaderBoardButtonClick;
        public abstract event Action OnEscButtonClick;

        // public abstract bool IsAttackButtonUp();
        // public abstract bool IsLeaderBoardButtonUp();
        // public abstract bool IsEscButtonUp();
        public abstract void Shoot(InputAction.CallbackContext callbackContext);
        protected abstract void OpenLeaderboardWindow(InputAction.CallbackContext callbackContext);
        protected abstract void OpenEscWindow(InputAction.CallbackContext callbackContext);
    }
}