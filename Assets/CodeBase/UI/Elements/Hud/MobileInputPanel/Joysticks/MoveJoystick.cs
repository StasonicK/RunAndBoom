using UnityEngine;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

namespace CodeBase.UI.Elements.Hud.MobileInputPanel.Joysticks
{
    public class MoveJoystick : JoystickBase
    {
        protected override void HandleFingerDown(ETouch.Finger touchedFinger)
        {
            if (MovementFinger == null && touchedFinger.screenPosition.x <= Screen.width / 2f)
            {
                MovementFinger = touchedFinger;
                Input = Vector2.zero;
                Joystick.gameObject.SetActive(true);
                Joystick.RectTransform.sizeDelta = JoystickSize;
                Joystick.RectTransform.anchoredPosition = ClampStartPosition(touchedFinger.screenPosition);
            }
        }
    }
}