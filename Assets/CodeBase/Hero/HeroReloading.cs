using System;
using System.Linq;
using CodeBase.Data;
using CodeBase.Data.Upgrades;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Items;
using CodeBase.StaticData.Items.Shop.WeaponsUpgrades;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.Weapons;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroReloading : MonoCache, IProgressReader
    {
        [SerializeField] private HeroWeaponSelection _heroWeaponSelection;
        [SerializeField] private HeroShooting _heroShooting;

        private const float BaseRatio = 1f;

        private IStaticDataService _staticDataService;
        private PlayerProgress _progress;
        private HeroWeaponTypeId _weaponTypeId;
        private float _currentAttackCooldown = 0f;
        private float _initialCooldown = 2f;
        private float _baseCooldown = 0f;
        private float _cooldown = 0f;
        private float _reloadingRatio = 0f;
        private bool _startReloaded;
        private UpgradeItemData _reloadingItemData;
        private bool _canReload;
        private bool _reloadingStoped;

        public Action<float> OnStartReloading;
        public Action OnStopReloading;

        private void Awake()
        {
            _heroWeaponSelection.WeaponSelected += GetCurrentWeaponObject;
            _heroShooting.Shot += StartCooldown;
        }

        protected override void OnEnabled()
        {
            if (_reloadingItemData != null)
                _reloadingItemData.LevelChanged += ChangeCooldown;
        }

        protected override void OnDisabled()
        {
            if (_reloadingItemData != null)
                _reloadingItemData.LevelChanged -= ChangeCooldown;
        }

        protected override void Run()
        {
            if (_canReload)
            {
                UpdateInitialCooldown();
                UpdateCooldown();
            }
        }

        public void Construct(IStaticDataService staticDataService) =>
            _staticDataService = staticDataService;

        public void TurnOn() =>
            _canReload = true;

        public void TurnOff() =>
            _canReload = false;

        private void SetCooldown()
        {
            _reloadingItemData = _progress.WeaponsData.UpgradesData.UpgradeItemDatas.First(x =>
                x.WeaponTypeId == _weaponTypeId && x.UpgradeTypeId == UpgradeTypeId.Reloading);
            _reloadingItemData.LevelChanged += ChangeCooldown;
            ChangeCooldown();
        }

        private void ChangeCooldown()
        {
            _reloadingItemData = _progress.WeaponsData.UpgradesData.UpgradeItemDatas.First(x =>
                x.WeaponTypeId == _weaponTypeId && x.UpgradeTypeId == UpgradeTypeId.Reloading);

            if (_reloadingItemData.LevelTypeId == LevelTypeId.None)
                _reloadingRatio = BaseRatio;
            else
                _reloadingRatio = _staticDataService
                    .ForUpgradeLevelsInfo(_reloadingItemData.UpgradeTypeId, _reloadingItemData.LevelTypeId).Value;

            _cooldown = _baseCooldown * _reloadingRatio;
        }

        private void StartCooldown() =>
            _currentAttackCooldown = _baseCooldown;

        private void GetCurrentWeaponObject(GameObject weaponPrefab, HeroWeaponStaticData heroWeaponStaticData,
            TrailStaticData trailStaticData)
        {
            _weaponTypeId = heroWeaponStaticData.WeaponTypeId;
            _baseCooldown = heroWeaponStaticData.Cooldown;
            _currentAttackCooldown = 0f;

            if (_progress != null)
                SetCooldown();
        }

        private void UpdateCooldown()
        {
            if (!CooldownUp())
            {
                OnStartReloading?.Invoke(_currentAttackCooldown / _cooldown);
                _currentAttackCooldown -= Time.deltaTime;
                _reloadingStoped = false;
            }
            else
            {
                if (_reloadingStoped == false)
                {
                    OnStopReloading?.Invoke();
                    _reloadingStoped = true;
                }
            }
        }

        private bool CooldownUp() =>
            _currentAttackCooldown <= 0;

        private bool InitialCooldownUp() =>
            _initialCooldown <= 0f;

        private void UpdateInitialCooldown()
        {
            if (InitialCooldownUp() && !_startReloaded)
            {
                OnStopReloading?.Invoke();
                _startReloaded = true;
            }

            _initialCooldown -= Time.deltaTime;
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _progress = progress;
            SetCooldown();
        }
    }
}