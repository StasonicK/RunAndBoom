using NTC.Global.Cache;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace CodeBase.UI.Elements.Hud.MobileInputPanel.Joysticks
{
    public abstract class JoystickBase : MonoCache
    {
        [SerializeField] protected Vector2 JoystickSize = new Vector2(300, 300);
        [SerializeField] protected FloatingJoystick Joystick;

        protected Finger MovementFinger;
        public Vector2 Input = Vector2.zero;

        protected override void OnEnabled()
        {
            EnhancedTouchSupport.Enable();
            Touch.onFingerDown += HandleFingerDown;
            Touch.onFingerUp += HandleLoseFinger;
            Touch.onFingerMove += HandleFingerMove;
        }

        protected override void OnDisabled()
        {
            Touch.onFingerDown -= HandleFingerDown;
            Touch.onFingerUp -= HandleLoseFinger;
            Touch.onFingerMove -= HandleFingerMove;
            EnhancedTouchSupport.Disable();
        }

        private void HandleFingerMove(Finger movedFinger)
        {
            if (movedFinger == MovementFinger)
            {
                Vector2 knobPosition;
                float maxMovement = JoystickSize.x / 2f;
                Touch currentTouch = movedFinger.currentTouch;

                if (Vector2.Distance(currentTouch.screenPosition, Joystick.RectTransform.anchoredPosition) >
                    maxMovement)
                {
                    knobPosition = (
                                       currentTouch.screenPosition - Joystick.RectTransform.anchoredPosition
                                   ).normalized
                                   * maxMovement;
                }
                else
                {
                    knobPosition = currentTouch.screenPosition - Joystick.RectTransform.anchoredPosition;
                }

                Joystick.Knob.anchoredPosition = knobPosition;
                Input = knobPosition / maxMovement;
            }
        }

        private void HandleLoseFinger(Finger lostFinger)
        {
            if (lostFinger == MovementFinger)
            {
                MovementFinger = null;
                Joystick.Knob.anchoredPosition = Vector2.zero;
                Joystick.gameObject.SetActive(false);
                Input = Vector2.zero;
            }
        }

        protected Vector2 ClampStartPosition(Vector2 startPosition)
        {
            if (startPosition.x < JoystickSize.x / 2)
                startPosition.x = JoystickSize.x / 2;

            if (startPosition.y < JoystickSize.y / 2)
                startPosition.y = JoystickSize.y / 2;
            else if (startPosition.y > Screen.height - JoystickSize.y / 2)
                startPosition.y = Screen.height - JoystickSize.y / 2;

            return startPosition;
        }

        protected abstract void HandleFingerDown(Finger touchedFinger);
    }
}