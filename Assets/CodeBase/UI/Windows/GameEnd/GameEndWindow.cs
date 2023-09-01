using CodeBase.Data;
using CodeBase.Infrastructure.States;
using CodeBase.Services;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.GameEnd
{
    public class GameEndWindow : WindowBase
    {
        [SerializeField] private Button _startNewStandardGameButton;
        [SerializeField] private Button _startNewHardGameButton;

        protected override void OnEnabled()
        {
            _startNewStandardGameButton.onClick.AddListener(StartNewCommonGame);
            _startNewHardGameButton.onClick.AddListener(StartNewHardModeGame);

            if (Application.isEditor || LeaderBoardService == null || Progress == null)
                return;

            Progress.AllStats.SaveCurrentLevelStats();
            LeaderBoardService.OnInitializeSuccess += RequestLeaderBoard;
            InitializeLeaderBoard();
        }

        protected override void OnDisabled()
        {
            _startNewStandardGameButton.onClick.RemoveListener(StartNewCommonGame);
            _startNewHardGameButton.onClick.RemoveListener(StartNewHardModeGame);
            LeaderBoardService.OnInitializeSuccess -= RequestLeaderBoard;
        }

        public void Construct(GameObject hero) =>
            base.Construct(hero, WindowId.GameEnd);

        protected override void RequestLeaderBoard()
        {
            base.RequestLeaderBoard();
            AddLevelResult();
        }

        protected override void SubscribeSetValueSuccess() =>
            LeaderBoardService.OnSetValueSuccess += AddGameResult;

        private void StartNewCommonGame()
        {
            PrepareToStartNewGame();
            AllServices.Container.Single<IGameStateMachine>().Enter<LoadPlayerProgressState, bool>(false);
        }

        private void StartNewHardModeGame()
        {
            PrepareToStartNewGame();
            AllServices.Container.Single<IGameStateMachine>().Enter<LoadPlayerProgressState, bool>(true);
        }

        private void PrepareToStartNewGame()
        {
            SoundInstance.StopRandomMusic();
            WindowService.HideAll();
            SaveLoadService.ClearProgress();
        }

        private void AddGameResult()
        {
            LeaderBoardService.OnSetValueError += ShowSetValueError;
            LeaderBoardService.OnSetValueSuccess += SuccessSetValue;
            LeaderBoardService.SetValue(Scene.Initial.GetLeaderBoardName(Progress.IsHardMode),
                Progress.AllStats.GetAllLevelsStats());
        }
    }
}