using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.ShotVfxs;
using Cysharp.Threading.Tasks;
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
        private const bool EnableDebugLog = false;

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

        public ObjectsPoolService(IAssets assets) => _assets = assets;

        public void GenerateObjects() => CreateRoots().Forget();

        private async UniTask CreateRoots()
        {
            _heroProjectilesRoot = await InstantiateRoot(AssetAddresses.HeroProjectilesRoot);
            _enemyProjectilesRoot = await InstantiateRoot(AssetAddresses.EnemyProjectilesRoot);
            _shotVfxsRoot = await InstantiateRoot(AssetAddresses.ShotVfxsRoot);

            await GenerateHeroProjectiles();
            await GenerateEnemyProjectiles();
            await GenerateShotVfxs();
        }

        private async UniTask<Transform> InstantiateRoot(string address)
        {
            var rootPrefab = await _assets.Load<GameObject>(address);
            if (rootPrefab == null)
            {
                Debug.LogError($"[Pool Error] Could not load root prefab at {address}");
                return null;
            }
            return Object.Instantiate(rootPrefab).transform;
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
            var passiveList = new List<GameObject>(count);
            for (int i = 0; i < count; i++)
            {
                GameObject go = await _assets.Instantiate(assetAddress, parent);
                if (go == null)
                {
                    Debug.LogError($"[Pool Error] Could not instantiate {assetAddress}");
                    continue;
                }
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

        public void ReturnEnemyProjectile(string name, GameObject go) =>
            Return(go, name, _enemyProjectilesRoot, _activeEnemyProjectiles, _passiveEnemyProjectiles);

        public void ReturnHeroProjectile(string name, GameObject go) =>
            Return(go, name, _heroProjectilesRoot, _activeHeroProjectiles, _passiveHeroProjectiles);

        public void ReturnShotVfx(string name, GameObject go) =>
            Return(go, name, _shotVfxsRoot, _activeShotVfxs, _passiveShotVfxs);

        private void Return(GameObject go, string name, Transform root, Dictionary<string, List<GameObject>> active,
            Dictionary<string, List<GameObject>> passive)
        {
            var passiveList = GetOrCreateList(passive, name);
            var activeList = GetOrCreateList(active, name);

            if (!activeList.Remove(go))
            {
                Debug.LogWarning($"[Pool Return] Tried to return object not in active list: {name}");
            }
            passiveList.Add(go);

            go.SetActive(false);
            go.transform.SetParent(root);

            if (EnableDebugLog)
                Debug.Log($"[Pool Return] {name} → Active: {activeList.Count}, Passive: {passiveList.Count}");
        }

        private async UniTask<GameObject> GetGameObject(Pools pool, string name,
            Dictionary<string, List<GameObject>> activeDict,
            Dictionary<string, List<GameObject>> passiveDict)
        {
            var activeList = GetOrCreateList(activeDict, name);
            var passiveList = GetOrCreateList(passiveDict, name);

            if (passiveList.Count > 0)
            {
                var obj = passiveList[0];
                passiveList.RemoveAt(0);
                activeList.Add(obj);
                return obj;
            }

            // Pool exhausted — create more
            var newObjects = await ExtendList(pool, name, activeList.Count, AdditionalCount);
            passiveList.AddRange(newObjects);
            var objToActivate = passiveList[0];
            passiveList.RemoveAt(0);
            activeList.Add(objToActivate);

            if (EnableDebugLog)
                Debug.LogWarning(
                    $"[Pool Extend] {name} extended. Total Active: {activeList.Count}, Passive: {passiveList.Count}");

            return objToActivate;
        }

        private async UniTask<List<GameObject>> ExtendList(Pools pool, string name, int currentCount, int additionalCount)
        {
            var newObjects = new List<GameObject>(additionalCount);
            for (int i = 0; i < additionalCount; i++)
            {
                var obj = await CreateObject(pool, name);
                if (obj != null)
                    newObjects.Add(obj);
            }
            return newObjects;
        }

        private async UniTask<GameObject> CreateObject(Pools pool, string name)
        {
            string assetAddress = pool switch
            {
                Pools.HeroProjectiles when name == ProjectileTypeId.Grenade.ToString() => AssetAddresses.Grenade,
                Pools.HeroProjectiles when name == ProjectileTypeId.RpgRocket.ToString() => AssetAddresses.RpgRocket,
                Pools.HeroProjectiles when name == ProjectileTypeId.RocketLauncherRocket.ToString() => AssetAddresses.RocketLauncherRocket,
                Pools.HeroProjectiles when name == ProjectileTypeId.Bomb.ToString() => AssetAddresses.Bomb,
                Pools.EnemyProjectiles when name == ProjectileTypeId.PistolBullet.ToString() => AssetAddresses.PistolBullet,
                Pools.EnemyProjectiles when name == ProjectileTypeId.RifleBullet.ToString() => AssetAddresses.PistolBullet,
                Pools.EnemyProjectiles when name == ProjectileTypeId.Shot.ToString() => AssetAddresses.Shot,
                Pools.ShotVfxs when name == ShotVfxTypeId.Bullet.ToString() => AssetAddresses.BulletMuzzleFire,
                Pools.ShotVfxs when name == ShotVfxTypeId.Shot.ToString() => AssetAddresses.ShotMuzzleFire,
                Pools.ShotVfxs when name == ShotVfxTypeId.Grenade.ToString() => AssetAddresses.GrenadeMuzzleFire,
                Pools.ShotVfxs when name == ShotVfxTypeId.RpgRocket.ToString() => AssetAddresses.RpgMuzzleFire,
                Pools.ShotVfxs when name == ShotVfxTypeId.RocketLauncherRocket.ToString() => AssetAddresses.RocketLauncherMuzzleBlue,
                Pools.ShotVfxs when name == ShotVfxTypeId.Bomb.ToString() => AssetAddresses.BombMuzzle,
                _ => null
            };

            Transform parent = pool switch
            {
                Pools.HeroProjectiles => _heroProjectilesRoot,
                Pools.EnemyProjectiles => _enemyProjectilesRoot,
                Pools.ShotVfxs => _shotVfxsRoot,
                _ => null
            };

            if (assetAddress == null || parent == null)
            {
                Debug.LogError($"[Pool Error] Invalid asset or parent for {pool}/{name}");
                return null;
            }

            var obj = await _assets.Instantiate(assetAddress, parent);
            if (obj == null)
            {
                Debug.LogError($"[Pool Error] Could not create object for {pool}/{name}");
                return null;
            }

            obj.SetActive(false);
            return obj;
        }

        private List<GameObject> GetOrCreateList(Dictionary<string, List<GameObject>> dict, string key)
        {
            if (!dict.TryGetValue(key, out var list))
            {
                list = new List<GameObject>();
                dict[key] = list;
            }
            return list;
        }
    }
}
