using System.Collections;
using CodeBase.Data;
using CodeBase.Data.Stats;
using CodeBase.Infrastructure.States;
using CodeBase.Logic;
using CodeBase.Services;
using CodeBase.Services.Ads;
using CodeBase.Services.Input;
using CodeBase.Services.LeaderBoard;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.SaveLoad;
using CodeBase.Services.StaticData;
using CodeBase.UI.Services.Windows;
using NTC.Global.Cache;
using NTC.Global.System;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;

namespace CodeBase.UI.Windows.Common
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class WindowBase : MonoCache, IProgressReader
    {
        protected IWindowService WindowService;
        protected ISaveLoadService SaveLoadService;
        protected IGameStateMachine GameStateMachine;
        protected IStaticDataService StaticDataService;
        protected IAdsService AdsService;
        protected ILeaderboardService LeaderBoardService;
        protected AudioSource AudioSource;
        protected GameObject Hero;
        protected PlayerProgress Progress;
        protected float Volume;
        private WindowId _windowId;
        protected LevelStats LevelStats;
        protected Scene CurrentLevel;

        protected override void OnEnabled()
        {
            if (WindowService == null)
                WindowService = AllServices.Container.Single<IWindowService>();

            if (SaveLoadService == null)
                SaveLoadService = AllServices.Container.Single<ISaveLoadService>();

            if (GameStateMachine == null)
                GameStateMachine = AllServices.Container.Single<IGameStateMachine>();

            if (StaticDataService == null)
                StaticDataService = AllServices.Container.Single<IStaticDataService>();

            if (AdsService == null)
                AdsService = AllServices.Container.Single<IAdsService>();

            if (LeaderBoardService == null)
                LeaderBoardService = AllServices.Container.Single<ILeaderboardService>();

            if (AudioSource == null)
                AudioSource = GetComponent<AudioSource>();
        }

        protected void Construct(GameObject hero, WindowId windowId)
        {
            WindowService = AllServices.Container.Single<IWindowService>();
            SaveLoadService = AllServices.Container.Single<ISaveLoadService>();
            GameStateMachine = AllServices.Container.Single<IGameStateMachine>();
            StaticDataService = AllServices.Container.Single<IStaticDataService>();
            AdsService = AllServices.Container.Single<IAdsService>();
            LeaderBoardService = AllServices.Container.Single<ILeaderboardService>();
            AudioSource = GetComponent<AudioSource>();
            Hero = hero;
            _windowId = windowId;
            Hide();
        }

        protected void Hide()
        {
            gameObject.Disable();
            PlayCloseSound();

            if (!WindowService.IsAnotherActive(_windowId))
            {
                if (AllServices.Container.Single<IInputService>() is DesktopInputService)
                    Cursor.lockState = CursorLockMode.Locked;

                Hero.ResumeHero();
                Time.timeScale = Constants.TimeScaleResume;
            }
        }

        public void Show(bool showCursor = true)
        {
            gameObject.Enable();
            Hero.StopHero();
            Time.timeScale = Constants.TimeScaleStop;

            if (AllServices.Container.Single<IInputService>() is DesktopInputService)
                ShowCursor(showCursor);

            PlayOpenSound();
        }

        private void ShowCursor(bool showCursor) =>
            Cursor.lockState = showCursor ? CursorLockMode.Confined : CursorLockMode.Locked;

        protected virtual void PlayCloseSound()
        {
            SoundInstance.InstantiateOnTransform(
                audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.MenuClose), transform: transform,
                Volume, AudioSource);
        }

        protected virtual void PlayOpenSound()
        {
            SoundInstance.InstantiateOnTransform(
                audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.MenuOpen), transform: transform,
                Volume, AudioSource);
        }

        public void LoadProgress(PlayerProgress progress)
        {
            Progress = progress;
            Progress.SettingsData.SoundSwitchChanged += SwitchChanged;
            Progress.SettingsData.SoundVolumeChanged += VolumeChanged;
            VolumeChanged();
            SwitchChanged();
            LevelStats = Progress.AllStats.CurrentLevelStats;
        }

        private void VolumeChanged() =>
            Volume = Progress.SettingsData.SoundVolume;

        private void SwitchChanged() =>
            Volume = Progress.SettingsData.SoundOn ? Progress.SettingsData.SoundVolume : Constants.Zero;

        protected void RestartLevel()
        {
            WindowService.HideAll();
            Progress.AllStats.RestartedLevel();
            SoundInstance.StopRandomMusic();
            AllServices.Container.Single<IGameStateMachine>().Enter<LoadPlayerProgressState, bool>(Progress.IsHardMode);
        }

        protected void InitializeAdsSDK()
        {
            if (AdsService.IsInitialized())
                AdsServiceInitializedSuccess();
            else
                StartCoroutine(AdsService.Initialize());
        }

        protected virtual void AdsServiceInitializedSuccess() =>
            AdsService.OnInitializeSuccess -= AdsServiceInitializedSuccess;

        protected void InitializeLeaderBoard()
        {
            if (LeaderBoardService.IsInitialized())
                RequestLeaderBoard();
            else
                StartCoroutine(CoroutineInitializeLeaderBoard());
        }

        protected virtual void RequestLeaderBoard() =>
            LeaderBoardService.OnInitializeSuccess -= RequestLeaderBoard;

        private IEnumerator CoroutineInitializeLeaderBoard()
        {
            yield return LeaderBoardService.Initialize();
        }

        protected void ShowSetValueError(string error)
        {
            Debug.Log($"ShowSetValueError {error}");
            LeaderBoardService.OnSetValueError -= ShowSetValueError;
        }

        protected void AddLevelResult()
        {
            LeaderBoardService.OnSetValueError += ShowSetValueError;
            SubscribeSetValueSuccess();
            LeaderBoardService.SetValue(CurrentLevel.GetLeaderBoardName(Progress.IsHardMode),
                LevelStats.Score);
        }

        protected void SuccessSetValue() =>
            LeaderBoardService.OnSetValueSuccess -= SuccessSetValue;

        protected virtual void SubscribeSetValueSuccess() =>
            LeaderBoardService.OnSetValueSuccess += SuccessSetValue;
    }
}