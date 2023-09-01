using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Data.Perks;
using CodeBase.Services.Input;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Items;
using CodeBase.UI.Elements.Hud.MobileInputPanel.Joysticks;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroMovement : MonoCache, IProgressReader
    {
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private float _groundYOffset = -0.1f;
        [SerializeField] private float _gravity = -9.81f;

        private const float BaseRatio = 1f;

        private IStaticDataService _staticDataService;
        private IInputService _inputService;
        private MoveJoystick _moveJoystick;
        private CharacterController _characterController;
        private float _baseMovementSpeed = 5f;
        private float _movementRatio = 1f;
        private float _movementSpeed;
        private PerkItemData _runningItemData;
        private PlayerProgress _progress;
        private List<PerkItemData> _perks;
        private Vector3 _playerMovementInput;
        private float _airSpeed = 1.5f;
        private Vector3 _spherePosition;
        private Vector3 _velocity;
        private bool _canMove;
        private bool _update;
        private Vector2 _moveInput;
        private bool _isMobile;
        private Vector3 _direction = Vector3.zero;
        private Coroutine _coroutineMove;

        private void Awake() =>
            _characterController = Get<CharacterController>();

        protected override void Run()
        {
            if (_update == false)
                return;

            if (!_canMove)
                return;

            if (_isMobile)
                MobileMove();
            else
                DesktopMove();

            Gravity();
        }

        protected override void OnEnabled()
        {
            if (_runningItemData != null)
                _runningItemData.LevelChanged += ChangeSpeed;
        }

        protected override void OnDisabled()
        {
            if (_runningItemData != null)
                _runningItemData.LevelChanged -= ChangeSpeed;
        }

        public void ConstructDesktopPlatform(IStaticDataService staticDataService, DesktopInputService inputService)
        {
            _staticDataService = staticDataService;
            _inputService = inputService;
            _isMobile = false;
            _update = true;
            _inputService.Moved += DesktopMove;
        }

        public void ConstructMobilePlatform(IStaticDataService staticDataService, MoveJoystick moveJoystick)
        {
            _moveJoystick = moveJoystick;
            _staticDataService = staticDataService;
            _isMobile = true;
            _update = true;
        }

        private void DesktopMove(Vector2 moveInput) =>
            _moveInput = moveInput;

        private void DesktopMove()
        {
            _direction = transform.forward * _moveInput.y + transform.right * _moveInput.x;
            _characterController.Move((_direction.normalized * _movementSpeed) * Time.deltaTime);
        }

        private void MobileMove()
        {
            if (_moveJoystick.Input.sqrMagnitude <= Constants.MovementEpsilon)
                return;

            _direction = transform.forward * _moveJoystick.Input.y + transform.right * _moveJoystick.Input.x;
            _characterController.Move((_direction.normalized * _movementSpeed) * Time.deltaTime);
        }

        private void Gravity()
        {
            if (!IsGrounded())
                _velocity.y += _gravity * Time.deltaTime;
            else if (_velocity.y < 0)
                _velocity.y = -0.2f;

            _characterController.Move(_velocity * Time.deltaTime);
        }

        private bool IsGrounded()
        {
            _spherePosition = new Vector3(transform.position.x, transform.position.y - _groundYOffset,
                transform.position.z);

            if (Physics.CheckSphere(_spherePosition, _characterController.radius - 0.05f, _groundMask))
                return true;

            return false;
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