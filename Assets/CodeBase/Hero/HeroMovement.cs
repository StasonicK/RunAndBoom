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

        private IStaticDataService _staticDataService;
        private float _baseMovementSpeed = 5f;
        private float _movementRatio = 1f;
        private float _movementSpeed;
        private bool _canMove;
        private PerkItemData _runningItemData;
        private PlayerProgress _progress;
        private List<PerkItemData> _perks;
        private Rigidbody _rigidbody;

        private void Awake() =>
            _rigidbody = GetComponent<Rigidbody>();

        private void Update()
        {
            Move();
        }

        private void FixedUpdate()
        {
            // Move();
        }

        private void Move()
        {
            Vector2 axis = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal")).normalized *
                           _movementSpeed;
            Vector3 forward = new Vector3(-_cameraTransform.right.z, 0.0f, _cameraTransform.right.x);
            Vector3 wishDirection =
                (forward * axis.x + _cameraTransform.right * axis.y + Vector3.up * _rigidbody.velocity.y).normalized *
                _movementSpeed;
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