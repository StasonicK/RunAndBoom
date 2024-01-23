using CodeBase.Projectiles.Movement;
using CodeBase.Services;
using CodeBase.Services.Pool;
using CodeBase.StaticData.Projectiles;
using UnityEngine;

namespace CodeBase.Projectiles
{
    public class Projectile : MonoBehaviour
    {
        private IHeroProjectilesPoolService _heroProjectilesPool;
        private IEnemyProjectilesPoolService _enemyProjectilesPoolService;
        // private IObjectsPoolService _objectsPoolService;
        private ProjectileMovement _projectileMovement;

        public ProjectileTypeId ProjectileTypeId { get; private set; }

        public void Construct(ProjectileTypeId projectileTypeId)
        {
            ProjectileTypeId = projectileTypeId;
            _projectileMovement = GetComponent<ProjectileMovement>();
            _projectileMovement.Stoped += ReturnToRoot;
        }

        private void Awake()
        {
            _heroProjectilesPool = AllServices.Container.Single<IHeroProjectilesPoolService>();
            _enemyProjectilesPoolService = AllServices.Container.Single<IEnemyProjectilesPoolService>();
            // _objectsPoolService = AllServices.Container.Single<IObjectsPoolService>();
        }

        private void ReturnToRoot()
        {
            switch (ProjectileTypeId)
            {
                case ProjectileTypeId.PistolBullet:
                    _enemyProjectilesPoolService.Return(gameObject);
                    // _objectsPoolService.ReturnEnemyProjectile(gameObject);
                    break;

                case ProjectileTypeId.RifleBullet:
                    _enemyProjectilesPoolService.Return(gameObject);
                    // _objectsPoolService.ReturnEnemyProjectile(gameObject);
                    break;

                case ProjectileTypeId.Shot:
                    _enemyProjectilesPoolService.Return(gameObject);
                    // _objectsPoolService.ReturnEnemyProjectile(gameObject);
                    break;

                default:
                    _heroProjectilesPool.Return(gameObject);
                    // _objectsPoolService.ReturnHeroProjectile(gameObject);
                    break;
            }
        }
    }
}