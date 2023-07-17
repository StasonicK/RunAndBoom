using CodeBase.Services;
using CodeBase.Services.Input;
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
        private float _xAxisClamp = 0;
        private bool _canRotate = true;
        private float _verticalRotation;

        private void Awake() =>
            _inputService = AllServices.Container.Single<IInputService>();

        private void Start()
        {
            if (AllServices.Container.Single<IInputService>() is DesktopInputService)
                Cursor.lockState = CursorLockMode.Locked;
            else
                Cursor.lockState = CursorLockMode.Confined;
        }

        private void Update()
        {
            if (_canRotate)
                Rotate();
        }

        private void Rotate()
        {
            if (_inputService.LookAxis.sqrMagnitude > Constants.Epsilon)
            {
                RotateHorizontal();
                RotateVertical();
            }
        }

        private void RotateHorizontal() =>
            transform.Rotate(Vector3.up * _inputService.LookAxis.x * _horizontalSensitivity);

        private void RotateVertical()
        {
            _verticalRotation -= _inputService.LookAxis.y;
            float verticalAngle = _edgeAngle / _verticalSensitivity;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -verticalAngle, verticalAngle);
            _camera.transform.localRotation = Quaternion.Euler(_verticalRotation * _verticalSensitivity, 0, 0);
        }

        public void TurnOn() =>
            _canRotate = true;

        public void TurnOff() =>
            _canRotate = false;
    }
}