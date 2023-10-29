﻿using CodeBase.Data.Progress;
using CodeBase.Data.Settings;

namespace CodeBase.Services.PersistentProgress
{
    public interface IPlayerProgressService : IService
    {
        public ProgressData ProgressData { get; }
        public SettingsData SettingsData { get; }
        void ClearProgressData();
        void SetProgressData(ProgressData progressData);
        void SetSettingsData(SettingsData settingsData);
    }
}