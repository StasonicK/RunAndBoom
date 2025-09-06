using System.Collections;
using CodeBase.Data;
using CodeBase.Data.Progress;
using CodeBase.Data.Progress.Stats;
using CodeBase.Infrastructure.States;
using CodeBase.Logic;
using CodeBase.Services;
using CodeBase.Services.Ads;
using CodeBase.Services.Audio;
using CodeBase.Services.Input;
using CodeBase.Services.LeaderBoard;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.SaveLoad;
using CodeBase.Services.StaticData;
using CodeBase.UI.Elements.Hud;
using CodeBase.UI.Elements.Hud.MobileInputPanel;
using CodeBase.UI.Services.Windows;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;

namespace CodeBase.UI.Windows.Common
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class WindowBase : MonoBehaviour, IProgressReader
    {
        protected IWindowService _windowService;
        protected ISaveLoadService _saveLoadService;
        protected IPlayerProgressService _playerProgressService;
        protected IGameStateMachine _gameStateMachine;
        protected IStaticDataService _staticDataService;
        protected IAdsService _adsService;
        protected ILeaderboardService _leaderBoardService;
        protected IInputService _inputService;
        protected IAudioService _audioService;
        protected AudioSource _audioSource;
        protected GameObject _hero;
        protected float Volume;
        protected ProgressData ProgressData;
        private WindowId _windowId;
        protected LevelStats LevelStats;
        protected SceneId CurrentLevel;
        private OpenSettings _openSettings;
        private bool _isInitial;
        private MobileInput _mobileInput;
        protected PlayerInput PlayerInput;

        private void Awake() =>
            PlayerInput = new PlayerInput();

        protected void Construct(GameObject hero, WindowId windowId, OpenSettings openSettings, MobileInput mobileInput)
        {
            _inputService = AllServices.Container.Single<IInputService>();
            _windowService = AllServices.Container.Single<IWindowService>();
            _saveLoadService = AllServices.Container.Single<ISaveLoadService>();
            _gameStateMachine = AllServices.Container.Single<IGameStateMachine>();
            _staticDataService = AllServices.Container.Single<IStaticDataService>();
            _playerProgressService = AllServices.Container.Single<IPlayerProgressService>();
            _adsService = AllServices.Container.Single<IAdsService>();
            _leaderBoardService = AllServices.Container.Single<ILeaderboardService>();
            _audioService = AllServices.Container.Single<IAudioService>();
            _audioSource = GetComponent<AudioSource>();
            _hero = hero;
            _windowId = windowId;
            _openSettings = openSettings;
            _mobileInput = mobileInput;
            _isInitial = true;
        }

        protected void Hide()
        {
            gameObject.SetActive(false);
            PlayCloseSound();

            if (!_windowService.IsAnotherActive(_windowId))
            {
                if (AllServices.Container.Single<IInputService>() is DesktopInputService)
                    Cursor.lockState = CursorLockMode.Locked;

                _hero.ResumeHero();
                Time.timeScale = Constants.TimeScaleResume;
                _openSettings.On();

                if (_mobileInput != null)
                    _mobileInput.On();
            }
        }

        public void Show(bool showCursor = true)
        {
            gameObject.SetActive(true);
            _hero.StopHero();
            Time.timeScale = Constants.TimeScaleStop;

            if (AllServices.Container.Single<IInputService>() is DesktopInputService)
                ShowCursor(showCursor);

            _openSettings.Off();

            if (_mobileInput != null)
                _mobileInput.Off();

            if (!_isInitial)
                PlayOpenSound();
            else
                _isInitial = false;
        }

        private void ShowCursor(bool showCursor) =>
            Cursor.lockState = showCursor ? CursorLockMode.Confined : CursorLockMode.Locked;

        private void PlayCloseSound() =>
            _audioService.LaunchUIActionSound(UIActionSoundId.MenuClose, _hero.transform, _audioSource);

        private void PlayOpenSound() =>
            _audioService.LaunchUIActionSound(UIActionSoundId.MenuOpen, _hero.transform, _audioSource);

        public void LoadProgressData(ProgressData progressData)
        {
            ProgressData = progressData;
            LevelStats = ProgressData.AllStats.CurrentLevelStats;
        }

        protected void RestartLevel()
        {
            _windowService.ClearAll();
            ProgressData.AllStats.RestartedLevel();
            SoundInstance.StopRandomMusic();
            AllServices.Container.Single<IGameStateMachine>()
                .Enter<LoadGameDataState, bool>(ProgressData.IsAsianMode);
        }

        protected void InitializeAdsSDK()
        {
            if (_adsService.IsInitialized())
                AdsServiceInitializedSuccess();
            else
                StartCoroutine(_adsService.Initialize());
        }

        protected virtual void AdsServiceInitializedSuccess() =>
            _adsService.OnInitializeSuccess -= AdsServiceInitializedSuccess;

        protected void InitializeLeaderBoard()
        {
            if (_leaderBoardService.IsInitialized())
                RequestLeaderBoard();
            else
                StartCoroutine(CoroutineInitializeLeaderBoard());
        }

        protected virtual void RequestLeaderBoard() =>
            _leaderBoardService.OnInitializeSuccess -= RequestLeaderBoard;

        private IEnumerator CoroutineInitializeLeaderBoard()
        {
            yield return _leaderBoardService.Initialize();
        }

        protected void ShowSetValueError(string error)
        {
            Debug.Log($"ShowSetValueError {error}");
            _leaderBoardService.OnSetValueError -= ShowSetValueError;
        }

        protected void AddLevelResult()
        {
            Debug.Log($"AddLevelResult {LevelStats.SceneId} {LevelStats.Score}");
            _leaderBoardService.OnSetValueError += ShowSetValueError;
            SubscribeSetValueSuccess();
            _leaderBoardService.SetValue(CurrentLevel.GetLeaderBoardName(ProgressData.IsAsianMode),
                LevelStats.Score);
        }

        protected void SuccessSetValue() =>
            _leaderBoardService.OnSetValueSuccess -= SuccessSetValue;

        protected virtual void SubscribeSetValueSuccess() =>
            _leaderBoardService.OnSetValueSuccess += SuccessSetValue;
    }
}