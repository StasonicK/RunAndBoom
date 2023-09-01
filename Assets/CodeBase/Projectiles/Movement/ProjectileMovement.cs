using System;
using System.Collections;
using System.Linq;
using CodeBase.Data;
using CodeBase.Data.Upgrades;
using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Items;
using CodeBase.StaticData.Items.Shop.WeaponsUpgrades;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.Weapons;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Projectiles.Movement
{
    public abstract class ProjectileMovement : MonoCache
    {
        private const float BaseRatio = 1f;

        private IStaticDataService _staticDataService;
        private float _movementTimeLimit = 3f;
        private float _launchCounter = 0f;
        private float _baseSpeed;
        private UpgradeItemData _speedItemData;
        private float _speedRatio = BaseRatio;
        private HeroWeaponTypeId? _weaponTypeId;
        private PlayerProgress _progress;

        [HideInInspector] public float Speed { get; private set; }
        protected bool IsMove { get; set; }

        public abstract event Action Stoped;

        protected override void OnEnabled()
        {
            if (_speedItemData != null)
                _speedItemData.LevelChanged += ChangeSpeed;
        }

        protected override void OnDisabled()
        {
            if (_speedItemData != null)
                _speedItemData.LevelChanged -= ChangeSpeed;
        }

        public void Construct(ProjectileTypeId projectileTypeId)
        {
            _progress = AllServices.Container.Single<IPlayerProgressService>().Progress;
            _staticDataService = AllServices.Container.Single<IStaticDataService>();
            ProjectileStaticData projectileStaticData = _staticDataService.ForProjectile(projectileTypeId);
            Speed = projectileStaticData.Speed;
            _baseSpeed = projectileStaticData.Speed;
            _movementTimeLimit = projectileStaticData.MovementLifeTime;
            IsMove = false;

            if (_weaponTypeId != null)
                SetSpeed();
        }

        public void Construct(ProjectileTypeId projectileTypeId, HeroWeaponTypeId heroWeaponTypeId)
        {
            _weaponTypeId = heroWeaponTypeId;
            Construct(projectileTypeId);
        }

        private void SetSpeed()
        {
            _speedItemData = _progress.WeaponsData.UpgradesData.UpgradeItemDatas.First(x =>
                x.WeaponTypeId == _weaponTypeId && x.UpgradeTypeId == UpgradeTypeId.Speed);
            _speedItemData.LevelChanged += ChangeSpeed;
            ChangeSpeed();
        }

        private void ChangeSpeed()
        {
            _speedItemData = _progress.WeaponsData.UpgradesData.UpgradeItemDatas.First(x =>
                x.WeaponTypeId == _weaponTypeId && x.UpgradeTypeId == UpgradeTypeId.Speed);

            if (_speedItemData.LevelTypeId == LevelTypeId.None)
                _speedRatio = BaseRatio;
            else
                _speedRatio = _staticDataService
                    .ForUpgradeLevelsInfo(_speedItemData.UpgradeTypeId, _speedItemData.LevelTypeId).Value;

            Speed = _baseSpeed * _speedRatio;
        }

        public abstract void Launch();
        public abstract void Stop();

        protected void OffMove() =>
            IsMove = false;

        protected IEnumerator LaunchTime()
        {
            _launchCounter = _movementTimeLimit;
            IsMove = true;

            while (_launchCounter > 0f)
            {
                _launchCounter -= Time.deltaTime;
                yield return null;
            }

            if (_launchCounter <= 0f)
            {
                Stop();
            }
        }
    }
}