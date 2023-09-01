using CodeBase.Data;
using CodeBase.Services.PersistentProgress;
using CodeBase.UI.Services;
using NTC.Global.Cache;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Settings
{
    public abstract class AudioButton : MonoCache, IProgressReader
    {
        [SerializeField] private Button _button;
        [SerializeField] protected Image ImageSelected;
        [SerializeField] protected Image ImageUnselected;

        protected PlayerProgress Progress;
        protected bool IsSelected;
        private float _volume;
        private AudioSource _audioSource;

        private void Awake() =>
            _audioSource = Get<AudioSource>();

        protected override void OnEnabled() =>
            _button.onClick.AddListener(ButtonPressed);

        protected override void OnDisabled() =>
            _button.onClick.RemoveListener(ButtonPressed);

        private void ButtonPressed()
        {
            ButtonClickAudio();
            SwitchAudio();
            ButtonClickAudio();
            ChangeImage();
        }

        public void LoadProgress(PlayerProgress progress)
        {
            Progress = progress;
            Progress.SettingsData.SoundSwitchChanged += SwitchChanged;
            Progress.SettingsData.SoundVolumeChanged += VolumeChanged;
            VolumeChanged();
            SwitchChanged();
            SetSelection();
            ChangeImage();
        }

        private void VolumeChanged() =>
            _volume = Progress.SettingsData.SoundVolume;

        private void SwitchChanged() =>
            _volume = Progress.SettingsData.SoundOn ? Progress.SettingsData.SoundVolume : Constants.Zero;

        private void ChangeImage()
        {
            if (IsSelected)
            {
                ImageSelected.ChangeImageAlpha(Constants.Visible);
                ImageUnselected.ChangeImageAlpha(Constants.Invisible);
            }
            else
            {
                ImageUnselected.ChangeImageAlpha(Constants.Visible);
                ImageSelected.ChangeImageAlpha(Constants.Invisible);
            }
        }

        private void ButtonClickAudio()
        {
            if (_volume != Constants.Zero)
                SoundInstance.InstantiateOnTransform(
                    audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.CheckboxClick), transform: transform,
                    _volume, _audioSource);
        }

        protected abstract void SwitchAudio();
        protected abstract void SetSelection();
    }
}