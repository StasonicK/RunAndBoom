using System;

namespace CodeBase.Data.Settings
{
    [Serializable]
    public class SettingsData
    {
        public float MusicVolume;
        public float SoundVolume;
        public bool MusicOn;
        public bool SoundOn;
        public Language Language;
        public float AimSensitiveMultiplier = 2f;

        public event Action MusicVolumeChanged;
        public event Action SoundVolumeChanged;
        public event Action MusicSwitchChanged;
        public event Action SoundSwitchChanged;
        public event Action AimSensitiveMultiplierChanged;

        public SettingsData(Language language)
        {
            SetMusicVolume(Constants.InitialMusicVolume);
            SetSoundVolume(Constants.InitialSoundVolume);
            SetMusicSwitch(true);
            SetSoundSwitch(true);
            SetAimSensitiveMultiplier(Constants.InitialAimSliderValue);
            SetLanguage(language);
        }

        public void SetMusicVolume(float volume)
        {
            if (MusicVolume == volume)
                return;

            MusicVolume = volume;
            MusicVolumeChanged?.Invoke();
        }

        public void SetSoundVolume(float volume)
        {
            if (SoundVolume == volume)
                return;

            SoundVolume = volume;
            SoundVolumeChanged?.Invoke();
        }

        public void SetMusicSwitch(bool switcher)
        {
            if (MusicOn == switcher)
                return;

            MusicOn = switcher;
            MusicSwitchChanged?.Invoke();
        }

        public void SetSoundSwitch(bool switcher)
        {
            if (SoundOn == switcher)
                return;

            SoundOn = switcher;
            SoundSwitchChanged?.Invoke();
        }

        public void SetLanguage(Language language)
        {
            if (Language == language)
                return;

            Language = language;
        }

        public void SetAimSensitiveMultiplier(float value)
        {
            if (AimSensitiveMultiplier == value)
                return;

            AimSensitiveMultiplier = value;
            AimSensitiveMultiplierChanged?.Invoke();
        }

    }
}