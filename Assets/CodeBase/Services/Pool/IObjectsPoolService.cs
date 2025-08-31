using CodeBase.StaticData.ShotVfxs;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Services.Pool
{
    public interface IObjectsPoolService : IService
    {
        void GenerateObjects();
        UniTask<GameObject> GetEnemyProjectile(string name);
        UniTask<GameObject> GetHeroProjectile(string name);
        UniTask<GameObject> GetShotVfx(ShotVfxTypeId typeId);
        void ReturnEnemyProjectile(string name, GameObject gameObject);
        void ReturnHeroProjectile(string name, GameObject gameObject);
        void ReturnShotVfx(string name, GameObject gameObject);
    }
}