using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBase.Services.Input
{
    public interface IInputService : IService
    {
        public event Action<Vector2> Moved;
        public event Action<Vector2> Looked;
        public event Action Shot;
        public event Action OnLeaderBoardButtonClick;
        public event Action OnEscButtonClick;
        
        // bool IsAttackButtonUp();
        // bool IsLeaderBoardButtonUp();
        // bool IsEscButtonUp();
        void Shoot(InputAction.CallbackContext callbackContext);
    }
}