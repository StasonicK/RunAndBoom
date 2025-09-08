using CodeBase.Data;
using CodeBase.Data.Progress;
using CodeBase.Infrastructure.States;
using CodeBase.Services;
using CodeBase.UI.Elements.Hud;
using CodeBase.UI.Elements.Hud.MobileInputPanel;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.GameFinished
{
    public class GameFinishedWindow : WindowBase
    {
        [SerializeField] private Button _startNewStandardGameButton;
        [SerializeField] private Button _startNewAsianGameButton;

        private void OnEnable()
        {
            _startNewStandardGameButton.onClick.AddListener(StartNewStandardDifficultyGame);
            _startNewAsianGameButton.onClick.AddListener(StartNewAsianDifficultyGame);

            if (Application.isEditor || _leaderBoardService == null || ProgressData == null)
                return;

            ProgressData.AllStats.SaveCurrentLevelStats();
            _leaderBoardService.OnInitializeSuccess += RequestLeaderBoard;
            InitializeLeaderBoard();
        }

        private void OnDisable()
        {
            _startNewStandardGameButton.onClick.RemoveListener(StartNewStandardDifficultyGame);
            _startNewAsianGameButton.onClick.RemoveListener(StartNewAsianDifficultyGame);
            _leaderBoardService.OnInitializeSuccess -= RequestLeaderBoard;
        }

        public void Construct(GameObject hero, OpenSettings openSettings, MobileInput mobileInput) =>
            base.Construct(hero, WindowId.GameEnd, openSettings, mobileInput);

        protected override void RequestLeaderBoard()
        {
            base.RequestLeaderBoard();
            AddLevelResult();
        }

        protected override void SubscribeSetValueSuccess() =>
            _leaderBoardService.OnSetValueSuccess += AddGameResult;

        private void StartNewStandardDifficultyGame()
        {
            PrepareToStartNewGame();
            AllServices.Container.Single<IGameStateMachine>().Enter<LoadGameDataState, bool>(false);
        }

        private void StartNewAsianDifficultyGame()
        {
            PrepareToStartNewGame();
            AllServices.Container.Single<IGameStateMachine>().Enter<LoadGameDataState, bool>(true);
        }

        private void PrepareToStartNewGame()
        {
            SoundInstance.StopRandomMusic();
            _windowService.ClearAll();
            _saveLoadService.ClearProgressData();
            _playerProgressService.FirstLaunch = false;
        }

        private void AddGameResult()
        {
            int allLevelsScore = ProgressData.AllStats.GetAllLevelsStats();
            Debug.Log($"AddGameResult {allLevelsScore}");
            _leaderBoardService.OnSetValueError += ShowSetValueError;
            _leaderBoardService.OnSetValueSuccess += SuccessSetValue;
            _leaderBoardService.SetValue(SceneId.Initial.GetLeaderBoardName(ProgressData.IsAsianMode),
                allLevelsScore);
        }
    }
}