using UnityEngine;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

namespace CodeBase.UI.Elements.Hud.MobileInputPanel
{
    public class MoveJoystick : MonoBehaviour
    {
        [SerializeField] private Vector2 JoystickSize = new Vector2(300, 300);
        [SerializeField] private FloatingJoystick Joystick;

        private ETouch.Finger MovementFinger;
        private Vector2 _moveInput = Vector2.zero;

        public Vector2 MoveInput => _moveInput;

        private void OnEnable()
        {
            ETouch.EnhancedTouchSupport.Enable();
            ETouch.Touch.onFingerDown += HandleFingerDown;
            ETouch.Touch.onFingerUp += HandleLoseFinger;
            ETouch.Touch.onFingerMove += HandleFingerMove;
        }

        private void OnDisable()
        {
            ETouch.Touch.onFingerDown -= HandleFingerDown;
            ETouch.Touch.onFingerUp -= HandleLoseFinger;
            ETouch.Touch.onFingerMove -= HandleFingerMove;
            ETouch.EnhancedTouchSupport.Disable();
        }

        private void HandleFingerMove(ETouch.Finger movedFinger)
        {
            if (movedFinger == MovementFinger)
            {
                Vector2 knobPosition;
                float maxMovement = JoystickSize.x / 2f;
                Debug.Log($"maxMovement {maxMovement}");
                ETouch.Touch currentTouch = movedFinger.currentTouch;

                float distance = Vector2.Distance(currentTouch.screenPosition, Joystick.RectTransform.anchoredPosition);
                Debug.Log($"distance {distance}");
                
                if (distance > maxMovement)
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

                Debug.Log($"knobPosition {knobPosition}");
                Joystick.Knob.anchoredPosition = knobPosition;
                _moveInput = knobPosition / maxMovement;
                Debug.Log($"move input {_moveInput}");
            }
        }

        private void HandleLoseFinger(ETouch.Finger lostFinger)
        {
            if (lostFinger == MovementFinger)
            {
                MovementFinger = null;
                Joystick.Knob.anchoredPosition = Vector2.zero;
                Joystick.gameObject.SetActive(false);
                _moveInput = Vector2.zero;
            }
        }

        private void HandleFingerDown(ETouch.Finger touchedFinger)
        {
            if (MovementFinger == null && touchedFinger.screenPosition.x <= Screen.width / 2f)
            {
                MovementFinger = touchedFinger;
                _moveInput = Vector2.zero;
                Joystick.gameObject.SetActive(true);
                Joystick.RectTransform.sizeDelta = JoystickSize;
                Joystick.RectTransform.anchoredPosition = ClampStartPosition(touchedFinger.screenPosition);
            }
        }

        private Vector2 ClampStartPosition(Vector2 startPosition)
        {
            if (startPosition.x < JoystickSize.x / 2)
                startPosition.x = JoystickSize.x / 2;

            if (startPosition.y < JoystickSize.y / 2)
                startPosition.y = JoystickSize.y / 2;
            else if (startPosition.y > Screen.height - JoystickSize.y / 2)
                startPosition.y = Screen.height - JoystickSize.y / 2;

            return startPosition;
        }

        private void Update()
        {
            // Vector3 scaledMovement = Player.speed * Time.deltaTime * new Vector3(
            //     _moveInput.x,
            //     0,
            //     _moveInput.y
            // );
            //
            // Player.transform.LookAt(Player.transform.position + scaledMovement, Vector3.up);
            // Player.Move(scaledMovement);
        }
    }
}