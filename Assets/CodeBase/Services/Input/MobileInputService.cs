using UnityEngine;

namespace CodeBase.Services.Input
{
    public class MobileInputService : InputService
    {
        private const string Button = "Fire";

        public override bool IsAttackButtonUp() => SimpleInput.GetButtonDown(Button);

        public override Vector2 MoveAxis => MoveSimpleInputAxis();

        public override Vector2 LookAxis
        {
            get
            {
                Vector2 axis = LookSimpleInputAxis();

                Debug.Log($"LookSimpleInputAxis: {axis}");

                if (axis == Vector2.zero)
                {
                    axis = UnityAxis();

                    Debug.Log($"UnityAxis: {axis}");
                }

                return axis;
            }
        }

        private static Vector2 UnityAxis() =>
            new(UnityEngine.Input.GetAxis(Horizontal2), UnityEngine.Input.GetAxis(Vertical2));
    }
}