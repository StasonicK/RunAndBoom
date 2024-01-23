using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Hero;
using CodeBase.Projectiles.Hit;
using CodeBase.Projectiles.Movement;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.Weapons;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;

namespace CodeBase.Weapons
{
    public class HeroWeaponAppearance : BaseWeaponAppearance
    {
        [SerializeField] private List<GameObject> _projectiles;
        [HideInInspector] protected Vector3 TargetPosition;

        private HeroReloading _heroReloading;
        private HeroWeaponSelection _heroWeaponSelection;
        private HeroWeaponTypeId _heroWeaponTypeId;
        private HeroDeath _death;
        private GameObject _firstProjectile;
        private bool _filled;

        public void Construct(HeroDeath death, HeroReloading heroReloading, HeroWeaponSelection heroWeaponSelection)
        {
            _death = death;
            _heroReloading = heroReloading;
            _heroWeaponSelection = heroWeaponSelection;
            _projectiles = new List<GameObject>(ProjectilesRespawns.Length);
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

        private void ReadyToShoot()
        {
            if (gameObject.activeInHierarchy && (_filled == false || _projectiles.Count == 0) && Enabled)
            {
                foreach (Transform respawn in ProjectilesRespawns)
                {
                    var projectile = SetNewProjectile(respawn);
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
            for (int i = 0; i < ProjectilesRespawns.Length; i++)
            {
                StartCoroutine(CoroutineShootTo());
                Release();
                PlayShootSound();
            }

            ShotVfxsContainer.ShowShotVfx(ShotVfxsRespawns[0]);
        }

        public void ReturnShotsVfx() =>
            ShotVfxsContainer.ReturnShotVfx();

        protected virtual IEnumerator CoroutineShootTo()
        {
            Launch();
            yield return LaunchProjectileCooldown;
        }

        protected override void PlayShootSound()
        {
            switch (_heroWeaponTypeId)
            {
                case HeroWeaponTypeId.GrenadeLauncher:
                    SoundInstance.InstantiateOnTransform(
                        audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.ShotGl), transform: transform,
                        Volume, AudioSource);
                    break;
                case HeroWeaponTypeId.RPG:
                    SoundInstance.InstantiateOnTransform(
                        audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.ShotRpg), transform: transform,
                        Volume, AudioSource);
                    break;
                case HeroWeaponTypeId.RocketLauncher:
                    SoundInstance.InstantiateOnTransform(
                        audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.ShotRl), transform: transform,
                        Volume, AudioSource);
                    break;
                case HeroWeaponTypeId.Mortar:
                    SoundInstance.InstantiateOnTransform(
                        audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.ShotMortar),
                        transform: transform,
                        Volume, AudioSource);
                    break;
            }
        }

        protected override GameObject GetProjectile() 
            // =>
            {
                Debug.Log($"GetProjectile {HeroProjectilesPoolService}");
                return HeroProjectilesPoolService.GetFromPool(_heroWeaponTypeId);
            }
            // PoolService.GetHeroProjectile(_heroWeaponTypeId.ToString());

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