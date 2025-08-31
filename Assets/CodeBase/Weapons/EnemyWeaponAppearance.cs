using System.Collections;
using CodeBase.Enemy;
using CodeBase.Projectiles.Movement;
using CodeBase.Services.Audio;
using CodeBase.StaticData.Enemies;
using CodeBase.StaticData.Weapons;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Weapons
{
    public class EnemyWeaponAppearance : BaseWeaponAppearance
    {
        private EnemyWeaponTypeId _enemyWeaponTypeId;
        private Transform _currentProjectileRespawn;
        private EnemyStaticData _enemyStaticData;
        private EnemyTypeId _enemyTypeId;
        private EnemyWeaponStaticData _enemyWeaponStaticData;

        public void Construct(EnemyDeath death, EnemyTypeId enemyTypeId, EnemyWeaponStaticData weaponStaticData)
        {
            base.Construct(death, weaponStaticData.MuzzleVfxLifeTime, weaponStaticData.Cooldown,
                weaponStaticData.ProjectileTypeId, weaponStaticData.ShotVfxTypeId);

            _enemyTypeId = enemyTypeId;
            _enemyWeaponTypeId = weaponStaticData.WeaponTypeId;
        }

        public void Shoot(Vector3 targetPosition)
        {
            foreach (var t in _projectilesRespawns)
            {
                _currentProjectileRespawn = t;
                StartCoroutine(CoroutineShootTo(targetPosition));
            }

            _shotVfxsContainer.ShowShotVfx(_shotVfxsRespawns[0]);
            PlayShootSound();
        }

        public void Shoot()
        {
            foreach (var respawn in _projectilesRespawns)
            {
                _currentProjectileRespawn = respawn;
                StartCoroutine(CoroutineShootTo());
            }

            _shotVfxsContainer.ShowShotVfx(_shotVfxsRespawns[0]);
            PlayShootSound();
        }

        private IEnumerator CoroutineShootTo(Vector3 targetPosition)
        {
            Launch(targetPosition);
            yield return _launchProjectileCooldown;
        }

        private IEnumerator CoroutineShootTo()
        {
            Launch();
            yield return _launchProjectileCooldown;
        }

        protected override async void Launch()
        {
            GameObject projectile = await SetNewProjectile(_currentProjectileRespawn);
            ProjectileMovement projectileMovement = projectile.GetComponent<ProjectileMovement>();
            TuneProjectileBeforeLaunch(projectile, projectileMovement);
        }

        protected override async void Launch(Vector3 targetPosition)
        {
            GameObject projectile = await SetNewProjectile(_currentProjectileRespawn, targetPosition);
            ProjectileMovement projectileMovement = projectile.GetComponent<ProjectileMovement>();
            TuneProjectileBeforeLaunch(projectile, projectileMovement);
        }

        protected override async UniTask<GameObject> GetProjectile()
        {
            _projectile = await _poolService.GetEnemyProjectile(_projectileTypeId.ToString());
            _enemyStaticData = _staticDataService.ForEnemy(_enemyTypeId);
            _enemyWeaponStaticData = _staticDataService.ForEnemyWeapon(_enemyWeaponTypeId);
            _constructorService.ConstructEnemyProjectile(_projectile, _enemyStaticData.Damage,
                _enemyWeaponStaticData.ProjectileTypeId);
            return _projectile;
        }

        protected override void PlayShootSound()
        {
            switch (_enemyWeaponTypeId)
            {
                case EnemyWeaponTypeId.None:
                    break;
                case EnemyWeaponTypeId.Pistol:
                    _audioService.LaunchShotSound(ShotSoundId.ShotPistol, transform, _audioSource);
                    break;
                case EnemyWeaponTypeId.Shotgun:
                    _audioService.LaunchShotSound(ShotSoundId.ShotShotgun, transform, _audioSource);
                    break;
                case EnemyWeaponTypeId.SniperRifle:
                    _audioService.LaunchShotSound(ShotSoundId.ShotSr, transform, _audioSource);
                    break;
                case EnemyWeaponTypeId.SMG:
                    _audioService.LaunchShotSound(ShotSoundId.ShotSmg, transform, _audioSource);
                    break;
                case EnemyWeaponTypeId.MG:
                    _audioService.LaunchShotSound(ShotSoundId.ShotMg, transform, _audioSource);
                    break;
            }
        }
    }
}