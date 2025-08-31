using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Hero;
using CodeBase.Projectiles.Hit;
using CodeBase.Projectiles.Movement;
using CodeBase.Services.Audio;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.Weapons;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Weapons
{
    public class HeroWeaponAppearance : BaseWeaponAppearance
    {
        [SerializeField] private List<GameObject> _projectiles;
        [HideInInspector] protected Vector3 _targetPosition;

        private HeroReloading _heroReloading;
        private HeroWeaponSelection _heroWeaponSelection;
        private HeroWeaponTypeId _heroWeaponTypeId;
        private HeroDeath _death;
        private GameObject _firstProjectile;
        private bool _filled;
        private HeroWeaponStaticData _heroWeaponStaticData;

        public void Construct(HeroDeath death, HeroReloading heroReloading, HeroWeaponSelection heroWeaponSelection)
        {
            _death = death;
            _heroReloading = heroReloading;
            _heroWeaponSelection = heroWeaponSelection;
            _projectiles = new List<GameObject>(_projectilesRespawns.Length);
            _heroWeaponSelection.WeaponSelected += InitializeSelectedWeapon;
        }

        private void InitializeSelectedWeapon(GameObject weaponPrefab, HeroWeaponStaticData weaponStaticData,
            TrailStaticData trailStaticData)
        {
            base.Construct(_death, weaponStaticData.ShotVfxLifeTime, weaponStaticData.Cooldown,
                weaponStaticData.ProjectileTypeId, weaponStaticData.ShotVfxTypeId);
            _heroWeaponTypeId = weaponStaticData.WeaponTypeId;

            _heroReloading.OnStopReloading += ReadyToShoot;
            _heroWeaponSelection.WeaponSelected += ReadyToShoot;
        }

        private void ReadyToShoot(GameObject arg1, HeroWeaponStaticData arg2, TrailStaticData arg3) =>
            ReadyToShoot();

        private async void ReadyToShoot()
        {
            if (gameObject.activeInHierarchy && (_filled == false || _projectiles.Count == 0) && _enabled)
            {
                foreach (Transform respawn in _projectilesRespawns)
                {
                    var projectile = await SetNewProjectile(respawn);
                    projectile.SetActive(true);
                    projectile.GetComponentInChildren<MeshRenderer>().enabled = _showProjectiles;
                    projectile.GetComponentInChildren<ProjectileBlast>()?.OffCollider();
                    _projectiles.Add(projectile);
                }

                _filled = true;
            }
        }

        public void ShootTo()
        {
            for (int i = 0; i < _projectilesRespawns.Length; i++)
            {
                StartCoroutine(CoroutineShootTo());
                Release();
            }

            _shotVfxsContainer.ShowShotVfx(_shotVfxsRespawns[0]);
            PlayShootSound();
        }

        public void ReturnShotsVfx() =>
            _shotVfxsContainer.ReturnShotVfx();

        protected virtual IEnumerator CoroutineShootTo()
        {
            Launch();
            yield return _launchProjectileCooldown;
        }

        protected override void PlayShootSound()
        {
            switch (_heroWeaponTypeId)
            {
                case HeroWeaponTypeId.GrenadeLauncher:
                    _audioService.LaunchShotSound(ShotSoundId.ShotGl, transform, _audioSource);
                    break;
                case HeroWeaponTypeId.RPG:
                    _audioService.LaunchShotSound(ShotSoundId.ShotRpg, transform, _audioSource);
                    break;
                case HeroWeaponTypeId.RocketLauncher:
                    _audioService.LaunchShotSound(ShotSoundId.ShotRl, transform, _audioSource);
                    break;
                case HeroWeaponTypeId.Mortar:
                    _audioService.LaunchShotSound(ShotSoundId.ShotMortar, transform, _audioSource);
                    break;
            }
        }

        protected override async UniTask<GameObject> GetProjectile()
        {
            _projectile = await _poolService.GetHeroProjectile(_projectileTypeId.ToString());
            _heroWeaponStaticData = _staticDataService.ForHeroWeapon(_heroWeaponTypeId);
            _constructorService.ConstructHeroProjectile(_projectile, _heroWeaponStaticData.ProjectileTypeId,
                _heroWeaponStaticData.BlastTypeId, _heroWeaponTypeId);
            return _projectile;
        }

        protected override void Launch()
        {
            GameObject projectile = GetFirstProjectile();
            ProjectileMovement projectileMovement = projectile.GetComponent<ProjectileMovement>();
            TuneProjectileBeforeLaunch(projectile, projectileMovement);
        }

        protected override void Launch(Vector3 targetPosition)
        {
            GameObject projectile = GetFirstProjectile();
            ProjectileMovement projectileMovement = projectile.GetComponent<ProjectileMovement>();
            (projectileMovement as BombMovement)?.SetTargetPosition(targetPosition);
            TuneProjectileBeforeLaunch(projectile, projectileMovement);
        }

        private GameObject GetFirstProjectile()
        {
            _firstProjectile = _projectiles.First();
            return _firstProjectile;
        }

        private void Release()
        {
            _projectiles.Remove(_firstProjectile);
            _firstProjectile = null;

            if (_projectiles.Count == 0)
                _filled = false;
        }
    }
}