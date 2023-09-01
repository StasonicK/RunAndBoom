using System.Collections.Generic;
using System.Linq;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.Constructor;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Enemies;
using CodeBase.StaticData.Hits;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.ShotVfxs;
using CodeBase.StaticData.Weapons;
using NTC.Global.System;
using UnityEngine;

namespace CodeBase.Services.Pool
{
    public class ObjectsPoolService : IObjectsPoolService
    {
        private const int InitialVfxCapacity = 5;
        private const int InitialEnemyProjectilesCapacity = 15;
        private const int InitialHeroProjectilesCapacity = 6;
        private const int AdditionalCount = 5;

        private IAssets _assets;
        private IConstructorService _constructorService;
        private IStaticDataService _staticDataService;
        private Dictionary<string, List<GameObject>> _heroProjectiles;
        private Dictionary<string, List<GameObject>> _enemyProjectiles;
        private Dictionary<string, List<GameObject>> _shotVfxs;
        private Transform _enemyProjectilesRoot;
        private Transform _heroProjectilesRoot;
        private Transform _shotVfxsRoot;

        public ObjectsPoolService(IAssets assets, IConstructorService constructorService,
            IStaticDataService staticDataService)
        {
            _staticDataService = staticDataService;
            _assets = assets;
            _constructorService = constructorService;
        }

        public void GenerateObjects() =>
            CreateRoots();

        private async void CreateRoots()
        {
            GameObject root = await _assets.Load<GameObject>(AssetAddresses.HeroProjectilesRoot);
            GameObject gameObject = Object.Instantiate(root);
            _heroProjectilesRoot = gameObject.transform;

            root = await _assets.Load<GameObject>(AssetAddresses.EnemyProjectilesRoot);
            gameObject = Object.Instantiate(root);
            _enemyProjectilesRoot = gameObject.transform;

            root = await _assets.Load<GameObject>(AssetAddresses.ShotVfxsRoot);
            gameObject = Object.Instantiate(root);
            _shotVfxsRoot = gameObject.transform;

            GenerateHeroProjectiles();
            GenerateEnemyProjectiles();
            GenerateShotVfxs();
        }

        private async void GenerateEnemyProjectiles()
        {
            _enemyProjectiles = new Dictionary<string, List<GameObject>>();
            List<GameObject> gameObjects = new List<GameObject>(InitialEnemyProjectilesCapacity);
            GameObject projectile;
            EnemyStaticData enemyStaticData;

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                projectile = await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot);
                enemyStaticData = _staticDataService.ForEnemy(EnemyTypeId.WithPistol);
                _constructorService.ConstructEnemyProjectile(projectile, enemyStaticData.Damage,
                    ProjectileTypeId.PistolBullet);
                projectile.Disable();
                gameObjects.Add(projectile);
            }

            _enemyProjectiles.Add(EnemyWeaponTypeId.Pistol.ToString(), gameObjects);

            gameObjects = new List<GameObject>(InitialEnemyProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                projectile = await _assets.Instantiate(AssetAddresses.Shot, _enemyProjectilesRoot);
                enemyStaticData = _staticDataService.ForEnemy(EnemyTypeId.WithShotgun);
                _constructorService.ConstructEnemyProjectile(projectile, enemyStaticData.Damage,
                    ProjectileTypeId.Shot);
                projectile.Disable();
                gameObjects.Add(projectile);
            }

            _enemyProjectiles.Add(EnemyWeaponTypeId.Shotgun.ToString(), gameObjects);

