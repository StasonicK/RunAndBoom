using System;
using System.Collections;
using CodeBase.Services;
using CodeBase.Services.SaveLoad;
using NTC.Global.Cache;
using NTC.Global.System;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;

namespace CodeBase.Infrastructure
{
    public class LoadingCurtain : MonoCache, ILoadingCurtain
    {
        [SerializeField] private CanvasGroup _curtain;

        private const int MinimumAlpha = 0;
        private const int MaximumAlpha = 1;
        private const float StepAlpha = 0.03f;
        private const float PrepareWaiting = 2f;
        private bool _isInitial = true;

        public event Action FadedOut;

        private void Awake() =>
            DontDestroyOnLoad(this);

        public void Show()
        {
            gameObject.Enable();
            _curtain.alpha = MaximumAlpha;
        }

        public void Hide()
        {
            SoundInstance.SetStartFade();
            SoundInstance.StartRandomMusic();
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            yield return new WaitForSeconds(PrepareWaiting);

            while (_curtain.alpha > MinimumAlpha)
            {
                _curtain.alpha -= StepAlpha;
                yield return new WaitForSeconds(StepAlpha);
            }

            FadedOut?.Invoke();
            AllServices.Container.Single<ISaveLoadService>().SaveProgress();
            gameObject.Disable();
        }
    }
}