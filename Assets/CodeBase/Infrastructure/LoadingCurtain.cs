using System;
using CodeBase.Services;
using CodeBase.Services.Ads;
using CodeBase.Services.SaveLoad;
using Cysharp.Threading.Tasks;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;

namespace CodeBase.Infrastructure
{
    public class LoadingCurtain : MonoBehaviour, ILoadingCurtain
    {
        [SerializeField] private CanvasGroup _curtain;

        private const int MinimumAlpha = 0;
        private const int MaximumAlpha = 1;
        private const float StepAlpha = 0.03f;
        private const float PrepareWaiting = 2f;
        private bool _isInitial = true;
        private ISaveLoadService _saveLoadService;
        private WaitForSeconds _waitForSeconds;
        private WaitForSeconds _forSeconds;
        private IAdsService _adsService;

        public event Action FadedOut;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            _adsService = AllServices.Container.Single<IAdsService>();
            _waitForSeconds = new WaitForSeconds(PrepareWaiting);
            _forSeconds = new WaitForSeconds(StepAlpha);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _curtain.alpha = MaximumAlpha;
        }

        public void Hide()
        {
            SoundInstance.SetStartFade();
            SoundInstance.StartRandomMusic();
            // StartCoroutine(FadeOut());
            // FadeOut().Forget();
            FadedOut?.Invoke();
            _isInitial = false;
            SaveData();
            gameObject.SetActive(false);
        }

        // private IEnumerator FadeOut()
        // {
        //     yield return _waitForSeconds;
        //
        //     while (_curtain.alpha > MinimumAlpha)
        //     {
        //         if (_isInitial && !Application.isEditor)
        //         {
        //             if (_adsService != null)
        //             {
        //                 if (_adsService.IsInitialized())
        //                     AllServices.Container.Single<IGameReadyService>().GameReady();
        //             }
        //         }
        //     
        //         _curtain.alpha -= StepAlpha;
        //         yield return _forSeconds;
        //     }
        //
        //     FadedOut?.Invoke();
        //     _isInitial = false;
        //     SaveData();
        //     gameObject.SetActive(false);
        // }
        
        private async UniTask FadeOut()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(PrepareWaiting));

            float fadeDuration = 2f;
            float elapsed = 0f;
            float startAlpha = _curtain.alpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                _curtain.alpha = Mathf.Lerp(startAlpha, MinimumAlpha, elapsed / fadeDuration);
                await UniTask.Yield();
            }

            _curtain.alpha = MinimumAlpha;
            FadedOut?.Invoke();
            _isInitial = false;
            SaveData();
            await UniTask.Yield(); // Ensure the log is processed before deactivating the object
            gameObject.SetActive(false);
        }

        private void SaveData()
        {
            if (_saveLoadService == null)
                _saveLoadService = AllServices.Container.Single<ISaveLoadService>();

            _saveLoadService.SaveProgressData();
            _saveLoadService.SaveSettingsData();
        }
    }
}