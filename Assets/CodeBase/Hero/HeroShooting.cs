using System;
using System.Collections;
using CodeBase.Services;
using CodeBase.Services.Input;
using CodeBase.Services.PersistentProgress;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.Weapons;
using CodeBase.Weapons;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroShooting : MonoCache
    {
        [SerializeField] private HeroWeaponSelection _heroWeaponSelection;
        [SerializeField] private HeroReloading _heroReloading;

        private const float ShootDelay = 0.1f;

        private IPlayerProgressService _progressService;
        private IInputService _inputService;
        private HeroWeaponAppearance _heroWeaponAppearance;
        private bool _canShoot = false;

        public event Action Shot;

        private void Awake()
        {
            _inputService = AllServices.Container.Single<IInputService>();
            _progressService = AllServices.Container.Single<IPlayerProgressService>();
            _heroWeaponSelection.WeaponSelected += GetCurrentWeaponObject;
            _heroReloading.OnStopReloading += TurnOn;
        }

        protected override void Run() =>
            TryShoot();

        private void GetCurrentWeaponObject(GameObject weaponPrefab, HeroWeaponStaticData heroWeaponStaticData,
            TrailStaticData trailStaticData)
        {
            _heroWeaponAppearance = weaponPrefab.GetComponent<HeroWeaponAppearance>();
            TurnOn();
        }

        public void TurnOn() =>
            StartCoroutine(EnableShoot());

        public void TurnOff() =>
            _canShoot = false;

        private IEnumerator EnableShoot()
        {
            yield return new WaitForSeconds(ShootDelay);
            _canShoot = true;
        }

        private void TryShoot()
        {
            if (!_canShoot)
                return;

            if (_inputService.IsAttackButtonUp() && IsAvailableAmmo())
                Shoot();
        }

        private bool IsAvailableAmmo() =>
            _progressService.Progress.WeaponsData.WeaponsAmmoData.IsAmmoAvailable();

        private void Shoot()
        {
            Shot?.Invoke();
            TurnOff();
            _progressService.Progress.WeaponsData.WeaponsAmmoData.ReduceAmmo();
            _heroWeaponAppearance.ShootTo();
        }
    }
}