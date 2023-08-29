using CodeBase.Services.Input;
using CodeBase.UI.Elements.Hud.MobileInputPanel;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroRotating : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _verticalSensitivity = 2.0f;
        [SerializeField] private float _horizontalSensitivity = 2.0f;
        [SerializeField] private float _edgeAngle = 85f;

        private IInputService _inputService;
        private VariableJoystick _joystick;
        private LookByTouch _lookByTouch;
        private bool _isMobile = false;
        private bool _update;
        private float _xAxisClamp = 0;
        private bool _canRotate = true;
        private float _verticalRotation;

        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
            _isMobile = false;

            _update = true;
        }

        // public void Construct(MobileInput mobileInput)
        public void Construct(LookByTouch lookByTouch)
        {
            _lookByTouch = lookByTouch;
            // _joystick = mobileInput.LookJoystick;
            _isMobile = true;

            _update = true;
        }

        private void Start()
        {
            if (_inputService is DesktopInputService)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.Confined;
        }

        private void Update()
        {
            if (_update == false)
                return;

            if (_canRotate)
                Rotate();
        }

        private void Rotate()
        {
            // RotateVertical();
            // RotateHorizontal();
        }

        private void RotateVertical()
        {
            if (_isMobile == false)
            {
                if (_inputService.LookAxis.sqrMagnitude <= Constants.RotationEpsilon)
                    return;

                _verticalRotation -= _inputService.LookAxis.y;
            }
            else
            {
                if (_lookByTouch.JoystickVec.sqrMagnitude > Constants.RotationEpsilon)
                    _verticalRotation -= _lookByTouch.JoystickVec.y;
                // if (_joystick.Magnitude <= Constants.MovementEpsilon)
                //     return;
                //
                // _verticalRotation -= _joystick.Vertical;
            }

            ClampAngle();

            _camera.transform.localRotation =
                Quaternion.Euler(_verticalRotation * _verticalSensitivity, 0, 0);
        }

        private void ClampAngle()
        {
            float verticalAngle = _edgeAngle / _verticalSensitivity;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -verticalAngle, verticalAngle);
        }

        private void RotateHorizontal()
        {
            if (_isMobile == false)
            {
                if (_inputService.LookAxis.sqrMagnitude > Constants.RotationEpsilon)
                    transform.Rotate(Vector3.up * _inputService.LookAxis.x * _horizontalSensitivity * Time.deltaTime);
            }
            else
            {
                if (_lookByTouch.JoystickVec.sqrMagnitude > Constants.RotationEpsilon)
                    transform.Rotate(Vector3.up * _lookByTouch.JoystickVec.x * _horizontalSensitivity * Time.deltaTime);
                // if (_joystick.Magnitude > Constants.RotationEpsilon)
                // transform.Rotate(Vector3.up * _joystick.Horizontal * _horizontalSensitivity * Time.deltaTime);
            }

            transform.Rotate(Vector3.up * Constants.Zero * _horizontalSensitivity * Time.deltaTime);
        }

        public void TurnOn() =>
            _canRotate = true;

        public void TurnOff()
        {
            _canRotate = false;
            transform.Rotate(Vector3.zero);
        }
    }
}