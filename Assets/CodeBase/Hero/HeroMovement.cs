using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Data.Perks;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Items;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroMovement : MonoBehaviour, IProgressReader
    {
        [SerializeField] private Transform _cameraTransform;

        private const float BaseRatio = 1f;
        private const KeyCode KeyCodeW = KeyCode.W;
        private const KeyCode KeyCodeS = KeyCode.S;
        private const KeyCode KeyCodeA = KeyCode.A;
        private const KeyCode KeyCodeD = KeyCode.D;

        private IStaticDataService _staticDataService;
        private float _baseMovementSpeed = 5f;
        private float _movementRatio = 1f;
        private float _movementSpeed;
        private bool _canMove;
        private PerkItemData _runningItemData;
        private PlayerProgress _progress;
        private List<PerkItemData> _perks;
        private Rigidbody _rigidbody;
        private Vector3 _playerMovementInput;
        private float _getForward;
        private float _getBack;
        private float _getLeft;
        private float _getRight;
        private Vector2 _moveHorizontalVector;
        private Vector2 _moveVerticalVector;
        private float _horizontalInput;

        private float _verticalInput;

        // private Transform _oriantation;
        private Vector3 _movementDirection;

        private void Awake() =>
            _rigidbody = GetComponent<Rigidbody>();

        private void Update()
        {
            // _playerMovementInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            // _horizontalInput = Input.GetAxisRaw("Horizontal");
            // _verticalInput = Input.GetAxisRaw("Vertical");

            // if (Input.GetKeyDown(KeyCodeW))
            //     _getForward = 1;
            // if (Input.GetKeyUp(KeyCodeW))
            //     _getForward = 0;
            // if (Input.GetKeyDown(KeyCodeS))
            //     _getBack = -1;
            // if (Input.GetKeyUp(KeyCodeS))
            //     _getBack = 0;
            // if (Input.GetKeyDown(KeyCodeA))
            //     _getLeft = -1;
            // if (Input.GetKeyUp(KeyCodeA))
            //     _getLeft = 0;
            // if (Input.GetKeyDown(KeyCodeD))
            //     _getRight = 1;
            // if (Input.GetKeyUp(KeyCodeD))
            //     _getRight = 0;

            // Move();
        }

        private void FixedUpdate()
        {
            Move();
        }

        private void Move()
        {
            // if (_getForward)
            //     _moveHorizontalVector = new Vector2(1, 0);
            // if (_getBack)
            //     _moveHorizontalVector = new Vector2(-1, 0);
            // if (_getLeft)
            //     _moveVerticalVector = new Vector2(1, 0);
            // if (_getRight)
            //     _moveVerticalVector = new Vector2(-1, 0);
            // if (!_getForward && !_getBack)
            //     _moveVerticalVector = new Vector2(0, 0);
            // if (!_getLeft && !_getRight)
            //     _moveVerticalVector = new Vector2(0, 0);

            // Vector3 moveVector = transform.TransformDirection(_getLeft + _getRight, 0f, _getForward + _getBack) *
            //                      _movementSpeed;
            // _rigidbody.velocity = new Vector3(moveVector.x, _rigidbody.velocity.y, moveVector.z);

            // Vector3 moveVector = transform.TransformDirection(_moveHorizontalVector.x, 0f, _moveVerticalVector.x) *
            //                      _movementSpeed;
            // _rigidbody.velocity = new Vector3(moveVector.x, _rigidbody.velocity.y, moveVector.z);
            // _rigidbody.velocity = new Vector3(, _rigidbody.velocity.y, ) *
            //                       _movementSpeed;

            // Vector3 moveVector = transform.TransformDirection(_playerMovementInput) * _movementSpeed;
            // _rigidbody.velocity = new Vector3(moveVector.x, _rigidbody.velocity.y, moveVector.z);

            // transform.forward = _cameraTransform.forward;

            // _movementDirection = _cameraTransform.forward * _verticalInput + _cameraTransform.right * _horizontalInput;
            // _rigidbody.velocity = new Vector3(_movementDirection.x, _rigidbody.velocity.y, _movementDirection.z).normalized *
            //                       _movementSpeed;

            Vector2 axis = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal")).normalized *
                           _movementSpeed;
            Vector3 forward = new Vector3(-_cameraTransform.right.z, 0.0f, _cameraTransform.right.x);
            Vector3 wishDirection =
                (forward * axis.x + _cameraTransform.right * axis.y + Vector3.up * _rigidbody.velocity.y);
            _rigidbody.velocity = wishDirection;
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

        public void Construct(IStaticDataService staticDataService)
        {
            _staticDataService = staticDataService;
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