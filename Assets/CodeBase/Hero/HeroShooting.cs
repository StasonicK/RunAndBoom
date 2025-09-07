using System;
using System.Collections;
using CodeBase.Services;
using CodeBase.Services.Input;
using CodeBase.Services.PersistentProgress;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.Weapons;
using CodeBase.Weapons;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroShooting : MonoBehaviour
    {
        [SerializeField] private HeroWeaponSelection _heroWeaponSelection;
        [SerializeField] private HeroReloading _heroReloading;

        private const float ShootDelay = 0.1f;

        private IPlayerProgressService _progressService;
        private IInputService _inputService;
        private HeroWeaponAppearance _heroWeaponAppearance;
        private bool _canShootTemp;
        private bool _canShoot;
        private bool _isInitial = true;
        private WaitForSeconds _waitForSeconds;

        public event Action Shot;

        private void Awake()
        {
            _inputService = AllServices.Container.Single<IInputService>();
            _progressService = AllServices.Container.Single<IPlayerProgressService>();
            _heroWeaponSelection.WeaponSelected += GetCurrentWeaponObject;
            _heroReloading.OnStopReloading += TurnOn;
            _inputService.Shot += TryShoot;
        }

        private void OnDestroy()
        {
            _heroWeaponSelection.WeaponSelected -= GetCurrentWeaponObject;
            _heroReloading.OnStopReloading -= TurnOn;
            _inputService.Shot -= TryShoot;
        }

        private void GetCurrentWeaponObject(GameObject weaponPrefab, HeroWeaponStaticData heroWeaponStaticData,
            TrailStaticData trailStaticData)
        {
            _heroWeaponAppearance = weaponPrefab.GetComponent<HeroWeaponAppearance>();
            _waitForSeconds = new WaitForSeconds(ShootDelay);

            if (!_isInitial)
                TurnOn();
        }

        public void TurnOn()
        {
            _canShootTemp = true;
            StartCoroutine(EnableShoot());
        }

        public void TurnOff()
        {
            if (_canShootTemp)
                _canShootTemp = false;

            _canShoot = false;
        }

        private IEnumerator EnableShoot()
        {
            yield return _waitForSeconds;

            if (_canShootTemp)
            {
                _canShoot = true;
                _isInitial = false;
            }
        }

        private void TryShoot()
        {
            if (!_canShoot)
                return;

            if (IsAvailableAmmo())
            // if (_inputService.IsAttackButtonUp() && IsAvailableAmmo())
                Shoot();
        }

        private bool IsAvailableAmmo() =>
            _progressService.ProgressData.WeaponsData.WeaponsAmmoData.IsAmmoAvailable();

        private void Shoot()
        {
            Shot?.Invoke();
            TurnOff();
            _progressService.ProgressData.WeaponsData.WeaponsAmmoData.ReduceAmmo();
            _heroWeaponAppearance.ShootTo();
        }
    }
}