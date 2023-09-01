using CodeBase.Infrastructure.Factories;
using CodeBase.Services;
using CodeBase.StaticData.Enemies;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Logic.EnemySpawners
{
    public class SpawnPoint : MonoCache
    {
        [SerializeField] private EnemyTypeId _enemyTypeId;

        private IEnemyFactory _factory;

        private void Awake() =>
            _factory = AllServices.Container.Single<IEnemyFactory>();

        public void Construct(EnemyTypeId enemyTypeId) =>
            _enemyTypeId = enemyTypeId;

        public void Initialize() =>
            Spawn();

        private async void Spawn() =>
            await _factory.CreateEnemy(_enemyTypeId, transform);
    }
}