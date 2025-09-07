using CodeBase.Services.Input;
using CodeBase.Services.PersistentProgress;
using CodeBase.UI.Elements.Hud.MobileInputPanel.Joysticks;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroRotating : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Camera _camera;

        [Header("Sensitivity")]
        [SerializeField] private float _desktopBaseSensitivity = 5.0f;
        [SerializeField] private float _mobileBaseSensitivity = 100.0f;

        [Header("Clamp")]
        [SerializeField] private float _edgeAngle = 85f;

        private IInputService _inputService;
        private IPlayerProgressService _playerProgressService;
        private LookJoystick _lookJoystick;

        private bool _isMobile;
        private bool _update;
        private bool _canRotate = true;

        private float _verticalRotation;
        private Vector2 _lookInput = Vector2.zero;
        private float _sensitivity;

        public void ConstructDesktopPlatform(IInputService inputService, IPlayerProgressService playerProgressService)
        {
            _inputService = inputService;
            _playerProgressService = playerProgressService;
            _isMobile = false;
            _update = true;

            _inputService.Looked += OnDesktopLook;
            _playerProgressService.SettingsData.AimSensitiveMultiplierChanged += UpdateSensitivity;

            UpdateSensitivity();
        }

        public void ConstructMobilePlatform(LookJoystick lookJoystick, IPlayerProgressService playerProgressService)
        {
            _lookJoystick = lookJoystick;
            _playerProgressService = playerProgressService;
            _isMobile = true;
            _update = true;

            _playerProgressService.SettingsData.AimSensitiveMultiplierChanged += UpdateSensitivity;

            UpdateSensitivity();
        }

        private void OnDesktopLook(Vector2 lookInput) =>
            _lookInput = lookInput;

        private void UpdateSensitivity()
        {
            float baseSensitivity = _isMobile ? _mobileBaseSensitivity : _desktopBaseSensitivity;
            _sensitivity = _playerProgressService.SettingsData.AimSensitiveMultiplier * baseSensitivity;
        }

        private void Start()
        {
            TurnOff();

            Cursor.lockState = _inputService is DesktopInputService
                ? CursorLockMode.Locked
                : CursorLockMode.Confined;
        }

        private void Update()
        {
            if (!_update || !_canRotate)
                return;

            if (_isMobile)
                RotateMobile();
            else
                RotateDesktop();
        }

        private void RotateDesktop()
        {
            transform.Rotate(Vector3.up * _lookInput.x * _sensitivity * Time.deltaTime);
            _verticalRotation -= _lookInput.y * _sensitivity * Time.deltaTime;

            ClampVerticalRotation();
            _camera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        }

        private void RotateMobile()
        {
            if (_lookJoystick.Input.sqrMagnitude <= Constants.RotationEpsilon)
                return;

            transform.Rotate(Vector3.up * _lookJoystick.Input.x * _sensitivity * Time.deltaTime);
            _verticalRotation -= _lookJoystick.Input.y * _sensitivity * Time.deltaTime;

            ClampVerticalRotation();
            _camera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        }

        private void ClampVerticalRotation()
        {
            _verticalRotation = Mathf.Clamp(_verticalRotation, -_edgeAngle, _edgeAngle);
        }

        public void TurnOn() =>
            _canRotate = true;

        public void TurnOff()
        {
            _canRotate = false;
            _lookInput = Vector2.zero;
        }
    }
}
