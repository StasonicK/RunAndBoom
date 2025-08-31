using CodeBase.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CodeBase.Infrastructure.AssetManagement
{
    public interface IAssets : IService
    {
        void Initialize();
        UniTask<GameObject> Instantiate(string address);
        UniTask<GameObject> Instantiate(string address, Vector3 at);
        UniTask<GameObject> Instantiate(string address, Transform parent);
        UniTask<T> Load<T>(AssetReference assetReference) where T : class;
        UniTask<T> Load<T>(string address) where T : class;

        void CleanUp();
    }
}