using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.ShotVfxs;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.Services.Pool
{
    public class ObjectsPoolService : IObjectsPoolService
    {
        private const int InitialVfxCapacity = 1;
        private const int InitialEnemyProjectilesCapacity = 1;
        private const int InitialHeroProjectilesCapacity = 1;
        private const int AdditionalCount = 5;

        private IAssets _assets;
        private Dictionary<string, List<GameObject>> _activeHeroProjectiles;
        private Dictionary<string, List<GameObject>> _passiveHeroProjectiles;
        private Dictionary<string, List<GameObject>> _activeEnemyProjectiles;
        private Dictionary<string, List<GameObject>> _passiveEnemyProjectiles;
        private Dictionary<string, List<GameObject>> _activeShotVfxs;
        private Dictionary<string, List<GameObject>> _passiveShotVfxs;

        private Transform _enemyProjectilesRoot;
        private Transform _heroProjectilesRoot;
        private Transform _shotVfxsRoot;
        [CanBeNull] private GameObject _gameObject;

        private List<GameObject> _activeList;
        private List<GameObject> _passiveList;
        private List<GameObject> _tempList;

        private const bool EnableDebugLog = false;

        public ObjectsPoolService(IAssets assets) =>
            _assets = assets;

        public void GenerateObjects() =>
            CreateRoots().Forget();

        private async UniTask CreateRoots()
        {
            GameObject root = await _assets.Load<GameObject>(AssetAddresses.HeroProjectilesRoot);
            _heroProjectilesRoot = Object.Instantiate(root).transform;

            root = await _assets.Load<GameObject>(AssetAddresses.EnemyProjectilesRoot);
            _enemyProjectilesRoot = Object.Instantiate(root).transform;

            root = await _assets.Load<GameObject>(AssetAddresses.ShotVfxsRoot);
            _shotVfxsRoot = Object.Instantiate(root).transform;

         await   GenerateHeroProjectiles();
         await  GenerateEnemyProjectiles();
         await  GenerateShotVfxs();
        }

        private async UniTask GenerateEnemyProjectiles()
        {
            _activeEnemyProjectiles = new();
            _passiveEnemyProjectiles = new();

            await AddProjectileType(_passiveEnemyProjectiles, _activeEnemyProjectiles, ProjectileTypeId.PistolBullet,
                AssetAddresses.PistolBullet, _enemyProjectilesRoot, InitialEnemyProjectilesCapacity);
            await AddProjectileType(_passiveEnemyProjectiles, _activeEnemyProjectiles, ProjectileTypeId.RifleBullet,
                AssetAddresses.PistolBullet, _enemyProjectilesRoot, InitialEnemyProjectilesCapacity);
            await AddProjectileType(_passiveEnemyProjectiles, _activeEnemyProjectiles, ProjectileTypeId.Shot,
                AssetAddresses.Shot, _enemyProjectilesRoot, InitialEnemyProjectilesCapacity);
        }

        private async UniTask GenerateHeroProjectiles()
        {
            _activeHeroProjectiles = new();
            _passiveHeroProjectiles = new();

            await AddProjectileType(_passiveHeroProjectiles, _activeHeroProjectiles, ProjectileTypeId.Grenade,
                AssetAddresses.Grenade, _heroProjectilesRoot, InitialHeroProjectilesCapacity);
            await AddProjectileType(_passiveHeroProjectiles, _activeHeroProjectiles, ProjectileTypeId.RpgRocket,
                AssetAddresses.RpgRocket, _heroProjectilesRoot, InitialHeroProjectilesCapacity);
            await AddProjectileType(_passiveHeroProjectiles, _activeHeroProjectiles,
                ProjectileTypeId.RocketLauncherRocket, AssetAddresses.RocketLauncherRocket, _heroProjectilesRoot,
                InitialHeroProjectilesCapacity);
            await AddProjectileType(_passiveHeroProjectiles, _activeHeroProjectiles, ProjectileTypeId.Bomb,
                AssetAddresses.Bomb, _heroProjectilesRoot, InitialHeroProjectilesCapacity);
        }

        private async UniTask GenerateShotVfxs()
        {
            _activeShotVfxs = new();
            _passiveShotVfxs = new();

            await AddProjectileType(_passiveShotVfxs, _activeShotVfxs, ShotVfxTypeId.Grenade,
                AssetAddresses.GrenadeMuzzleFire, _shotVfxsRoot, InitialVfxCapacity);
            await AddProjectileType(_passiveShotVfxs, _activeShotVfxs, ShotVfxTypeId.RpgRocket,
                AssetAddresses.RpgMuzzleFire, _shotVfxsRoot, InitialVfxCapacity);
            await AddProjectileType(_passiveShotVfxs, _activeShotVfxs, ShotVfxTypeId.RocketLauncherRocket,
                AssetAddresses.RocketLauncherMuzzleBlue, _shotVfxsRoot, InitialVfxCapacity);
            await AddProjectileType(_passiveShotVfxs, _activeShotVfxs, ShotVfxTypeId.Bomb, AssetAddresses.BombMuzzle,
                _shotVfxsRoot, InitialVfxCapacity);
            await AddProjectileType(_passiveShotVfxs, _activeShotVfxs, ShotVfxTypeId.Bullet,
                AssetAddresses.BulletMuzzleFire, _shotVfxsRoot, InitialVfxCapacity);
            await AddProjectileType(_passiveShotVfxs, _activeShotVfxs, ShotVfxTypeId.Shot,
                AssetAddresses.ShotMuzzleFire, _shotVfxsRoot, InitialVfxCapacity);
        }

        private async UniTask AddProjectileType(Dictionary<string, List<GameObject>> passive,
            Dictionary<string, List<GameObject>> active,
            Enum typeId, string assetAddress, Transform parent, int count)
        {
            string key = typeId.ToString();
            List<GameObject> passiveList = new(count);
            for (int i = 0; i < count; i++)
            {
                GameObject go = await _assets.Instantiate(assetAddress, parent);
                go.SetActive(false);
                passiveList.Add(go);
            }

            passive[key] = passiveList;
            active[key] = new List<GameObject>(count);
        }

        public async UniTask<GameObject> GetEnemyProjectile(string name) =>
            await GetGameObject(Pools.EnemyProjectiles, name, _activeEnemyProjectiles, _passiveEnemyProjectiles);

        public async UniTask<GameObject> GetHeroProjectile(string name) =>
            await GetGameObject(Pools.HeroProjectiles, name, _activeHeroProjectiles, _passiveHeroProjectiles);

        public async UniTask<GameObject> GetShotVfx(ShotVfxTypeId typeId) =>
            await GetGameObject(Pools.ShotVfxs, typeId.ToString(), _activeShotVfxs, _passiveShotVfxs);

        public void ReturnEnemyProjectile(string name, GameObject go) => Return(go, name, _enemyProjectilesRoot,
            _activeEnemyProjectiles, _passiveEnemyProjectiles);

        public void ReturnHeroProjectile(string name, GameObject go) => Return(go, name, _heroProjectilesRoot,
            _activeHeroProjectiles, _passiveHeroProjectiles);

        public void ReturnShotVfx(string name, GameObject go) =>
            Return(go, name, _shotVfxsRoot, _activeShotVfxs, _passiveShotVfxs);

        private void Return(GameObject go, string name, Transform root, Dictionary<string, List<GameObject>> active,
            Dictionary<string, List<GameObject>> passive)
        {
            if (!passive.ContainsKey(name)) passive[name] = new();
            if (!active.ContainsKey(name)) active[name] = new();

            passive[name].Add(go);
            active[name].Remove(go);

            go.SetActive(false);
            go.transform.SetParent(root);

            if (EnableDebugLog)
                Debug.Log($"[Pool Return] {name} → Active: {active[name].Count}, Passive: {passive[name].Count}");
        }

        private async UniTask<GameObject> GetGameObject(Pools pool, string name,
            Dictionary<string, List<GameObject>> activeDict,
            Dictionary<string, List<GameObject>> passiveDict)
        {
            if (!activeDict.TryGetValue(name, out var activeList))
                activeDict[name] = activeList = new();

            if (!passiveDict.TryGetValue(name, out var passiveList))
                passiveDict[name] = passiveList = new();

            if (passiveList.Count > 0)
            {
                var obj = passiveList[0];
                passiveList.RemoveAt(0);
                activeList.Add(obj);
                return obj;
            }

            // Pool exhausted — create more
            _activeList = activeList;
            _passiveList = passiveList;
            _gameObject = await ExtendList(pool, name);
            passiveList.AddRange(_passiveList);
            passiveList.Remove(_gameObject);
            activeList.Add(_gameObject);

            if (EnableDebugLog)
                Debug.LogWarning(
                    $"[Pool Extend] {name} extended. Total Active: {activeList.Count}, Passive: {passiveList.Count}");

            return _gameObject;
        }

        private async UniTask<GameObject> ExtendList(Pools pool, string name)
        {
            int newCapacity = _activeList.Count + AdditionalCount;
            _tempList = new(newCapacity);
            _tempList.AddRange(_passiveList);

            int toCreate = newCapacity - _activeList.Count;
            for (int i = 0; i < toCreate; i++)
                await CreateObject(pool, name);

            _passiveList = _tempList;
            return _passiveList[0];
        }

        private async UniTask CreateObject(Pools pool, string name)
        {
            GameObject obj = pool switch
            {
                Pools.HeroProjectiles when name == ProjectileTypeId.Grenade.ToString() => await _assets.Instantiate(
                    AssetAddresses.Grenade, _heroProjectilesRoot),
                Pools.HeroProjectiles when name == ProjectileTypeId.RpgRocket.ToString() => await _assets.Instantiate(
                    AssetAddresses.RpgRocket, _heroProjectilesRoot),
                Pools.HeroProjectiles when name == ProjectileTypeId.RocketLauncherRocket.ToString() =>
                    await _assets.Instantiate(AssetAddresses.RocketLauncherRocket, _heroProjectilesRoot),
                Pools.HeroProjectiles when name == ProjectileTypeId.Bomb.ToString() => await _assets.Instantiate(
                    AssetAddresses.Bomb, _heroProjectilesRoot),
                Pools.EnemyProjectiles when name == ProjectileTypeId.PistolBullet.ToString() =>
                    await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot),
                Pools.EnemyProjectiles when name == ProjectileTypeId.RifleBullet.ToString() =>
                    await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot),
                Pools.EnemyProjectiles when name == ProjectileTypeId.Shot.ToString() => await _assets.Instantiate(
                    AssetAddresses.Shot, _enemyProjectilesRoot),
                Pools.ShotVfxs when name == ShotVfxTypeId.Bullet.ToString() => await _assets.Instantiate(
                    AssetAddresses.BulletMuzzleFire, _shotVfxsRoot),
                Pools.ShotVfxs when name == ShotVfxTypeId.Shot.ToString() => await _assets.Instantiate(
                    AssetAddresses.ShotMuzzleFire, _shotVfxsRoot),
                Pools.ShotVfxs when name == ShotVfxTypeId.Grenade.ToString() => await _assets.Instantiate(
                    AssetAddresses.GrenadeMuzzleFire, _shotVfxsRoot),
                Pools.ShotVfxs when name == ShotVfxTypeId.RpgRocket.ToString() => await _assets.Instantiate(
                    AssetAddresses.RpgMuzzleFire, _shotVfxsRoot),
                Pools.ShotVfxs when name == ShotVfxTypeId.RocketLauncherRocket.ToString() => await _assets.Instantiate(
                    AssetAddresses.RocketLauncherMuzzleBlue, _shotVfxsRoot),
                Pools.ShotVfxs when name == ShotVfxTypeId.Bomb.ToString() => await _assets.Instantiate(
                    AssetAddresses.BombMuzzle, _shotVfxsRoot),
                _ => null
            };

            if (obj == null)
            {
                Debug.LogError($"[Pool Error] Could not create object for {pool}/{name}");
                return;
            }

            obj.SetActive(false);
            _tempList.Add(obj);
        }
    }

    public enum Pools
    {
        EnemyProjectiles,
        HeroProjectiles,
        ShotVfxs,
    }
}