using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Data.Perks;
using CodeBase.Services.Input;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Items;
using CodeBase.UI.Elements.Hud.MobileInputPanel;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace CodeBase.Hero
{
    public class HeroMovement : MonoBehaviour, IProgressReader
    {
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _groundYOffset = -0.1f;
        [SerializeField] private float _gravity = -9.81f;

        private const float BaseRatio = 1f;

        private IStaticDataService _staticDataService;
        private IInputService _inputService;
        private MoveJoystick _moveJoystick;
        private bool _isMobile = false;
        private CharacterController _characterController;
        private float _baseMovementSpeed = 5f;
        private float _movementRatio = 1f;
        private float _movementSpeed;
        private bool _canMove;
        private PerkItemData _runningItemData;
        private PlayerProgress _progress;
        private List<PerkItemData> _perks;
        private Vector3 _playerMovementInput;
        private float _airSpeed = 1.5f;
        private Vector3 _spherePosition;
        private Vector3 _velocity;
        private bool _update;
        private Vector2 _moveInput;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (_update == false)
                return;

            // Move();
            Gravity();
        }

        public void ConstructDesktopPlatform(IStaticDataService staticDataService, DesktopInputService inputService)
        {
            _staticDataService = staticDataService;
            _inputService = inputService;
            _isMobile = false;
            _update = true;
            _inputService.Moved += Move;
        }

        private void Move(Vector2 moveInput)
        {
            _moveInput = moveInput;
            // Vector3 airDirection = Vector3.zero;
            // Vector3 mainDirection = Vector3.zero;
            // Debug.Log($"magnitude {direction.sqrMagnitude}");
            // if (direction.sqrMagnitude <= Constants.MovementEpsilon)
            //     return;
            //
            // if (IsGrounded())
            //     airDirection = transform.forward * _inputService.MoveAxis.y +
            //                    transform.right * _inputService.MoveAxis.x;
            // else
            //     mainDirection = transform.forward * _inputService.MoveAxis.y +
            //                     transform.right * _inputService.MoveAxis.x;
            //
            // _characterController.Move((mainDirection.normalized * _movementSpeed + airDirection * _airSpeed) *
            //                           Time.deltaTime);
        }

        public void ConstructMobilePlatform(IStaticDataService staticDataService, MoveJoystick moveJoystick)
        {
            _moveJoystick = moveJoystick;
            _staticDataService = staticDataService;
            _isMobile = true;
            _update = true;
        }

        private void Move()
        {
            Vector3 airDirection = Vector3.zero;
            Vector3 direction = Vector3.zero;

            if (_isMobile == false)
            {
                // if (_moveInput.sqrMagnitude <= Constants.MovementEpsilon)
                //     return;
                //
                // if (IsGrounded())
                //     airDirection = transform.forward * _moveInput.y +
                //                    transform.right * _moveInput.x;
                // else
                //     direction = transform.forward * _moveInput.y +
                //                 transform.right * _moveInput.x;
                Debug.Log($"move magnitude {_inputService.MoveAxis.sqrMagnitude}");
            }
            else
            {
                Debug.Log($"move magnitude {_moveJoystick.MoveInput.sqrMagnitude}");

                if (_moveJoystick.MoveInput.sqrMagnitude <= Constants.MovementEpsilon)
                    return;

                if (IsGrounded())
                    airDirection = transform.forward * _moveJoystick.MoveInput.y +
                                   transform.right * _moveJoystick.MoveInput.x;
                else
                    direction = transform.forward * _moveJoystick.MoveInput.y +
                                transform.right * _moveJoystick.MoveInput.x;
            }

            Debug.Log($"move input {_moveInput}");
            Debug.Log($"move direction {direction}");
            _characterController.Move((direction.normalized * _movementSpeed + airDirection * _airSpeed) *
                                      Time.deltaTime);
        }

        private bool IsGrounded()
        {
            _spherePosition = new Vector3(transform.position.x, transform.position.y - _groundYOffset,
                transform.position.z);

            if (Physics.CheckSphere(_spherePosition, _characterController.radius - 0.05f, _groundMask))
                return true;

            return false;
        }

        private void Gravity()
        {
            if (!IsGrounded())
                _velocity.y += _gravity * Time.deltaTime;
            else if (_velocity.y < 0)
                _velocity.y = -0.2f;

            _characterController.Move(_velocity * Time.deltaTime);
        }

        private void OnEnable()
        {
            if (_runningItemData != null)
                _runningItemData.LevelChanged += ChangeSpeed;
        }

        private void OnDisable()
        {
            if (_runningItemData != null)
                _runningItemData.LevelChanged -= ChangeSpeed;
        }

        private void SetSpeed()
        {
            _runningItemData = _perks.Find(x => x.PerkTypeId == PerkTypeId.Running);
            _runningItemData.LevelChanged += ChangeSpeed;
            ChangeSpeed();
        }

        private void ChangeSpeed()
        {
            if (_runningItemData.LevelTypeId == LevelTypeId.None)
                _movementRatio = BaseRatio;
            else
                _movementRatio = _staticDataService.ForPerk(PerkTypeId.Running, _runningItemData.LevelTypeId).Value;

            _movementSpeed = _baseMovementSpeed * _movementRatio;
        }

        public void TurnOn()
        {
            EnhancedTouchSupport.Enable();
            Touch.onFingerDown += HandleFingerDown;
            Touch.onFingerUp += HandleFingerUp;
            Touch.onFingerMove += HandleFingerMove;
            _canMove = true;
        }

        public void TurnOff()
        {
            EnhancedTouchSupport.Disable();
            Touch.onFingerDown -= HandleFingerDown;
            Touch.onFingerUp -= HandleFingerUp;
            Touch.onFingerMove -= HandleFingerMove;
            _canMove = false;
        }

        private void HandleFingerDown(Finger obj)
        {
        }

        private void HandleFingerUp(Finger obj)
        {
        }

        private void HandleFingerMove(Finger obj)
        {
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _progress = progress;
            _perks = _progress.PerksData.Perks;
            SetSpeed();
        }
    }
}