﻿using CodeBase.Data;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using CodeBase.UI.Windows.GameEnd;
using CodeBase.UI.Windows.Gifts;
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

        private Scene _nextScene;
        private int _maxPrice;

        private void OnEnable()
        {
            _restartButton.onClick.AddListener(RestartLevel);

            if (Application.isEditor || LeaderBoardService == null || Progress == null)
                return;

            PrepareLevelStats();
            LeaderBoardService.OnInitializeSuccess += RequestLeaderBoard;
            InitializeLeaderBoard();
        }

        private void OnDisable()
        {
            _restartButton.onClick.RemoveListener(RestartLevel);

            if (LeaderBoardService != null)
                LeaderBoardService.OnInitializeSuccess -= RequestLeaderBoard;
        }

        public void Construct(GameObject hero) =>
            base.Construct(hero, WindowId.Result);

        public void AddData(Scene currentLevel, Scene nextLevel, int maxPrice)
        {
            CurrentLevel = currentLevel;
            _nextScene = nextLevel;
            _maxPrice = maxPrice;

            if (_nextScene == Scene.Initial)
                _toNextWindowButton.onClick.AddListener(ToGameEndWindow);
            else
                _toNextWindowButton.onClick.AddListener(ToGiftsWindow);
        }

        public void ShowData()
        {
            LevelStats.CalculateScore();
            Debug.Log($"ShowData LevelStats.StarsCount {LevelStats.StarsCount}");
            _starsPanel.ShowStars(LevelStats.StarsCount);
            _playTimeCount.text = $"{LevelStats.PlayTimeData.PlayTime.ToInt()}";
            _killed.text = $"{LevelStats.KillsData.KilledEnemies}";
            _totalEnemies.text = $"{LevelStats.KillsData.TotalEnemies}";
            _restartsCount.text = $"{LevelStats.RestartsData.Count}";
            Debug.Log($"ShowData {LevelStats.Scene} {LevelStats.Score}");
            _score.text = $"{LevelStats.Score}";
        }

        private void ToGameEndWindow() =>
            WindowService.Show<GameEndWindow>(WindowId.GameEnd);

        private void ToGiftsWindow()
        {
            WindowBase giftsWindow = WindowService.Show<GiftsWindow>(WindowId.Gifts);
            (giftsWindow as GiftsWindow).AddData(_nextScene);
            GiftsGenerator giftsGenerator = (giftsWindow as GiftsWindow)?.gameObject.GetComponent<GiftsGenerator>();
            giftsGenerator?.SetMaxPrice(_maxPrice);
            giftsGenerator?.Generate();
        }

        protected override void RequestLeaderBoard()
        {
            base.RequestLeaderBoard();
            AddLevelResult();
        }
    }
}