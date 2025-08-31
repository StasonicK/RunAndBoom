using System.Collections.Generic;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.ShotVfxs;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

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
        private int _currentVfxCapacity;
        private int _currentEnemyProjectilesCapacity;
        private int _currentHeroProjectilesCapacity;
        private List<GameObject> _activeList;
        private List<GameObject> _passiveList;
        private List<GameObject> _tempList;

        public ObjectsPoolService(IAssets assets) =>
            _assets = assets;

        public void GenerateObjects() =>
            CreateRoots();

        private async void CreateRoots()
        {
            GameObject root = await _assets.Load<GameObject>(AssetAddresses.HeroProjectilesRoot);
            _gameObject = Object.Instantiate(root);
            _heroProjectilesRoot = _gameObject.transform;

            root = await _assets.Load<GameObject>(AssetAddresses.EnemyProjectilesRoot);
            _gameObject = Object.Instantiate(root);
            _enemyProjectilesRoot = _gameObject.transform;

            root = await _assets.Load<GameObject>(AssetAddresses.ShotVfxsRoot);
            _gameObject = Object.Instantiate(root);
            _shotVfxsRoot = _gameObject.transform;

            GenerateHeroProjectiles();
            GenerateEnemyProjectiles();
            GenerateShotVfxs();
        }

        private async void GenerateEnemyProjectiles()
        {
            _activeEnemyProjectiles = new Dictionary<string, List<GameObject>>();
            _passiveEnemyProjectiles = new Dictionary<string, List<GameObject>>();
            _currentEnemyProjectilesCapacity = InitialEnemyProjectilesCapacity;
            List<GameObject> gameObjects = new List<GameObject>(InitialEnemyProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                _gameObject = await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot);
                _gameObject.SetActive(false);
                gameObjects.Add(_gameObject);
            }

            _passiveEnemyProjectiles.Add(ProjectileTypeId.PistolBullet.ToString(), gameObjects);
            _activeEnemyProjectiles.Add(ProjectileTypeId.PistolBullet.ToString(),
                new List<GameObject>(gameObjects.Count));

            gameObjects = new List<GameObject>(InitialEnemyProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                _gameObject = await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot);
                _gameObject.SetActive(false);
                gameObjects.Add(_gameObject);
            }

            _passiveEnemyProjectiles.Add(ProjectileTypeId.RifleBullet.ToString(), gameObjects);
            _activeEnemyProjectiles.Add(ProjectileTypeId.RifleBullet.ToString(),
                new List<GameObject>(gameObjects.Count));

            gameObjects = new List<GameObject>(InitialEnemyProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                _gameObject = await _assets.Instantiate(AssetAddresses.Shot, _enemyProjectilesRoot);
                _gameObject.SetActive(false);
                gameObjects.Add(_gameObject);
            }

            _passiveEnemyProjectiles.Add(ProjectileTypeId.Shot.ToString(), gameObjects);
            _activeEnemyProjectiles.Add(ProjectileTypeId.Shot.ToString(),
                new List<GameObject>(gameObjects.Count));
        }

        private async void GenerateHeroProjectiles()
        {
            _passiveHeroProjectiles = new Dictionary<string, List<GameObject>>();
            _activeHeroProjectiles = new Dictionary<string, List<GameObject>>();
            _currentHeroProjectilesCapacity = InitialHeroProjectilesCapacity;
            List<GameObject> gameObjects = new List<GameObject>(InitialHeroProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                _gameObject = await _assets.Instantiate(AssetAddresses.Grenade, _heroProjectilesRoot);
                _gameObject.SetActive(false);
                gameObjects.Add(_gameObject);
            }

            _passiveHeroProjectiles.Add(ProjectileTypeId.Grenade.ToString(), gameObjects);
            _activeHeroProjectiles.Add(ProjectileTypeId.Grenade.ToString(),
                new List<GameObject>(gameObjects.Count));

            gameObjects = new List<GameObject>(InitialHeroProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                _gameObject = await _assets.Instantiate(AssetAddresses.RpgRocket, _heroProjectilesRoot);
                _gameObject.SetActive(false);
                gameObjects.Add(_gameObject);
            }

            _passiveHeroProjectiles.Add(ProjectileTypeId.RpgRocket.ToString(), gameObjects);
            _activeHeroProjectiles.Add(ProjectileTypeId.RpgRocket.ToString(), new List<GameObject>(gameObjects.Count));

            gameObjects = new List<GameObject>(InitialHeroProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                _gameObject =
                    await _assets.Instantiate(AssetAddresses.RocketLauncherRocket, _heroProjectilesRoot);
                _gameObject.SetActive(false);
                gameObjects.Add(_gameObject);
            }

            _passiveHeroProjectiles.Add(ProjectileTypeId.RocketLauncherRocket.ToString(), gameObjects);
            _activeHeroProjectiles.Add(ProjectileTypeId.RocketLauncherRocket.ToString(),
                new List<GameObject>(gameObjects.Count));
            gameObjects = new List<GameObject>(InitialHeroProjectilesCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                _gameObject = await _assets.Instantiate(AssetAddresses.Bomb, _heroProjectilesRoot);
                _gameObject.SetActive(false);
                gameObjects.Add(_gameObject);
            }

            _passiveHeroProjectiles.Add(ProjectileTypeId.Bomb.ToString(), gameObjects);
            _activeHeroProjectiles.Add(ProjectileTypeId.Bomb.ToString(), new List<GameObject>(gameObjects.Count));
        }

        private async void GenerateShotVfxs()
        {
            _passiveShotVfxs = new Dictionary<string, List<GameObject>>();
            _activeShotVfxs = new Dictionary<string, List<GameObject>>();
            _currentVfxCapacity = InitialVfxCapacity;
            List<GameObject> gameObjects = new List<GameObject>(InitialVfxCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                _gameObject = await _assets.Instantiate(AssetAddresses.GrenadeMuzzleFire, _shotVfxsRoot);
                _gameObject.SetActive(false);
                gameObjects.Add(_gameObject);
            }

            _passiveShotVfxs.Add(ShotVfxTypeId.Grenade.ToString(), gameObjects);
            _activeShotVfxs.Add(ShotVfxTypeId.Grenade.ToString(), new List<GameObject>(gameObjects.Count));
            gameObjects = new List<GameObject>(InitialVfxCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                _gameObject = await _assets.Instantiate(AssetAddresses.RpgMuzzleFire, _shotVfxsRoot);
                _gameObject.SetActive(false);
                gameObjects.Add(_gameObject);
            }

            _passiveShotVfxs.Add(ShotVfxTypeId.RpgRocket.ToString(), gameObjects);
            _activeShotVfxs.Add(ShotVfxTypeId.RpgRocket.ToString(), new List<GameObject>(gameObjects.Count));
            gameObjects = new List<GameObject>(InitialVfxCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                _gameObject = await _assets.Instantiate(AssetAddresses.RocketLauncherMuzzleBlue, _shotVfxsRoot);
                _gameObject.SetActive(false);
                gameObjects.Add(_gameObject);
            }

            _passiveShotVfxs.Add(ShotVfxTypeId.RocketLauncherRocket.ToString(), gameObjects);
            _activeShotVfxs.Add(ShotVfxTypeId.RocketLauncherRocket.ToString(),
                new List<GameObject>(gameObjects.Count));
            gameObjects = new List<GameObject>(InitialVfxCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                _gameObject = await _assets.Instantiate(AssetAddresses.BombMuzzle, _shotVfxsRoot);
                _gameObject.SetActive(false);
                gameObjects.Add(_gameObject);
            }

            _passiveShotVfxs.Add(ShotVfxTypeId.Bomb.ToString(), gameObjects);
            _activeShotVfxs.Add(ShotVfxTypeId.Bomb.ToString(), new List<GameObject>(gameObjects.Count));
            gameObjects = new List<GameObject>(InitialVfxCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                _gameObject = await _assets.Instantiate(AssetAddresses.BulletMuzzleFire, _shotVfxsRoot);
                _gameObject.SetActive(false);
                gameObjects.Add(_gameObject);
            }

            _passiveShotVfxs.Add(ShotVfxTypeId.Bullet.ToString(), gameObjects);
            _activeShotVfxs.Add(ShotVfxTypeId.Bullet.ToString(), new List<GameObject>(gameObjects.Count));
            gameObjects = new List<GameObject>(InitialVfxCapacity);

            for (int i = 0; i < gameObjects.Capacity; i++)
            {
                _gameObject = await _assets.Instantiate(AssetAddresses.ShotMuzzleFire, _shotVfxsRoot);
                _gameObject.SetActive(false);
                gameObjects.Add(_gameObject);
            }

            _passiveShotVfxs.Add(ShotVfxTypeId.Shot.ToString(), gameObjects);
            _activeShotVfxs.Add(ShotVfxTypeId.Shot.ToString(), new List<GameObject>(gameObjects.Count));
        }

        public async UniTask<GameObject> GetEnemyProjectile(string name) =>
            await GetGameObject(Pools.EnemyProjectiles, name, _activeEnemyProjectiles, _passiveEnemyProjectiles);

        public async UniTask<GameObject> GetHeroProjectile(string name) =>
            await GetGameObject(Pools.HeroProjectiles, name, _activeHeroProjectiles, _passiveHeroProjectiles);

        public async UniTask<GameObject> GetShotVfx(ShotVfxTypeId typeId) =>
            await GetGameObject(Pools.ShotVfxs, typeId.ToString(), _activeShotVfxs, _passiveShotVfxs);

        public void ReturnEnemyProjectile(string name, GameObject gameObject)
        {
            _passiveEnemyProjectiles[name].Add(gameObject);
            _activeEnemyProjectiles[name].Remove(gameObject);
            ReturnGameObject(gameObject, _enemyProjectilesRoot);
        }

        public void ReturnHeroProjectile(string name, GameObject gameObject)
        {
            _passiveHeroProjectiles[name].Add(gameObject);
            _activeHeroProjectiles[name].Remove(gameObject);
            ReturnGameObject(gameObject, _heroProjectilesRoot);
        }

        public void ReturnShotVfx(string name, GameObject gameObject)
        {
            _passiveShotVfxs[name].Add(gameObject);
            _activeShotVfxs[name].Remove(gameObject);
            ReturnGameObject(gameObject, _shotVfxsRoot);
        }

        private void ReturnGameObject(GameObject gameObject, Transform parent)
        {
            gameObject.SetActive(false);
            gameObject.transform.SetParent(parent);
        }

        private async UniTask<GameObject> GetGameObject(Pools pool, string name,
            Dictionary<string, List<GameObject>> activeDictionary,
            Dictionary<string, List<GameObject>> passiveDictionary)
        {
            activeDictionary.TryGetValue(name, out List<GameObject> activeList);
            passiveDictionary.TryGetValue(name, out List<GameObject> passiveList);
            _gameObject = null;

            if (passiveList != null && activeList != null)
            {
                if (passiveList.Count != 0)
                {
                    _gameObject = passiveList[0];
                    passiveList.Remove(_gameObject);
                    activeList.Add(_gameObject);
                    return _gameObject;
                }
                else
                {
                    _activeList = activeList;
                    _passiveList = passiveList;
                    _gameObject = await ExtendList(pool, name);
                    passiveList.AddRange(_passiveList);
                    activeList.Add(_gameObject);
                    passiveList.Remove(_gameObject);
                    return _gameObject;
                }
            }

            return _gameObject;
        }

        private async UniTask<GameObject> ExtendList(Pools pool, string name)
        {
            int newCapacity = _activeList.Capacity + AdditionalCount;
            _tempList = new List<GameObject>(newCapacity);
            _tempList.AddRange(_passiveList);
            int difference = newCapacity - _activeList.Count;

            for (int i = 0; i < difference; i++)
                _gameObject = await CreateObject(pool, name);

            _passiveList = _tempList;
            return _passiveList[0];
        }

        private async UniTask<GameObject> CreateObject(Pools pool, string name)
        {
            _gameObject = null;

            switch (pool)
            {
                case Pools.HeroProjectiles when name == ProjectileTypeId.Grenade.ToString():
                    _gameObject = await _assets.Instantiate(AssetAddresses.Grenade, _heroProjectilesRoot);
                    break;
                case Pools.HeroProjectiles when name == ProjectileTypeId.RpgRocket.ToString():
                    _gameObject = await _assets.Instantiate(AssetAddresses.RpgRocket, _heroProjectilesRoot);
                    break;
                case Pools.HeroProjectiles when name == ProjectileTypeId.RocketLauncherRocket.ToString():
                    _gameObject = await _assets.Instantiate(AssetAddresses.RocketLauncherRocket, _heroProjectilesRoot);
                    break;
                case Pools.HeroProjectiles when name == ProjectileTypeId.Bomb.ToString():
                    _gameObject = await _assets.Instantiate(AssetAddresses.Bomb, _heroProjectilesRoot);
                    break;
                case Pools.EnemyProjectiles when name == ProjectileTypeId.None.ToString():
                case Pools.ShotVfxs when name == ShotVfxTypeId.None.ToString():
                    break;
                case Pools.EnemyProjectiles when name == ProjectileTypeId.PistolBullet.ToString():
                case Pools.EnemyProjectiles when name == ProjectileTypeId.RifleBullet.ToString():
                    _gameObject = await _assets.Instantiate(AssetAddresses.PistolBullet, _enemyProjectilesRoot);
                    break;
                case Pools.EnemyProjectiles when name == ProjectileTypeId.Shot.ToString():
                    _gameObject = await _assets.Instantiate(AssetAddresses.Shot, _enemyProjectilesRoot);
                    break;
                case Pools.ShotVfxs when name == ShotVfxTypeId.Bullet.ToString():
                    _gameObject = await _assets.Instantiate(AssetAddresses.BulletMuzzleFire, _shotVfxsRoot);
                    break;
                case Pools.ShotVfxs when name == ShotVfxTypeId.Shot.ToString():
                    _gameObject = await _assets.Instantiate(AssetAddresses.ShotMuzzleFire, _shotVfxsRoot);
                    break;
                case Pools.ShotVfxs when name == ShotVfxTypeId.Grenade.ToString():
                    _gameObject = await _assets.Instantiate(AssetAddresses.BulletMuzzleFire, _shotVfxsRoot);
                    break;
                case Pools.ShotVfxs when name == ShotVfxTypeId.RpgRocket.ToString():
                    _gameObject = await _assets.Instantiate(AssetAddresses.RpgMuzzleFire, _shotVfxsRoot);
                    break;
                case Pools.ShotVfxs when name == ShotVfxTypeId.RocketLauncherRocket.ToString():
                    _gameObject = await _assets.Instantiate(AssetAddresses.RocketLauncherMuzzleBlue, _shotVfxsRoot);
                    break;
                case Pools.ShotVfxs when name == ShotVfxTypeId.Bomb.ToString():
                    _gameObject = await _assets.Instantiate(AssetAddresses.BombMuzzle, _shotVfxsRoot);
                    break;
            }

            while (_gameObject == null)
                UniTask.Yield();

            _gameObject.SetActive(false);
            _tempList.Add(_gameObject);
            return _gameObject;
        }
    }

    enum Pools
    {
        EnemyProjectiles,
        HeroProjectiles,
        ShotVfxs,
    }
}