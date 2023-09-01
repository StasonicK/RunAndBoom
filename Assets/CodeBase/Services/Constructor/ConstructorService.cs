using CodeBase.Projectiles;
using CodeBase.Projectiles.Hit;
using CodeBase.Projectiles.Movement;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Hits;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.Weapons;
using UnityEngine;

namespace CodeBase.Services.Constructor
{
    public class ConstructorService : IConstructorService
    {
        private IStaticDataService _staticDataService;

        public ConstructorService() =>
            _staticDataService = AllServices.Container.Single<IStaticDataService>();

        public void ConstructEnemyProjectile(GameObject projectile, float damage, ProjectileTypeId typeId)
        {
            projectile.GetComponent<Projectile>().Construct(typeId);
            projectile.GetComponent<ProjectileMovement>().Construct(typeId);
            projectile.GetComponentInChildren<ProjectileHit>().Construct(damage);
        }

        public void ConstructHeroProjectile(GameObject projectile, ProjectileTypeId projectileTypeId,
            BlastTypeId blastTypeId, HeroWeaponTypeId heroWeaponTypeId)
        {
            ProjectileStaticData projectileStaticData = _staticDataService.ForProjectile(projectileTypeId);
            BlastStaticData blastStaticData = _staticDataService.ForBlast(blastTypeId);
            TrailStaticData trailStaticData = _staticDataService.ForTrail(projectileStaticData.TrailTypeId);
            projectile.GetComponent<Projectile>().Construct(projectileTypeId);
            projectile.GetComponent<ProjectileMovement>()
                .Construct(projectileTypeId, heroWeaponTypeId);
            projectile.GetComponentInChildren<ProjectileBlast>()
                .Construct(blastStaticData.Prefab, blastStaticData.Radius, blastStaticData.Damage, heroWeaponTypeId);
            projectile.GetComponent<ProjectileTrail>().Construct(trailStaticData);
        }

        public void ConstructProjectileLike(GameObject projectile, GameObject newProjectile)
        {
            ProjectileTypeId projectileTypeId = projectile.GetComponent<Projectile>().ProjectileTypeId;
            ProjectileStaticData projectileStaticData;
            BlastStaticData blastStaticData;
            TrailStaticData trailStaticData;

            switch (projectileTypeId)
            {
                case ProjectileTypeId.PistolBullet:
                    newProjectile.GetComponent<Projectile>().Construct(projectileTypeId);
                    newProjectile.GetComponent<ProjectileMovement>()
                        .Construct(projectileTypeId);
                    break;

                case ProjectileTypeId.RifleBullet:
                    newProjectile.GetComponent<Projectile>().Construct(projectileTypeId);
                    newProjectile.GetComponent<ProjectileMovement>()
                        .Construct(projectileTypeId);
                    break;

                case ProjectileTypeId.Shot:
                    newProjectile.GetComponent<Projectile>().Construct(projectileTypeId);
                    newProjectile.GetComponent<ProjectileMovement>()
                        .Construct(projectileTypeId);
                    break;

                case ProjectileTypeId.Grenade:
                    projectileStaticData = _staticDataService.ForProjectile(projectileTypeId);
                    blastStaticData = _staticDataService.ForBlast(BlastTypeId.Grenade);
                    trailStaticData = _staticDataService.ForTrail(projectileStaticData.TrailTypeId);
                    newProjectile.GetComponent<Projectile>().Construct(projectileTypeId);
                    newProjectile.GetComponent<ProjectileMovement>()
                        .Construct(projectileTypeId, HeroWeaponTypeId.GrenadeLauncher);
                    newProjectile.GetComponentInChildren<ProjectileBlast>()
                        .Construct(blastStaticData.Prefab, blastStaticData.Radius, blastStaticData.Damage,
                            HeroWeaponTypeId.GrenadeLauncher);
                    newProjectile.GetComponent<ProjectileTrail>().Construct(trailStaticData);
                    break;

                case ProjectileTypeId.RpgRocket:
                    projectileStaticData = _staticDataService.ForProjectile(projectileTypeId);
                    blastStaticData = _staticDataService.ForBlast(BlastTypeId.RpgRocket);
                    trailStaticData = _staticDataService.ForTrail(projectileStaticData.TrailTypeId);
                    newProjectile.GetComponent<Projectile>().Construct(projectileTypeId);
                    newProjectile.GetComponent<ProjectileMovement>()
                        .Construct(projectileTypeId, HeroWeaponTypeId.RPG);
                    newProjectile.GetComponentInChildren<ProjectileBlast>().Construct(blastStaticData.Prefab,
                        blastStaticData.Radius, blastStaticData.Damage, HeroWeaponTypeId.RPG);
                    newProjectile.GetComponent<ProjectileTrail>().Construct(trailStaticData);
                    break;

                case ProjectileTypeId.RocketLauncherRocket:
                    projectileStaticData = _staticDataService.ForProjectile(projectileTypeId);
                    blastStaticData = _staticDataService.ForBlast(BlastTypeId.RocketLauncherRocket);
                    trailStaticData = _staticDataService.ForTrail(projectileStaticData.TrailTypeId);
                    newProjectile.GetComponent<Projectile>().Construct(projectileTypeId);
                    newProjectile.GetComponent<ProjectileMovement>()
                        .Construct(projectileTypeId, HeroWeaponTypeId.RocketLauncher);
                    newProjectile.GetComponentInChildren<ProjectileBlast>()
                        .Construct(blastStaticData.Prefab, blastStaticData.Radius, blastStaticData.Damage,
                            HeroWeaponTypeId.RocketLauncher);
                    newProjectile.GetComponent<ProjectileTrail>().Construct(trailStaticData);
                    break;

                case ProjectileTypeId.Bomb:
                    projectileStaticData = _staticDataService.ForProjectile(projectileTypeId);
                    blastStaticData = _staticDataService.ForBlast(BlastTypeId.Bomb);
                    trailStaticData = _staticDataService.ForTrail(projectileStaticData.TrailTypeId);
                    newProjectile.GetComponent<Projectile>().Construct(projectileTypeId);
                    newProjectile.GetComponent<ProjectileMovement>().Construct(projectileTypeId,
                        HeroWeaponTypeId.Mortar);
                    newProjectile.GetComponentInChildren<ProjectileBlast>()
                        .Construct(blastStaticData.Prefab, blastStaticData.Radius, blastStaticData.Damage,
                            HeroWeaponTypeId.Mortar);
                    newProjectile.GetComponent<ProjectileTrail>().Construct(trailStaticData);
                    break;
            }
        }
    }
}