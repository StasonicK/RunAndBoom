﻿using System.Collections;
using CodeBase.Data;
using CodeBase.Data.Stats;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using CodeBase.UI.Windows.Leaderboard;
using Tayx.Graphy.Utils.NumString;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Results
{
    public class ResultsWindow : WindowBase
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _toLeaderBoardWindowButton;
        [SerializeField] private StarsPanel _starsPanel;
        [SerializeField] private TextMeshProUGUI _playTimeCount;
        [SerializeField] private TextMeshProUGUI _killed;
        [SerializeField] private TextMeshProUGUI _totalEnemies;
        [SerializeField] private TextMeshProUGUI _restartsCount;
        [SerializeField] private TextMeshProUGUI _score;

        private LevelStats _levelStats;
        private Scene _nextScene;
        private int _maxPrice;

        private void Awake()
        {
            PrepareLevelStats();
            LeaderboardService.OnInitializeSuccess += AddNewResult;
            StartCoroutine(InitializeLeaderboardSDK());
        }

        private void OnEnable()
        {
            _restartButton.onClick.AddListener(RestartLevel);
            _toLeaderBoardWindowButton.onClick.AddListener(ToLeaderBoardWindow);
        }

        private void OnDisable()
        {
            _restartButton.onClick.RemoveListener(RestartLevel);
            _toLeaderBoardWindowButton.onClick.RemoveListener(ToLeaderBoardWindow);
        }

        public void Construct(GameObject hero) =>
            base.Construct(hero, WindowId.Result);

        public void AddData(Scene nextScene, int maxPrice)
        {
            _maxPrice = maxPrice;
            _nextScene = nextScene;
        }

        private void ToLeaderBoardWindow()
        {
            Hide();
            LeaderboardWindow leaderboardWindow =
                WindowService.Show<LeaderboardWindow>(WindowId.LeaderBoard) as LeaderboardWindow;
            leaderboardWindow?.AddData(_nextScene, _maxPrice);
        }

        private void PrepareLevelStats()
        {
            _levelStats = Progress.Stats.CurrentLevelStats;
            _levelStats.CalculateScore();
        }

        private IEnumerator InitializeLeaderboardSDK()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            yield break;
#endif
            yield return LeaderboardService.Initialize();
        }

        private void AddNewResult() =>
            LeaderboardService.SetValue(_nextScene.GetLeaderBoardName(), _levelStats.Score);

        public void ShowData()
        {
            _starsPanel.ShowStars(_levelStats.StarsCount);
            _playTimeCount.text = $"{_levelStats.PlayTimeData.PlayTime.ToInt()}";
            _killed.text = $"{_levelStats.KillsData.KilledEnemies}";
            _totalEnemies.text = $"{_levelStats.KillsData.TotalEnemies}";
            _restartsCount.text = $"{_levelStats.RestartsData.Count}";
            _score.text = $"{_levelStats.Score}";
        }
    }
}