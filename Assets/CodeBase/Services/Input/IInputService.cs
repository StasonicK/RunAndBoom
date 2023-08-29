using System;
using UnityEngine;

namespace CodeBase.Services.Input
{
    public interface IInputService : IService
    {
        Vector2 MoveAxis { get; }
        Vector2 LookAxis { get; }

        bool IsAttackButtonUp();


        public event Action<Vector2> Moved;
        public event Action<Vector2> Looked;
    }
}