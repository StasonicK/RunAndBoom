using UnityEngine;
using UnityEngine.AI;
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

        private void HandleFingerMove(ETouch.Finger MovedFinger)
        {
            if (MovedFinger == MovementFinger)
            {
                Vector2 knobPosition;
                float maxMovement = JoystickSize.x / 2f;
                ETouch.Touch currentTouch = MovedFinger.currentTouch;

                if (Vector2.Distance(
                        currentTouch.screenPosition,
                        Joystick.RectTransform.anchoredPosition
                    ) > maxMovement)
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
                _moveInput = knobPosition / maxMovement;
                Debug.Log($"move input {_moveInput}");
            }
        }

        private void HandleLoseFinger(ETouch.Finger LostFinger)
        {
            if (LostFinger == MovementFinger)
            {
                MovementFinger = null;
                Joystick.Knob.anchoredPosition = Vector2.zero;
                Joystick.gameObject.SetActive(false);
                _moveInput = Vector2.zero;
            }
        }

        private void HandleFingerDown(ETouch.Finger TouchedFinger)
        {
            if (MovementFinger == null && TouchedFinger.screenPosition.x <= Screen.width / 2f)
            {
                MovementFinger = TouchedFinger;
                _moveInput = Vector2.zero;
                Joystick.gameObject.SetActive(true);
                Joystick.RectTransform.sizeDelta = JoystickSize;
                Joystick.RectTransform.anchoredPosition = ClampStartPosition(TouchedFinger.screenPosition);
            }
        }

        private Vector2 ClampStartPosition(Vector2 StartPosition)
        {
            if (StartPosition.x < JoystickSize.x / 2)
                StartPosition.x = JoystickSize.x / 2;

            if (StartPosition.y < JoystickSize.y / 2)
                StartPosition.y = JoystickSize.y / 2;
            else if (StartPosition.y > Screen.height - JoystickSize.y / 2)
                StartPosition.y = Screen.height - JoystickSize.y / 2;

            return StartPosition;
        }

        private void Update()
        {
            // Vector3 scaledMovement = Player.speed * Time.deltaTime * new Vector3(
            //     _moveInput.x,
            //     0,
            //     MovementAmount.y
            // );
            //
            // Player.transform.LookAt(Player.transform.position + scaledMovement, Vector3.up);
            // Player.Move(scaledMovement);
        }
    }
}