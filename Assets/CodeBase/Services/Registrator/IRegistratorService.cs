using System.Collections.Generic;
using CodeBase.Services.PersistentProgress;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Services.Registrator
{
    public interface IRegistratorService : IService
    {
        List<IProgressReader> ProgressReaders { get; }
        List<IProgressSaver> ProgressWriters { get; }
        GameObject InstantiateRegistered(GameObject prefab);
        GameObject InstantiateRegistered(GameObject prefab, Vector3 at);
        UniTask<GameObject> InstantiateRegisteredAsync(string prefabPath, Vector3 at);
        UniTask<GameObject> InstantiateRegisteredAsync(string prefabPath, Transform parent);
        UniTask<GameObject> InstantiateRegisteredAsync(string prefabPath);
        UniTask<GameObject> LoadRegisteredAsync(string prefabPath);
        void RegisterProgressWatchers(GameObject gameObject);
    }
}