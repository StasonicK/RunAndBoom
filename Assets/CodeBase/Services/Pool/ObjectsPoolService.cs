using System;
using System.Collections.Generic;
using System.Threading;
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

        private readonly IAssets _assets;

        private Dictionary<string, List<GameObject>> _activeHeroProjectiles;
        private Dictionary<string, List<GameObject>> _passiveHeroProjectiles;
        private Dictionary<string, List<GameObject>> _activeEnemyProjectiles;
        private Dictionary<string, List<GameObject>> _passiveEnemyProjectiles;
        private Dictionary<string, List<GameObject>> _activeShotVfxs;
        private Dictionary<string, List<GameObject>> _passiveShotVfxs;

        private Transform _enemyProjectilesRoot;
        private Transform _heroProjectilesRoot;
        private Transform _shotVfxsRoot;

        // Per-pool-key async locks to avoid concurrent extension races
        private readonly Dictionary<string, SemaphoreSlim> _poolLocks = new();

        private const bool EnableDebugLog = false;

        public ObjectsPoolService(IAssets assets) => _assets = assets;

        public void GenerateObjects() => CreateRoots().Forget();

        private async UniTask CreateRoots()
        {
            GameObject root = await _assets.Load<GameObject>(AssetAddresses.HeroProjectilesRoot);
            _heroProjectilesRoot = Object.Instantiate(root).transform;

            root = await _assets.Load<GameObject>(AssetAddresses.EnemyProjectilesRoot);
            _enemyProjectilesRoot = Object.Instantiate(root).transform;

            root = await _assets.Load<GameObject>(AssetAddresses.ShotVfxsRoot);
            _shotVfxsRoot = Object.Instantiate(root).transform;

            await GenerateHeroProjectiles();
            await GenerateEnemyProjectiles();
            await GenerateShotVfxs();
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
            await AddProjectileType(_passiveShotVfxs, _activeShotVfxs, ShotVfxTypeId.Bomb,
                AssetAddresses.BombMuzzle, _shotVfxsRoot, InitialVfxCapacity);
            await AddProjectileType(_passiveShotVfxs, _activeShotVfxs, ShotVfxTypeId.Bullet,
                AssetAddresses.BulletMuzzleFire, _shotVfxsRoot, InitialVfxCapacity);
            await AddProjectileType(_passiveShotVfxs, _activeShotVfxs, ShotVfxTypeId.Shot,
                AssetAddresses.ShotMuzzleFire, _shotVfxsRoot, InitialVfxCapacity);
        }

        private async UniTask AddProjectileType(
            Dictionary<string, List<GameObject>> passive,
            Dictionary<string, List<GameObject>> active,
            Enum typeId, string assetAddress, Transform parent, int count)
        {
            string key = typeId.ToString();
            var passiveList = new List<GameObject>(count);
            for (int i = 0; i < count; i++)
            {
                GameObject go = await _assets.Instantiate(assetAddress, parent);
                go.SetActive(false);
                passiveList.Add(go);
            }
            passive[key] = passiveList;
            active[key] = new List<GameObject>(count);

            // init lock for this key
            GetOrCreateLock(key);
        }

        public async UniTask<GameObject> GetEnemyProjectile(string name) =>
            await GetGameObject(Pools.EnemyProjectiles, name, _activeEnemyProjectiles, _passiveEnemyProjectiles);

        public async UniTask<GameObject> GetHeroProjectile(string name) =>
            await GetGameObject(Pools.HeroProjectiles, name, _activeHeroProjectiles, _passiveHeroProjectiles);

        public async UniTask<GameObject> GetShotVfx(ShotVfxTypeId typeId) =>
            await GetGameObject(Pools.ShotVfxs, typeId.ToString(), _activeShotVfxs, _passiveShotVfxs);

        public void ReturnEnemyProjectile(string name, GameObject go) =>
            Return(go, name, _enemyProjectilesRoot, _activeEnemyProjectiles, _passiveEnemyProjectiles);

        public void ReturnHeroProjectile(string name, GameObject go) =>
            Return(go, name, _heroProjectilesRoot, _activeHeroProjectiles, _passiveHeroProjectiles);

        public void ReturnShotVfx(string name, GameObject go) =>
            Return(go, name, _shotVfxsRoot, _activeShotVfxs, _passiveShotVfxs);

        private void Return(
            GameObject go, string name, Transform root,
            Dictionary<string, List<GameObject>> active,
            Dictionary<string, List<GameObject>> passive)
        {
            if (!passive.TryGetValue(name, out var passiveList))
            {
                passiveList = new List<GameObject>();
                passive[name] = passiveList;
            }
            if (!active.TryGetValue(name, out var activeList))
            {
                activeList = new List<GameObject>();
                active[name] = activeList;
            }

            // Avoid duplicates
            if (!passiveList.Contains(go))
                passiveList.Add(go);
            // Remove if present in active
            int idx = activeList.IndexOf(go);
            if (idx >= 0)
                activeList.RemoveAt(idx);

            go.SetActive(false);
            go.transform.SetParent(root);

            if (EnableDebugLog)
                Debug.Log($"[Pool Return] {name} → Active: {activeList.Count}, Passive: {passiveList.Count}");
        }

        private async UniTask<GameObject> GetGameObject(
            Pools pool, string name,
            Dictionary<string, List<GameObject>> activeDict,
            Dictionary<string, List<GameObject>> passiveDict)
        {
            if (!activeDict.TryGetValue(name, out var activeList))
                activeDict[name] = activeList = new();
            if (!passiveDict.TryGetValue(name, out var passiveList))
                passiveDict[name] = passiveList = new();

            // Fast path: take from passive (pop-back = O(1))
            int last = passiveList.Count - 1;
            if (last >= 0)
            {
                var obj = passiveList[last];
                passiveList.RemoveAt(last);
                activeList.Add(obj);
                return obj;
            }

            // Slow path: extend pool (serialized per key)
            var sem = GetOrCreateLock(name);
            await sem.WaitAsync();
            try
            {
                // Another waiter may have already extended / returned items
                last = passiveList.Count - 1;
                if (last >= 0)
                {
                    var obj = passiveList[last];
                    passiveList.RemoveAt(last);
                    activeList.Add(obj);
                    return obj;
                }

                // Actually create more
                await ExtendList(pool, name, passiveList, AdditionalCount);
                if (passiveList.Count == 0)
                {
                    Debug.LogError($"[Pool Error] Extension produced no items for {pool}/{name}");
                    return null;
                }

                var newObj = passiveList[^1];
                passiveList.RemoveAt(passiveList.Count - 1);
                activeList.Add(newObj);

                if (EnableDebugLog)
                    Debug.LogWarning($"[Pool Extend] {name} extended. Active: {activeList.Count}, Passive: {passiveList.Count}");

                return newObj;
            }
            finally
            {
                sem.Release();
            }
        }

        private async UniTask ExtendList(Pools pool, string name, List<GameObject> passiveList, int countToCreate)
        {
            for (int i = 0; i < countToCreate; i++)
            {
                GameObject obj = await CreateObject(pool, name);
                if (obj != null)
                {
                    obj.SetActive(false);
                    passiveList.Add(obj);
                }
            }
        }

        private async UniTask<GameObject> CreateObject(Pools pool, string name)
        {
            GameObject obj = pool switch
            {
                Pools.HeroProjectiles when name == ProjectileTypeId.Grenade.ToString()
                    => await _assets.Instantiate(AssetAddresses.Grenade, _heroProjectilesRoot),

                Pools.HeroProjectiles when name == ProjectileTypeId.RpgRocket.ToString()
                    => await _assets.Instantiate(AssetAddresses.RpgRocket, _heroProjectilesRoot),

                Pools.HeroProjectiles when name == ProjectileTypeId.RocketLauncherRocket.ToString()
                    => await _assets.Instantiate(AssetAddresses.RocketLauncherRocket, _heroProjectilesRoot),

                Pools.HeroProjectiles when name == ProjectileTypeId.Bomb.ToString()
                    => await _assets.Instantiate(AssetAddresses.Bomb, _heroProjectilesRoot),

                Pools.EnemyProjectiles when name == ProjectileTypeId.PistolBullet.ToString()
                    => await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot),

                Pools.EnemyProjectiles when name == ProjectileTypeId.RifleBullet.ToString()
                    => await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot),

                Pools.EnemyProjectiles when name == ProjectileTypeId.Shot.ToString()
                    => await _assets.Instantiate(AssetAddresses.Shot, _enemyProjectilesRoot),

                Pools.ShotVfxs when name == ShotVfxTypeId.Bullet.ToString()
                    => await _assets.Instantiate(AssetAddresses.BulletMuzzleFire, _shotVfxsRoot),

                Pools.ShotVfxs when name == ShotVfxTypeId.Shot.ToString()
                    => await _assets.Instantiate(AssetAddresses.ShotMuzzleFire, _shotVfxsRoot),

                Pools.ShotVfxs when name == ShotVfxTypeId.Grenade.ToString()
                    => await _assets.Instantiate(AssetAddresses.GrenadeMuzzleFire, _shotVfxsRoot),

                Pools.ShotVfxs when name == ShotVfxTypeId.RpgRocket.ToString()
                    => await _assets.Instantiate(AssetAddresses.RpgMuzzleFire, _shotVfxsRoot),

                Pools.ShotVfxs when name == ShotVfxTypeId.RocketLauncherRocket.ToString()
                    => await _assets.Instantiate(AssetAddresses.RocketLauncherMuzzleBlue, _shotVfxsRoot),

                Pools.ShotVfxs when name == ShotVfxTypeId.Bomb.ToString()
                    => await _assets.Instantiate(AssetAddresses.BombMuzzle, _shotVfxsRoot),

                _ => null
            };

            if (obj == null)
                Debug.LogError($"[Pool Error] Could not create object for {pool}/{name}");

            return obj;
        }

        private SemaphoreSlim GetOrCreateLock(string key)
        {
            if (!_poolLocks.TryGetValue(key, out var sem))
            {
                sem = new SemaphoreSlim(1, 1);
                _poolLocks[key] = sem;
            }
            return sem;
        }
    }

    public enum Pools
    {
        EnemyProjectiles,
        HeroProjectiles,
        ShotVfxs,
    }
}
