using Agava.WebUtility;
using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using NTC.Global.Cache;
using Plugins.SoundInstance.Core.Static;

namespace CodeBase.UI
{
    public class AudioBackgroundChanger : MonoCache
    {
        private IPlayerProgressService _playerProgressService;

        private void Awake()
        {
            _playerProgressService = AllServices.Container.Single<IPlayerProgressService>();
            DontDestroyOnLoad(this);
        }

        protected override void OnEnabled() =>
            WebApplication.InBackgroundChangeEvent += OnInBackgroundChange;

        protected override void OnDisabled() =>
            WebApplication.InBackgroundChangeEvent -= OnInBackgroundChange;

        private void OnInBackgroundChange(bool inBackground)
        {
            if (inBackground)
            {
                SoundInstance.musicVolume = Constants.Zero;
                SoundInstance.GetMusicSource().volume = Constants.Zero;
            }
            else
            {
                SoundInstance.musicVolume = _playerProgressService.Progress.SettingsData.MusicVolume;
                SoundInstance.GetMusicSource().volume = _playerProgressService.Progress.SettingsData.MusicVolume;
            }

            // Debug.Log($"saved volume {_playerProgressService.Progress.SettingsData.MusicVolume}");
            // Debug.Log($"current music volume {SoundInstance.musicVolume}");
            // Debug.Log($"current volume {SoundInstance.GetMusicSource().volume}");
        }
    }
}