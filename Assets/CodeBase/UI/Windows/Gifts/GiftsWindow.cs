﻿using CodeBase.Data.Progress;
using CodeBase.Infrastructure.States;
using CodeBase.StaticData.Levels;
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

        private void OnEnable()
        {
            _addCoinsButton.enabled = Application.isEditor;
            _addCoinsButton.onClick.AddListener(ShowAds);
            Cursor.lockState = CursorLockMode.Confined;
            GenerateItems();

            if (Application.isEditor)
                return;

            if (AdsService == null)
                return;

            AdsService.OnInitializeSuccess += AdsServiceInitializedSuccess;
            AdsService.OnShowVideoAdError += ShowError;
            AdsService.OnClosedVideoAd += ShowClosed;
            AdsService.OnRewardedAd += AddCoinsAfterAds;
            InitializeAdsSDK();
        }

        private void OnDisable()
        {
            _addCoinsButton.onClick.RemoveListener(ShowAds);

            if (AdsService == null)
                return;

            AdsService.OnInitializeSuccess -= AdsServiceInitializedSuccess;
            AdsService.OnShowVideoAdError -= ShowError;
            AdsService.OnClosedVideoAd -= ShowClosed;
            AdsService.OnRewardedAd -= AddCoinsAfterAds;
        }

        public void Construct(GameObject hero) =>
            base.Construct(hero, WindowId.Gifts);

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
            _addCoinsButton.enabled = true;
        }

        private void ShowClosed()
        {
            Debug.Log("OnClosedVideoAd");
            AdsService.OnClosedVideoAd -= ShowClosed;
            SoundInstance.StartRandomMusic();
        }

        private void ShowError(string message)
        {
            Debug.Log($"OnErrorFullScreenAd: {message}");
            AdsService.OnShowVideoAdError -= ShowError;
            SoundInstance.StartRandomMusic();
        }

        private void ToNextLevel()
        {
            Debug.Log("ToNextLevel");
            LevelStaticData levelStaticData = StaticDataService.ForLevel(_nextScene);
            ProgressData.WorldData.LevelNameData.ChangeLevel(_nextScene.ToString());
            ProgressData.AllStats.StartNewLevel(_nextScene, levelStaticData.TargetPlayTime,
                levelStaticData.EnemySpawners.Count);
            ProgressData.WorldData.ShowAdOnLevelStart = true;
            SaveLoadService.SaveProgressData();
            WindowService.ClearAll();
            Close();
            GameStateMachine.Enter<LoadSceneState, SceneId>(_nextScene);
            Debug.Log($"{_nextScene}");
        }

        private void Close()
        {
            Hide();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void ShowAds()
        {
            if (Application.isEditor)
            {
                AddCoins();
                return;
            }

            SoundInstance.StopRandomMusic();
            AdsService.ShowVideoAd();
        }

        private void AddCoinsAfterAds()
        {
            AddCoins();
            AdsService.OnRewardedAd -= AddCoinsAfterAds;
        }

        private void AddCoins()
        {
            Debug.Log("AddCoins");
            ProgressData.AllStats.AddMoney(_coinsCount);
            _addCoinsButton.enabled = false;
        }

        protected override void PlayOpenSound()
        {
            SoundInstance.InstantiateOnTransform(
                audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.VictoryMusic), transform: transform,
                Volume, AudioSource);
            SoundInstance.StopRandomMusic(false);
        }
    }
}