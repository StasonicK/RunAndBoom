using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Data.Perks;
using CodeBase.Services.Input;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Items;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroMovement : MonoBehaviour, IProgressReader
    {
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _groundYOffset = -0.1f;
        [SerializeField] float _gravity = -9.81f;

        private const float BaseRatio = 1f;

        private IStaticDataService _staticDataService;
        private IInputService _inputService;
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
        private FixedJoystick _joystick;
        private bool _isMobile = false;

        private void Awake() =>
            _characterController = GetComponent<CharacterController>();

        private void Update()
        {
            if (_inputService == null)
                return;

            Move();
            Gravity();
        }

        public void Construct(IStaticDataService staticDataService, IInputService inputService)
        {
            _staticDataService = staticDataService;
            _inputService = inputService;
            _isMobile = inputService is MobileInputService;
        }

        private void Move()
        {
            Vector3 airDirection = Vector3.zero;
            Vector3 direction = Vector3.zero;
            Vector3 moveVelocity = Vector3.zero;
            Vector3 moveInput = Vector3.zero;

            if (_isMobile!)
            {
                if (_inputService.MoveAxis.sqrMagnitude > Constants.MovementEpsilon)
                {
                    if (IsGrounded())
                        airDirection = transform.forward * _inputService.MoveAxis.y +
                                       transform.right * _inputService.MoveAxis.x;
                    else
                        direction = transform.forward * _inputService.MoveAxis.y +
                                    transform.right * _inputService.MoveAxis.x;
                }
            }
            else
            {
                moveVelocity = new Vector3(_joystick.Horizontal, Constants.Zero, _joystick.Vertical);
                moveInput = new Vector3(moveVelocity.x, Constants.Zero, moveVelocity.z);

                if (IsGrounded())
                    airDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
                else
                    direction = transform.forward * moveInput.y + transform.right * moveInput.x;
            }

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

        public void TurnOn() =>
            _canMove = true;

        public void TurnOff() =>
            _canMove = false;

        public void LoadProgress(PlayerProgress progress)
        {
            _progress = progress;
            _perks = _progress.PerksData.Perks;
            SetSpeed();
        }
    }
}