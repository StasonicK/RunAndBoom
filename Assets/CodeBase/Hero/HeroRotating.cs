using CodeBase.Services.Input;
using CodeBase.UI.Elements.Hud.MobileInputPanel;
using CodeBase.UI.Elements.Hud.MobileInputPanel.Joysticks;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroRotating : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _desktopVerticalSensitivity = 1.0f;
        [SerializeField] private float _desktopHorizontalSensitivity = 50.0f;
        [SerializeField] private float _mobileVerticalSensitivity = 5.0f;
        [SerializeField] private float _mobileHorizontalSensitivity = 100.0f;
        [SerializeField] private float _edgeAngle = 85f;

        private IInputService _inputService;

        private LookJoystick _lookJoystick;
        private bool _isMobile;
        private bool _update;
        private float _xAxisClamp = 0;
        private bool _canRotate = true;
        private float _verticalRotation;
        private float _verticalAngle;
        private Vector2 _lookInput = Vector3.zero;

        public void ConstructDesktopPlatform(IInputService inputService)
        {
            _inputService = inputService;
            _isMobile = false;
            _update = true;
            _inputService.Looked += DesktopRotate;
        }

        private void DesktopRotate(Vector2 lookInput)
        {
            _lookInput = lookInput;
            // RotateVertical();
            // RotateHorizontal();
        }

        public void ConstructMobilePlatform(LookJoystick lookJoystick)
        {
            _lookJoystick = lookJoystick;
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

            if (!_canRotate)
                return;

            if (_isMobile)
                MobileRotate();
            else
                DesktopRotate();
        }

        private void DesktopRotate()
        {
            _verticalRotation -= _lookInput.y;
            ClampAngle();
            transform.Rotate(Vector3.up * _lookInput.x * _desktopHorizontalSensitivity * Time.deltaTime);
            _camera.transform.localRotation = Quaternion.Euler(_verticalRotation * _desktopVerticalSensitivity, 0, 0);
        }

        private void MobileRotate()
        {
            RotateVertical();
            RotateHorizontal();
        }

        private void RotateVertical()
        {
            if (_lookJoystick.Input.sqrMagnitude > Constants.RotationEpsilon)
                _verticalRotation -= _lookJoystick.Input.y;

            ClampAngle();
            _camera.transform.localRotation = Quaternion.Euler(_verticalRotation * _mobileVerticalSensitivity, 0, 0);
        }

        private void ClampAngle()
        {
            if (_isMobile)
                _verticalAngle = _edgeAngle / _mobileVerticalSensitivity;
            else
                _verticalAngle = _edgeAngle / _desktopVerticalSensitivity;
            
            _verticalRotation = Mathf.Clamp(_verticalRotation, -_verticalAngle, _verticalAngle);
        }

        private void RotateHorizontal()
        {
            if (_lookJoystick.Input.sqrMagnitude > Constants.RotationEpsilon)
                transform.Rotate(Vector3.up * _lookJoystick.Input.x * _mobileHorizontalSensitivity * Time.deltaTime);

            // transform.Rotate(Vector3.up * Constants.Zero * _horizontalSensitivity * Time.deltaTime);
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