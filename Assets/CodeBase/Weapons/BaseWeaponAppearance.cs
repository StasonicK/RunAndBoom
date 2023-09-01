using CodeBase.Data;
using CodeBase.Logic;
using CodeBase.Projectiles;
using CodeBase.Projectiles.Hit;
using CodeBase.Projectiles.Movement;
using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.Pool;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.ShotVfxs;
using NTC.Global.Cache;
using NTC.Global.System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeBase.Weapons
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class BaseWeaponAppearance : MonoCache, IProgressReader
    {
        [FormerlySerializedAs("_projectilesRespawns")] [SerializeField]
        public Transform[] ProjectilesRespawns;

        [FormerlySerializedAs("MuzzlesRespawns")] [FormerlySerializedAs("_muzzlesRespawns")] [SerializeField]
        protected Transform[] ShotVfxsRespawns;

        [FormerlySerializedAs("ShowProjectiles")] [SerializeField]
        protected bool _showProjectiles;

        [SerializeField] protected ShotVfxsContainer ShotVfxsContainer;

        protected AudioSource AudioSource;
        protected IObjectsPoolService PoolService;
        private bool _initialVisibility;
        private ProjectileTypeId? _projectileTypeId;
        private IDeath _death;
        protected bool Enabled = true;
        private PlayerProgress _progress;
        protected float Volume = 1f;

        protected WaitForSeconds LaunchProjectileCooldown { get; private set; }

        private void Awake() =>
            AudioSource = Get<AudioSource>();

        protected override void OnEnabled() =>
            Enable();

        protected override void OnDisabled() =>
            Disable();

        protected void Construct(IDeath death, float shotVfxLifeTime, float cooldown, ProjectileTypeId projectileTypeId,
            ShotVfxTypeId shotVfxTypeId)
        {
            _death = death;
            PoolService = AllServices.Container.Single<IObjectsPoolService>();
            ShotVfxsContainer.Construct(shotVfxLifeTime, shotVfxTypeId, transform);
            LaunchProjectileCooldown = new WaitForSeconds(cooldown);
            _projectileTypeId = projectileTypeId;
            Enable();

            _death.Died += Disable;
        }

        private void Disable()
        {
            Enabled = false;

            if (_death != null)
                _death.Died -= Disable;
        }

        private void Enable()
        {
            Enabled = true;

            if (_death != null)
                _death.Died += Disable;
        }

        protected GameObject SetNewProjectile(Transform respawn)
        {
            GameObject projectile = GetProjectile();
            projectile.transform.SetParent(respawn);
            projectile.transform.localPosition = Vector3.zero;
            projectile.transform.rotation = respawn.rotation;
            return projectile;
        }

        protected GameObject SetNewProjectile(Transform respawn, Vector3 targetPosition)
        {
            GameObject projectile = GetProjectile();
            projectile.transform.SetParent(respawn);
            projectile.transform.localPosition = Vector3.zero;
            projectile.transform.rotation = RotationTo(targetPosition, respawn.position);
            return projectile;
        }

        private Quaternion RotationTo(Vector3 targetPosition, Vector3 respawnPosition)
        {
            Vector3 positionDelta = targetPosition - respawnPosition;
            Vector3 directionToLook = new Vector3(positionDelta.x, positionDelta.y, positionDelta.z).normalized;
            return Quaternion.LookRotation(directionToLook);
        }

        protected void TuneProjectileBeforeLaunch(GameObject projectile, ProjectileMovement projectileMovement)
        {
            projectile.GetComponentInChildren<MeshRenderer>().enabled = true;
            projectile.GetComponentInChildren<ProjectileBlast>()?.OnCollider();
            projectile.Enable();
            projectileMovement.Launch();
            projectile.transform.SetParent(null);
            ShowTrail(projectile);
        }

        private void ShowTrail(GameObject projectile)
        {
            if (_projectileTypeId != null)
                projectile.GetComponent<ProjectileTrail>().ShowTrail();
        }

        protected abstract void PlayShootSound();

        protected abstract GameObject GetProjectile();

        protected abstract void Launch();

        protected abstract void Launch(Vector3 targetPosition);

        public void LoadProgress(PlayerProgress progress)
        {
            _progress = progress;
            _progress.SettingsData.SoundSwitchChanged += SwitchChanged;
            _progress.SettingsData.SoundVolumeChanged += VolumeChanged;
            VolumeChanged();
            SwitchChanged();
        }

        private void VolumeChanged() =>
            Volume = _progress.SettingsData.SoundVolume;

        private void SwitchChanged() =>
            Volume = _progress.SettingsData.SoundOn ? _progress.SettingsData.SoundVolume : Constants.Zero;
    }
}