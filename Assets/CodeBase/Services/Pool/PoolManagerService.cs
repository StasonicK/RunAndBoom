using System.Collections.Generic;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Pool;
using CodeBase.Services.Constructor;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Enemies;
using CodeBase.StaticData.Hits;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.Weapons;
using UnityEngine;

namespace CodeBase.Services.Pool
{
    public class PoolManagerService : IObjectsPoolService
    {
        private const int InitialVfxCapacity = 15;
        private const int InitialEnemyProjectilesCapacity = 50;
        private const int InitialHeroProjectilesCapacity = 50;

        private IAssets _assets;
        private IConstructorService _constructorService;
        private IStaticDataService _staticDataService;
        private Transform _enemyProjectilesRoot;
        private Transform _heroProjectilesRoot;
        private Transform _shotVfxsRoot;
        private GameObject _gameObject;
        private List<GameObject> _list = new List<GameObject>();

        public PoolManagerService(IAssets assets, IConstructorService constructorService,
            IStaticDataService staticDataService)
        {
            _staticDataService = staticDataService;
            _assets = assets;
            _constructorService = constructorService;
        }

        public void GenerateObjects()
        {
            GenerateHeroProjectiles();
            GenerateEnemyProjectiles();
            GenerateShotVfxs();
        }

        private async void GenerateEnemyProjectiles()
        {
            EnemyStaticData enemyStaticData;

            _gameObject = await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot);
            enemyStaticData = _staticDataService.ForEnemy(EnemyTypeId.WithPistol);
            _constructorService.ConstructEnemyProjectile(_gameObject, enemyStaticData.Damage,
                ProjectileTypeId.PistolBullet);
            PoolManager.WarmPool(_gameObject, InitialEnemyProjectilesCapacity);

            _gameObject = await _assets.Instantiate(AssetAddresses.Shot, _enemyProjectilesRoot);
            enemyStaticData = _staticDataService.ForEnemy(EnemyTypeId.WithShotgun);
            _constructorService.ConstructEnemyProjectile(_gameObject, enemyStaticData.Damage,
                ProjectileTypeId.Shot);
            PoolManager.WarmPool(_gameObject, InitialEnemyProjectilesCapacity);

            _gameObject = await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot);
            enemyStaticData = _staticDataService.ForEnemy(EnemyTypeId.WithSniperRifle);
            _constructorService.ConstructEnemyProjectile(_gameObject, enemyStaticData.Damage,
                ProjectileTypeId.RifleBullet);
            PoolManager.WarmPool(_gameObject, InitialEnemyProjectilesCapacity);

            _gameObject = await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot);
            enemyStaticData = _staticDataService.ForEnemy(EnemyTypeId.WithSMG);
            _constructorService.ConstructEnemyProjectile(_gameObject, enemyStaticData.Damage,
                ProjectileTypeId.PistolBullet);
            PoolManager.WarmPool(_gameObject, InitialEnemyProjectilesCapacity);

            _gameObject = await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot);
            enemyStaticData = _staticDataService.ForEnemy(EnemyTypeId.WithMG);
            _constructorService.ConstructEnemyProjectile(_gameObject, enemyStaticData.Damage,
                ProjectileTypeId.RifleBullet);
            PoolManager.WarmPool(_gameObject, InitialEnemyProjectilesCapacity);
        }

        private async void GenerateHeroProjectiles()
        {
            _gameObject = await _assets.Instantiate(AssetAddresses.Grenade, _heroProjectilesRoot);
            _constructorService.ConstructHeroProjectile(_gameObject, ProjectileTypeId.Grenade, BlastTypeId.Grenade,
                HeroWeaponTypeId.GrenadeLauncher);
            PoolManager.WarmPool(_gameObject, InitialHeroProjectilesCapacity);

            _gameObject = await _assets.Instantiate(AssetAddresses.RpgRocket, _heroProjectilesRoot);
            _constructorService.ConstructHeroProjectile(_gameObject, ProjectileTypeId.RpgRocket,
                BlastTypeId.RpgRocket, HeroWeaponTypeId.RPG);
            PoolManager.WarmPool(_gameObject, InitialHeroProjectilesCapacity);

            _gameObject =
                await _assets.Instantiate(AssetAddresses.RocketLauncherRocket, _heroProjectilesRoot);
            _constructorService.ConstructHeroProjectile(_gameObject, ProjectileTypeId.RocketLauncherRocket,
                BlastTypeId.RocketLauncherRocket, HeroWeaponTypeId.RocketLauncher);
            PoolManager.WarmPool(_gameObject, InitialHeroProjectilesCapacity);

            _gameObject = await _assets.Instantiate(AssetAddresses.Bomb, _heroProjectilesRoot);
            _constructorService.ConstructHeroProjectile(_gameObject, ProjectileTypeId.Bomb, BlastTypeId.Bomb,
                HeroWeaponTypeId.Mortar);
            PoolManager.WarmPool(_gameObject, InitialHeroProjectilesCapacity);
        }

        private async void GenerateShotVfxs()
        {
            _gameObject = await _assets.Instantiate(AssetAddresses.GrenadeMuzzleFire, _shotVfxsRoot);
            PoolManager.WarmPool(_gameObject, InitialVfxCapacity);

            _gameObject = await _assets.Instantiate(AssetAddresses.RpgMuzzleFire, _shotVfxsRoot);
            PoolManager.WarmPool(_gameObject, InitialVfxCapacity);

            _gameObject = await _assets.Instantiate(AssetAddresses.RocketLauncherMuzzleBlue, _shotVfxsRoot);
            PoolManager.WarmPool(_gameObject, InitialVfxCapacity);

            _gameObject = await _assets.Instantiate(AssetAddresses.BombMuzzle, _shotVfxsRoot);
            PoolManager.WarmPool(_gameObject, InitialVfxCapacity);

            _gameObject = await _assets.Instantiate(AssetAddresses.BulletMuzzleFire, _shotVfxsRoot);
            PoolManager.WarmPool(_gameObject, InitialVfxCapacity);

            _gameObject = await _assets.Instantiate(AssetAddresses.ShotMuzzleFire, _shotVfxsRoot);
            PoolManager.WarmPool(_gameObject, InitialVfxCapacity);
        }
    }
}

enum Pools
{
    EnemyProjectiles,
    HeroProjectiles,
    ShotVfxs,
}