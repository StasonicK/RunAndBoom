using UnityEngine;

namespace CodeBase.Services.Input
{
    public class MobileInputService : InputService
    {
        private const string Button = "Fire";

        public override bool IsAttackButtonUp() => SimpleInput.GetButtonDown(Button);

        public override Vector2 MoveAxis => MoveSimpleInputAxis();

        public override Vector2 LookAxis => LookSimpleInputAxis();
    }
}