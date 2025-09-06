using CodeBase.Data.Progress;
using CodeBase.Data.Settings;

namespace CodeBase.Services.PersistentProgress
{
    public class PlayerProgressService : IPlayerProgressService
    {
        public ProgressData ProgressData { get; private set; }
        public SettingsData SettingsData { get; private set; }
        public string CurrentError { get; set; }
        public bool FirstLaunch { get; set; } = true;

        public void ClearProgressData() =>
            ProgressData = null;

        public void SetProgressData(ProgressData progressData) =>
            ProgressData = progressData;

        public void SetSettingsData(SettingsData settingsData) =>
            SettingsData = settingsData;
    }
}