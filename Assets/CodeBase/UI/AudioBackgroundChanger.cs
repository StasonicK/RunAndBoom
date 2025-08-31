using BananaParty.WebUtility;
using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;

namespace CodeBase.UI
{
    public class AudioBackgroundChanger : MonoBehaviour
    {
        private IPlayerProgressService _playerProgressService;

        private void Awake()
        {
            _playerProgressService = AllServices.Container.Single<IPlayerProgressService>();
            DontDestroyOnLoad(this);
        }

        private void OnEnable() =>
            WebApplication.InBackgroundChangeEvent += OnInBackgroundChange;

        private void OnDisable() =>
            WebApplication.InBackgroundChangeEvent -= OnInBackgroundChange;

        private void OnInBackgroundChange(bool inBackground)
        {
            if (_playerProgressService == null)
                _playerProgressService = AllServices.Container.Single<IPlayerProgressService>();

            AudioListener.pause = inBackground;
            AudioListener.volume = inBackground ? 0f : 1f;

            if (inBackground)
            {
                SoundInstance.musicVolume = Constants.Zero;
                SoundInstance.GetMusicSource().volume = Constants.Zero;
            }
            else
            {
                SoundInstance.musicVolume = _playerProgressService.SettingsData.MusicVolume;
                SoundInstance.GetMusicSource().volume = _playerProgressService.SettingsData.MusicVolume;
            }
        }
    }
}