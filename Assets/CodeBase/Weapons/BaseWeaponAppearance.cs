﻿using CodeBase.Logic;
using CodeBase.Projectiles;
using CodeBase.Projectiles.Hit;
using CodeBase.Projectiles.Movement;
using CodeBase.Services;
using CodeBase.Services.Pool;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.ShotVfxs;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeBase.Weapons
{
    public abstract class BaseWeaponAppearance : MonoBehaviour
    {
        [FormerlySerializedAs("_projectilesRespawns")] [SerializeField]
        public Transform[] ProjectilesRespawns;

        [FormerlySerializedAs("MuzzlesRespawns")] [FormerlySerializedAs("_muzzlesRespawns")] [SerializeField]
        protected Transform[] ShotVfxsRespawns;

        [FormerlySerializedAs("ShowProjectiles")] [SerializeField]
        protected bool _showProjectiles;

        [SerializeField] protected ShotVfxsContainer ShotVfxsContainer;

        protected IObjectsPoolService PoolService;
        private bool _initialVisibility;
        private ProjectileTypeId? _projectileTypeId;
        private IDeath _death;
        protected bool Enabled = true;

        protected WaitForSeconds LaunchProjectileCooldown { get; private set; }

        private void OnEnable() =>
            Enable();

        private void OnDisable() =>
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

        protected void TuneProjectileBeforeLaunch(GameObject projectile, ProjectileMovement projectileMovement)
        {
            projectile.GetComponentInChildren<MeshRenderer>().enabled = true;
            projectile.GetComponentInChildren<ProjectileBlast>()?.OnCollider();
            projectile.SetActive(true);
            projectileMovement.Launch();
            projectile.transform.SetParent(null);
            ShowTrail(projectile);
        }

        private void ShowTrail(GameObject projectile)
        {
            if (_projectileTypeId != null)
                projectile.GetComponent<ProjectileTrail>().ShowTrail();
        }

        protected abstract GameObject GetProjectile();

        protected abstract void Launch();

        protected abstract void Launch(Vector3 targetPosition);
    }
}