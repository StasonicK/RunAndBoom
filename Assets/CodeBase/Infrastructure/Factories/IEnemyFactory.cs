using CodeBase.Services;
using CodeBase.StaticData.Enemies;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Infrastructure.Factories
{
    public interface IEnemyFactory : IService
    {
        void CreateSpawnersRoot();

        UniTask CreateSpawner(Vector3 at, EnemyTypeId enemyTypeId
            // , AreaData area
        );

        UniTask<GameObject> CreateEnemy(EnemyTypeId typeId, Transform parent
            // , AreaData area
        );
    }
}