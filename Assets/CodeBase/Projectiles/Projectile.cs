﻿using CodeBase.Projectiles.Movement;
using CodeBase.Services;
using CodeBase.Services.Pool;
using CodeBase.StaticData.Projectiles;
using UnityEngine;

namespace CodeBase.Projectiles
{
    public class Projectile : MonoBehaviour
    {
        private IObjectsPoolService _objectsPoolService;
        private ProjectileMovement _projectileMovement;

        public ProjectileTypeId ProjectileTypeId { get; private set; }

        public void Construct(ProjectileTypeId projectileTypeId) =>
            ProjectileTypeId = projectileTypeId;

        private void Awake()
        {
            _objectsPoolService = AllServices.Container.Single<IObjectsPoolService>();
            _projectileMovement = GetComponent<ProjectileMovement>();
            _projectileMovement.Stoped += ReturnToRoot;
        }

        private void ReturnToRoot()
        {
            switch (ProjectileTypeId)
            {
                case ProjectileTypeId.PistolBullet:
                    _objectsPoolService.ReturnEnemyProjectile(gameObject);
                    break;
                case ProjectileTypeId.Shot:
                    _objectsPoolService.ReturnEnemyProjectile(gameObject);
                    break;
                default:
                    _objectsPoolService.ReturnHeroProjectile(gameObject);
                    break;
            }
        }
    }
}