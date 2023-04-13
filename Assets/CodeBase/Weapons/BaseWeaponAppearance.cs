﻿using System.Collections.Generic;
using System.Linq;
using CodeBase.Projectiles;
using CodeBase.Projectiles.Hit;
using CodeBase.Projectiles.Movement;
using CodeBase.Services;
using CodeBase.Services.Pool;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.ShotVfxs;
using JetBrains.Annotations;
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

        protected IPoolService PoolService;
        private bool _initialVisibility;
        [SerializeField] private List<GameObject> _projectiles;
        private ProjectileTypeId? _projectileTypeId;

        protected WaitForSeconds LaunchProjectileCooldown { get; private set; }
        protected bool Filled { get; private set; }

        protected void Construct(float shotVfxLifeTime, float cooldown, ProjectileTypeId projectileTypeId, ShotVfxTypeId shotVfxTypeId)
        {
            PoolService = AllServices.Container.Single<IPoolService>();
            ShotVfxsContainer.Construct(shotVfxLifeTime, shotVfxTypeId, transform);
            LaunchProjectileCooldown = new WaitForSeconds(cooldown);
            _projectiles = new List<GameObject>(ProjectilesRespawns.Length);
            _projectileTypeId = projectileTypeId;
        }

        protected void ReadyToShoot()
        {
            if (gameObject.activeInHierarchy && (Filled == false || _projectiles.Count == 0))
            {
                foreach (Transform respawn in ProjectilesRespawns)
                {
                    var projectile = SetNewProjectile(respawn);
                    projectile.SetActive(true);
                    projectile.GetComponentInChildren<MeshRenderer>().enabled = _showProjectiles;
                    projectile.GetComponentInChildren<ProjectileBlast>()?.OffCollider();
                    _projectiles.Add(projectile);
                }

                Filled = true;
            }
        }

        protected void Launch()
        {
            GameObject projectile = GetFirstProjectile();
            ProjectileMovement projectileMovement = projectile.GetComponent<ProjectileMovement>();
            projectile.GetComponentInChildren<MeshRenderer>().enabled = true;
            projectile.GetComponentInChildren<ProjectileBlast>()?.OnCollider();
            projectileMovement.Launch();
            projectile.transform.SetParent(null);

            ShowTrail(projectile);

            _projectiles.Remove(projectile);
            Debug.Log($"Count {_projectiles.Count}");
        }

        private GameObject GetFirstProjectile() =>
            _projectiles.First();

        [CanBeNull]
        public ProjectileMovement GetMovement()
        {
            if (_projectiles.Count != 0)
                return _projectiles.First().GetComponent<ProjectileMovement>();
            else
                return null;
        }

        private void ShowTrail(GameObject projectile)
        {
            if (_projectileTypeId != null)
                projectile.GetComponent<ProjectileTrail>().ShowTrail();
        }

        private GameObject SetNewProjectile(Transform respawn)
        {
            Debug.Log($"projectile type: {_projectileTypeId}");
            GameObject projectile = GetProjectile();

            projectile.transform.SetParent(respawn);
            projectile.transform.localPosition = Vector3.zero;
            projectile.transform.rotation = respawn.rotation;
            return projectile;
        }

        protected void Released() =>
            Filled = false;

        protected abstract GameObject GetProjectile();
    }
}