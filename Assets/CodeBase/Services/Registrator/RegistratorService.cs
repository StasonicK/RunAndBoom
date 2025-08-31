using System.Collections.Generic;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.PersistentProgress;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Services.Registrator
{
    public class RegistratorService : IRegistratorService
    {
        private IAssets _assets;
        public List<IProgressReader> ProgressReaders { get; } = new List<IProgressReader>();
        public List<IProgressSaver> ProgressWriters { get; } = new List<IProgressSaver>();

        public RegistratorService(IAssets assets) =>
            _assets = assets;

        public GameObject InstantiateRegistered(GameObject prefab)
        {
            GameObject gameObject = Object.Instantiate(prefab);
            Debug.Log("InstantiateRegistered " + gameObject.name);
            RegisterProgressWatchers(gameObject);
            return gameObject;
        }

        public GameObject InstantiateRegistered(GameObject prefab, Vector3 at)
        {
            GameObject gameObject = Object.Instantiate(prefab, at, Quaternion.identity);
            Debug.Log("InstantiateRegistered " + gameObject.name);
            RegisterProgressWatchers(gameObject);
            return gameObject;
        }

        public async UniTask<GameObject> InstantiateRegisteredAsync(string prefabPath)
        {
            GameObject gameObject = await _assets.Instantiate(prefabPath);
            Debug.Log("InstantiateRegisteredAsync " + gameObject.name);
            RegisterProgressWatchers(gameObject);
            return gameObject;
        }

        public async UniTask<GameObject> InstantiateRegisteredAsync(string prefabPath, Vector3 at)
        {
            GameObject gameObject = await _assets.Instantiate(prefabPath, at: at);
            Debug.Log("InstantiateRegisteredAsync " + gameObject.name);
            RegisterProgressWatchers(gameObject);
            return gameObject;
        }

        public async UniTask<GameObject> InstantiateRegisteredAsync(string prefabPath, Transform parent)
        {
            GameObject gameObject = await _assets.Instantiate(prefabPath, parent: parent);
            Debug.Log("InstantiateRegisteredAsync " + gameObject.name);
            RegisterProgressWatchers(gameObject);
            return gameObject;
        }

        public async UniTask<GameObject> LoadRegisteredAsync(string prefabPath)
        {
            GameObject gameObject = await _assets.Load<GameObject>(prefabPath);
            return gameObject;
        }

        public void RegisterProgressWatchers(GameObject gameObject)
        {
            foreach (IProgressReader progressReader in gameObject.GetComponentsInChildren<IProgressReader>())
                Register(progressReader);
        }

        private void Register(IProgressReader progressReader)
        {
            if (progressReader is IProgressSaver progressWriter)
                ProgressWriters.Add(progressWriter);

            ProgressReaders.Add(progressReader);
        }
    }
}