            gameObjects = new List<GameObject>(InitialEnemyProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                projectile = await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot);
                enemyStaticData = _staticDataService.ForEnemy(EnemyTypeId.WithSniperRifle);
                _constructorService.ConstructEnemyProjectile(projectile, enemyStaticData.Damage,
                    ProjectileTypeId.RifleBullet);
                projectile.Disable();
                gameObjects.Add(projectile);
            }

            _enemyProjectiles.Add(EnemyWeaponTypeId.SniperRifle.ToString(), gameObjects);

            gameObjects = new List<GameObject>(InitialEnemyProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                projectile = await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot);
                enemyStaticData = _staticDataService.ForEnemy(EnemyTypeId.WithSMG);
                _constructorService.ConstructEnemyProjectile(projectile, enemyStaticData.Damage,
                    ProjectileTypeId.PistolBullet);
                projectile.Disable();
                gameObjects.Add(projectile);
            }

            _enemyProjectiles.Add(EnemyWeaponTypeId.SMG.ToString(), gameObjects);

            gameObjects = new List<GameObject>(InitialEnemyProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                projectile = await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot);
                enemyStaticData = _staticDataService.ForEnemy(EnemyTypeId.WithMG);
                _constructorService.ConstructEnemyProjectile(projectile, enemyStaticData.Damage,
                    ProjectileTypeId.RifleBullet);
                projectile.Disable();
                gameObjects.Add(projectile);
            }

            _enemyProjectiles.Add(EnemyWeaponTypeId.MG.ToString(), gameObjects);
        }

        private async void GenerateHeroProjectiles()
        {
            _heroProjectiles = new Dictionary<string, List<GameObject>>();
            List<GameObject> gameObjects = new List<GameObject>(InitialHeroProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                GameObject projectile = await _assets.Instantiate(AssetAddresses.Grenade, _heroProjectilesRoot);
                _constructorService.ConstructHeroProjectile(projectile, ProjectileTypeId.Grenade, BlastTypeId.Grenade,
                    HeroWeaponTypeId.GrenadeLauncher);
                projectile.Disable();
                gameObjects.Add(projectile);
            }

            _heroProjectiles.Add(HeroWeaponTypeId.GrenadeLauncher.ToString(), gameObjects);

            gameObjects = new List<GameObject>(InitialHeroProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                GameObject projectile = await _assets.Instantiate(AssetAddresses.RpgRocket, _heroProjectilesRoot);
                _constructorService.ConstructHeroProjectile(projectile, ProjectileTypeId.RpgRocket,
                    BlastTypeId.RpgRocket, HeroWeaponTypeId.RPG);
                projectile.Disable();
                gameObjects.Add(projectile);
            }

            _heroProjectiles.Add(HeroWeaponTypeId.RPG.ToString(), gameObjects);

            gameObjects = new List<GameObject>(InitialHeroProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                GameObject projectile =
                    await _assets.Instantiate(AssetAddresses.RocketLauncherRocket, _heroProjectilesRoot);
                _constructorService.ConstructHeroProjectile(projectile, ProjectileTypeId.RocketLauncherRocket,
                    BlastTypeId.RocketLauncherRocket, HeroWeaponTypeId.RocketLauncher);
                projectile.Disable();
                gameObjects.Add(projectile);
            }

            _heroProjectiles.Add(HeroWeaponTypeId.RocketLauncher.ToString(), gameObjects);
            gameObjects = new List<GameObject>(InitialHeroProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                GameObject projectile = await _assets.Instantiate(AssetAddresses.Bomb, _heroProjectilesRoot);
                _constructorService.ConstructHeroProjectile(projectile, ProjectileTypeId.Bomb, BlastTypeId.Bomb,
                    HeroWeaponTypeId.Mortar);
                projectile.Disable();
                gameObjects.Add(projectile);
            }

            _heroProjectiles.Add(HeroWeaponTypeId.Mortar.ToString(), gameObjects);
        }

        private async void GenerateShotVfxs()
        {
            _shotVfxs = new Dictionary<string, List<GameObject>>();
            List<GameObject> gameObjects = new List<GameObject>(InitialVfxCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                GameObject shotVfx = await _assets.Instantiate(AssetAddresses.GrenadeMuzzleFire, _shotVfxsRoot);
                shotVfx.Disable();
                gameObjects.Add(shotVfx);
            }

            _shotVfxs.Add(ShotVfxTypeId.Grenade.ToString(), gameObjects);
            gameObjects = new List<GameObject>(InitialVfxCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                GameObject shotVfx = await _assets.Instantiate(AssetAddresses.RpgMuzzleFire, _shotVfxsRoot);
                shotVfx.Disable();
                gameObjects.Add(shotVfx);
            }

            _shotVfxs.Add(ShotVfxTypeId.RpgRocket.ToString(), gameObjects);
            gameObjects = new List<GameObject>(InitialVfxCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                GameObject shotVfx = await _assets.Instantiate(AssetAddresses.RocketLauncherMuzzleBlue, _shotVfxsRoot);
                shotVfx.Disable();
                gameObjects.Add(shotVfx);
            }

            _shotVfxs.Add(ShotVfxTypeId.RocketLauncherRocket.ToString(), gameObjects);
            gameObjects = new List<GameObject>(InitialVfxCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                GameObject shotVfx = await _assets.Instantiate(AssetAddresses.BombMuzzle, _shotVfxsRoot);
                shotVfx.Disable();
                gameObjects.Add(shotVfx);
            }

            _shotVfxs.Add(ShotVfxTypeId.Bomb.ToString(), gameObjects);
            gameObjects = new List<GameObject>(InitialVfxCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                GameObject shotVfx = await _assets.Instantiate(AssetAddresses.BulletMuzzleFire, _shotVfxsRoot);
                shotVfx.Disable();
                gameObjects.Add(shotVfx);
            }

            _shotVfxs.Add(ShotVfxTypeId.Bullet.ToString(), gameObjects);
            gameObjects = new List<GameObject>(InitialVfxCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                GameObject shotVfx = await _assets.Instantiate(AssetAddresses.ShotMuzzleFire, _shotVfxsRoot);
                shotVfx.Disable();
                gameObjects.Add(shotVfx);
            }

            _shotVfxs.Add(ShotVfxTypeId.Shot.ToString(), gameObjects);
        }

        public GameObject GetEnemyProjectile(string name) =>
            GetGameObject(Pools.EnemyProjectiles, name, _enemyProjectiles, _enemyProjectilesRoot);

        public GameObject GetHeroProjectile(string name) =>
            GetGameObject(Pools.HeroProjectiles, name, _heroProjectiles, _heroProjectilesRoot);

        public GameObject GetShotVfx(ShotVfxTypeId typeId)
        {
            GameObject gameObject = null;

            switch (typeId)
            {
                case ShotVfxTypeId.Grenade:
                    gameObject = GetGameObject(Pools.ShotVfxs, ShotVfxTypeId.Grenade.ToString(), _shotVfxs,
                        _shotVfxsRoot);
                    break;
                case ShotVfxTypeId.RocketLauncherRocket:
                    gameObject = GetGameObject(Pools.ShotVfxs, ShotVfxTypeId.RocketLauncherRocket.ToString(), _shotVfxs,
                        _shotVfxsRoot);
                    break;
                case ShotVfxTypeId.RpgRocket:
                    gameObject = GetGameObject(Pools.ShotVfxs, ShotVfxTypeId.RpgRocket.ToString(), _shotVfxs,
                        _shotVfxsRoot);
                    break;
                case ShotVfxTypeId.Bomb:
                    gameObject = GetGameObject(Pools.ShotVfxs, ShotVfxTypeId.Bomb.ToString(), _shotVfxs, _shotVfxsRoot);
                    break;
                case ShotVfxTypeId.Bullet:
                    gameObject = GetGameObject(Pools.ShotVfxs, ShotVfxTypeId.Bullet.ToString(), _shotVfxs,
                        _shotVfxsRoot);
                    break;
                case ShotVfxTypeId.Shot:
                    gameObject = GetGameObject(Pools.ShotVfxs, ShotVfxTypeId.Shot.ToString(), _shotVfxs, _shotVfxsRoot);
                    break;
            }

            return gameObject;
        }

        public void ReturnEnemyProjectile(GameObject gameObject) =>
            ReturnGameObject(gameObject, _enemyProjectilesRoot);

        public void ReturnHeroProjectile(GameObject gameObject) =>
            ReturnGameObject(gameObject, _heroProjectilesRoot);

        public void ReturnShotVfx(GameObject gameObject) =>
            ReturnGameObject(gameObject, _shotVfxsRoot);

        private void ReturnGameObject(GameObject gameObject, Transform parent)
        {
            gameObject.SetActive(false);
            gameObject.transform.SetParent(parent);
        }

        private GameObject GetGameObject(Pools pool, string name, Dictionary<string, List<GameObject>> dictionary,
            Transform parent)
        {
            dictionary.TryGetValue(name, out List<GameObject> list);
            GameObject gameObject = null;

            if (list != null)
            {
                gameObject = list.FirstOrDefault(it => it.activeInHierarchy == false);

                if (gameObject != null)
                {
                    return gameObject;
                }
                else
                {
                    gameObject = ExtendList(pool, name, list, dictionary, parent);
                    return gameObject;
                }
            }

            return gameObject;
        }

        private GameObject ExtendList(Pools pool, string name, List<GameObject> list,
            Dictionary<string, List<GameObject>> dictionary, Transform parent)
        {
            List<GameObject> newList = new List<GameObject>(list.Count + AdditionalCount);
            newList.AddRange(list);

            for (int i = 0; i < AdditionalCount; i++)
            {
                GameObject original = newList[0];
                GameObject newGameObject = Object.Instantiate(original, parent);

                if (pool != Pools.ShotVfxs)
                    _constructorService.ConstructProjectileLike(original, newGameObject);

                newGameObject.Disable();
                newList.Add(newGameObject);
            }

            dictionary[name] = newList;
            dictionary.TryGetValue(name, out List<GameObject> list1);
            GameObject gameObject = list1?.FirstOrDefault(it => it.activeInHierarchy == false);

            if (gameObject != null)
                return gameObject;
            else
                return ExtendList(pool, name, list1, dictionary, parent);
        }
    }

    enum Pools
    {
        EnemyProjectiles,
        HeroProjectiles,
        ShotVfxs,
    }
}