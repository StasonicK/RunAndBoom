using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = CodeBase.Data.Scene;

namespace CodeBase.Infrastructure
{
    public class SceneLoader : ISceneLoader
    {
        private readonly ICoroutineRunner _coroutineRunner;

        public SceneLoader(ICoroutineRunner coroutineRunner) =>
            _coroutineRunner = coroutineRunner;

        public void Load(Scene scene, Action<Scene> onLoaded = null) =>
            _coroutineRunner.StartCoroutine(LoadScene(scene, onLoaded));

        public void Load(Scene scene, Action onLoaded = null) =>
            _coroutineRunner.StartCoroutine(LoadScene(scene, onLoaded));

        private IEnumerator LoadScene(Scene nextScene, Action<Scene> onLoaded = null)
        {
            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(nextScene.ToString());

            while (!waitNextScene.isDone)
                yield return null;

            onLoaded?.Invoke(nextScene);
        }

        private IEnumerator LoadScene(Scene nextScene, Action onLoaded = null)
        {
            if (SceneManager.GetActiveScene().name == nextScene.ToString())
            {
                onLoaded?.Invoke();
                yield break;
            }

            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(nextScene.ToString());

            while (!waitNextScene.isDone)
                yield return null;

            onLoaded?.Invoke();
        }
    }
}