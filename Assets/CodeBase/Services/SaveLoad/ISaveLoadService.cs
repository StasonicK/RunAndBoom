using CodeBase.Data.Progress;
using CodeBase.Data.Settings;

namespace CodeBase.Services.SaveLoad
{
    public interface ISaveLoadService : IService
    {
        void SaveProgressData();
        void SaveSettingsData();
        void SaveMusicOn(bool musicOn);
        void SaveSoundOn(bool soundOn);
        void SaveMusicVolume(float value);
        void SaveSoundVolume(float value);
        void SaveLanguage(Language language);
        void SaveVerticalAimValue(float value);
        void SaveHorizontalAimValue(float value);
        void ClearProgressData();

        ProgressData LoadProgressData();
        SettingsData LoadSettingsData();
    }
}