﻿using System.Collections;
using CodeBase.Data;
using CodeBase.Data.Stats;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using CodeBase.UI.Windows.GameEnd;
using CodeBase.UI.Windows.Gifts;
using CodeBase.UI.Windows.LeaderBoard;
using Tayx.Graphy.Utils.NumString;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Results
{
    public class ResultsWindow : WindowBase
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _toNextWindowButton;
        [SerializeField] private StarsPanel _starsPanel;
        [SerializeField] private TextMeshProUGUI _playTimeCount;
        [SerializeField] private TextMeshProUGUI _killed;
        [SerializeField] private TextMeshProUGUI _totalEnemies;
        [SerializeField] private TextMeshProUGUI _restartsCount;
        [SerializeField] private TextMeshProUGUI _score;

        private LevelStats _levelStats;
        private Scene _nextScene;
        private int _maxPrice;

        private void OnEnable()
        {
            PrepareLevelStats();
            _restartButton.onClick.AddListener(RestartLevel);

            if (Application.isEditor || LeaderBoardService == null)
                return;

            LeaderBoardService.OnInitializeSuccess += AddNewResult;
            InitializeLeaderboard();
        }

        private void OnDisable()
        {
            _restartButton.onClick.RemoveListener(RestartLevel);

            if (LeaderBoardService != null)
                LeaderBoardService.OnInitializeSuccess -= AddNewResult;
        }

        public void Construct(GameObject hero) =>
            base.Construct(hero, WindowId.Result);

        public void AddData(Scene nextLevel, int maxPrice)
        {
            _nextScene = nextLevel;
            _maxPrice = maxPrice;

            if (_nextScene == Scene.Initial)
                _toNextWindowButton.onClick.AddListener(ToGameEndWindow);
            else
                _toNextWindowButton.onClick.AddListener(ToGiftsWindow);
        }

        private void ToLeaderBoardWindow() =>
            WindowService.Show<LeaderBoardWindow>(WindowId.LeaderBoard);

        private void PrepareLevelStats()
        {
            if (Progress == null)
                return;

            _levelStats = Progress.Stats.CurrentLevelStats;
            _levelStats.CalculateScore();
        }

        private void InitializeLeaderboard()
        {
            Debug.Log("InitializeLeaderBoard");
            if (LeaderBoardService.IsInitialized())
                AddNewResult();
            else
                StartCoroutine(CoroutineInitializeLeaderBoard());
        }

        private IEnumerator CoroutineInitializeLeaderBoard()
        {
            Debug.Log("CoroutineInitializeLeaderBoard");
            yield return LeaderBoardService.Initialize();
        }

        private void AddNewResult()
        {
            Debug.Log("AddNewResult");
            Debug.Log($"SetValue {_levelStats.Score}");
            LeaderBoardService.SetValue(_nextScene.GetLeaderBoardName(), _levelStats.Score);
        }

        public void ShowData()
        {
            _starsPanel.ShowStars(_levelStats.StarsCount);
            _playTimeCount.text = $"{_levelStats.PlayTimeData.PlayTime.ToInt()}";
            _killed.text = $"{_levelStats.KillsData.KilledEnemies}";
            _totalEnemies.text = $"{_levelStats.KillsData.TotalEnemies}";
            _restartsCount.text = $"{_levelStats.RestartsData.Count}";
            _score.text = $"{_levelStats.Score}";
        }

        private void ToGameEndWindow() =>
            WindowService.Show<GameEndWindow>(WindowId.GameEnd);

        private void ToGiftsWindow()
        {
            WindowBase giftsWindow = WindowService.Show<GiftsWindow>(WindowId.Gifts);
            (giftsWindow as GiftsWindow).AddData(_nextScene);
            GiftsGenerator giftsGenerator = (giftsWindow as GiftsWindow)?.gameObject.GetComponent<GiftsGenerator>();
            WindowService.HideOthers(WindowId.Gifts);
            giftsGenerator?.SetMaxPrice(_maxPrice);
            giftsGenerator?.Generate();
        }
    }
}