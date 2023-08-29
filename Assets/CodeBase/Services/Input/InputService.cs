using System;
using UnityEngine;

namespace CodeBase.Services.Input
{
    public abstract class InputService : IInputService
    {
        protected const string Horizontal = "Horizontal";
        protected const string Vertical = "Vertical";
        protected const string Horizontal2 = "Horizontal2";
        protected const string Vertical2 = "Vertical2";

        public abstract Vector2 MoveAxis { get; }

        public abstract Vector2 LookAxis { get; }

        public abstract bool IsAttackButtonUp();

        public abstract event Action<Vector2> Moved;
        public abstract event Action<Vector2> Looked;

        protected static Vector2 MoveSimpleInputAxis() =>
            new(SimpleInput.GetAxis(Horizontal), SimpleInput.GetAxis(Vertical));

        protected static Vector2 LookSimpleInputAxis() =>
            new(SimpleInput.GetAxis(Horizontal2), SimpleInput.GetAxis(Vertical2));
    }
}