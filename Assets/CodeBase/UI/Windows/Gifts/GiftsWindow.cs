using CodeBase.Data.Progress;
using CodeBase.Infrastructure.States;
using CodeBase.StaticData.Levels;
using CodeBase.UI.Elements.Hud;
using CodeBase.UI.Elements.Hud.MobileInputPanel;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Gifts
{
    public class GiftsWindow : WindowBase
    {
        [SerializeField] private Button _addCoinsButton;
        [SerializeField] private Button _toNextLevelButton;
        [SerializeField] private ItemsGeneratorBase _generator;
        [SerializeField] private int _coinsCount;

        private SceneId _nextScene;

        public void Construct(GameObject hero, OpenSettings openSettings, MobileInput mobileInput) =>
            base.Construct(hero, WindowId.Gifts, openSettings, mobileInput);

        private void ToNextLevel()
        {
            LevelStaticData levelStaticData = _staticDataService.ForLevel(_nextScene);
            ProgressData.WorldData.LevelNameData.ChangeLevel(_nextScene.ToString());

            if (ProgressData.IsAsianMode)
                ProgressData.AllStats.StartNewLevel(_nextScene, levelStaticData.TargetPlayTime,
                    levelStaticData.MaxStarsScoreAsian, levelStaticData.EnemySpawners.Count);
            else
                ProgressData.AllStats.StartNewLevel(_nextScene, levelStaticData.TargetPlayTime,
                    levelStaticData.MaxStarsScoreStandard, levelStaticData.EnemySpawners.Count);

            ProgressData.WorldData.ShowAdOnLevelStart = true;
            _saveLoadService.SaveProgressData();
            _saveLoadService.SaveSettingsData();
            _windowService.ClearAll();
            Close();
            _gameStateMachine.Enter<LoadSceneState, SceneId>(_nextScene);
        }

        private void OnEnable()
        {
            // _addCoinsButton.enabled = Application.isEditor;
            _addCoinsButton.gameObject.SetActive(Application.isEditor);
            _addCoinsButton.onClick.AddListener(ShowAds);
            Cursor.lockState = CursorLockMode.Confined;
            GenerateItems();

            if (Application.isEditor)
                return;

            if (_adsService == null)
                return;

            _adsService.OnInitializeSuccess += AdsServiceInitializedSuccess;
            _adsService.OnShowVideoAdError += ShowError;
            _adsService.OnClosedVideoAd += ShowClosed;
            _adsService.OnRewardedAd += AddCoinsAfterAds;
            InitializeAdsSDK();
        }

        private void OnDisable()
        {
            _addCoinsButton.onClick.RemoveListener(ShowAds);

            if (_adsService == null)
                return;

            _adsService.OnInitializeSuccess -= AdsServiceInitializedSuccess;
            _adsService.OnShowVideoAdError -= ShowError;
            _adsService.OnClosedVideoAd -= ShowClosed;
            _adsService.OnRewardedAd -= AddCoinsAfterAds;
        }

        public void AddData(SceneId nextLevel)
        {
            _nextScene = nextLevel;
            _toNextLevelButton.onClick.AddListener(ToNextLevel);
        }

        private void GenerateItems() =>
            _generator.Generate();

        protected override void AdsServiceInitializedSuccess()
        {
            base.AdsServiceInitializedSuccess();
            _addCoinsButton.gameObject.SetActive(true);
            // _addCoinsButton.enabled = true;
        }

        private void ShowClosed()
        {
            SoundInstance.StartRandomMusic();
            _adsService.OnClosedVideoAd -= ShowClosed;
        }

        private void ShowError(string message)
        {
            SoundInstance.StartRandomMusic();
            Debug.Log($"OnErrorFullScreenAd: {message}");
            _adsService.OnShowVideoAdError -= ShowError;
        }

        private void Close()
        {
            base.Close();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void ShowAds()
        {
            SoundInstance.PauseMusic();

            if (Application.isEditor)
            {
                _addCoinsButton.gameObject.SetActive(false);
                AddCoins();
                SoundInstance.ResumeMusic();
                return;
            }

            _adsService.ShowVideoAd();
        }

        private void AddCoinsAfterAds()
        {
            SoundInstance.StartRandomMusic();
            AddCoins();
            _adsService.OnRewardedAd -= AddCoinsAfterAds;
        }

        private void AddCoins()
        {
            ProgressData.AllStats.AddMoney(_coinsCount);
            _addCoinsButton.gameObject.SetActive(false);
            // _addCoinsButton.enabled = false;
        }
    }
}