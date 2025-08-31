using System;
using CodeBase.Data;
using CodeBase.Data.Progress;
using CodeBase.Hero;
using CodeBase.Infrastructure.Factories;
using CodeBase.Logic;
using CodeBase.Logic.Level;
using CodeBase.Services.Ads;
using CodeBase.Services.Input;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.StaticData;
using CodeBase.StaticData;
using CodeBase.StaticData.Levels;
using CodeBase.UI.Elements.Hud;
using CodeBase.UI.Elements.Hud.MobileInputPanel;
using CodeBase.UI.Elements.Hud.MobileInputPanel.Joysticks;
using CodeBase.UI.Elements.Hud.WeaponsPanel;
using CodeBase.UI.Services.Factory;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Authorization;
using CodeBase.UI.Windows.Death;
using CodeBase.UI.Windows.GameFinished;
using CodeBase.UI.Windows.Gifts;
using CodeBase.UI.Windows.LeaderBoard;
using CodeBase.UI.Windows.Results;
using CodeBase.UI.Windows.Settings;
using CodeBase.UI.Windows.Shop;
using CodeBase.UI.Windows.Start;
using CodeBase.UI.Windows.StartNewGame;
using Cysharp.Threading.Tasks;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Infrastructure.States
{
    public class LoadSceneState : IPayloadedState<SceneId>
    {
        private const string LevelName = "Level_";
        private const string FinishPointTag = "FinishPoint";

        private readonly IGameStateMachine _stateMachine;
        private readonly ISceneLoader _sceneLoader;
        private readonly ILoadingCurtain _loadingCurtain;
        private readonly IGameFactory _gameFactory;
        private readonly IEnemyFactory _enemyFactory;
        private readonly IPlayerProgressService _progressService;
        private readonly IStaticDataService _staticDataService;
        private readonly IUIFactory _uiFactory;
        private readonly IWindowService _windowService;
        private readonly IInputService _inputService;
        private SceneId _sceneId;
        private bool _isInitial = true;
        private GameObject _hud;
        private IAdListener _adListener;
        private IAdsService _adsService;
        private GameObject _hero;
        private OpenSettings _openSettings;
        private MobileInput _mobileInput;

        public LoadSceneState(IGameStateMachine gameStateMachine, ISceneLoader sceneLoader,
            ILoadingCurtain loadingCurtain, IGameFactory gameFactory, IEnemyFactory enemyFactory,
            IPlayerProgressService progressService, IStaticDataService staticDataService, IUIFactory uiFactory,
            IWindowService windowService, IInputService inputService, IAdsService adsService, IAdListener adListener)
        {
            _stateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _gameFactory = gameFactory;
            _enemyFactory = enemyFactory;
            _progressService = progressService;
            _staticDataService = staticDataService;
            _uiFactory = uiFactory;
            _windowService = windowService;
            _inputService = inputService;
            _adsService = adsService;
            _adListener = adListener;

            _loadingCurtain.FadedOut += TryPauseGame;
        }

        private void TryPauseGame()
        {
            if (_progressService.ProgressData.WorldData.ShowAdOnLevelStart)
            {
                if (Application.isEditor)
                    return;

                Time.timeScale = Constants.TimeScaleStop;
                SoundInstance.StopRandomMusic(false);
                _adsService.ShowInterstitialAd();
            }
        }

        public void Enter(SceneId sceneId)
        {
            _sceneId = sceneId;

            if (_sceneId.ToString().Contains(LevelName))
            {
                _loadingCurtain.Show();
                _gameFactory.CleanUp();
                _gameFactory.WarmUp();
            }

            _sceneLoader.Load(_sceneId, OnLoaded);
        }

        public void Exit()
        {
            _loadingCurtain.Hide();
        }

        private async void OnLoaded(SceneId sceneId)
        {
            await InitUIRoot();

            switch (sceneId)
            {
                case SceneId.Level_1:
                    await InitializeGameWorld();
                    break;
                case SceneId.Level_2:
                    await InitializeGameWorld();
                    break;
                case SceneId.Level_3:
                    await InitializeGameWorld();
                    break;
                case SceneId.Level_4:
                    await InitializeGameWorld();
                    break;
                case SceneId.Level_5:
                    await InitializeGameWorld();
                    break;
            }

            InformProgressReaders();
            _stateMachine.Enter<GameLoopState>();
        }

        private void InformProgressReaders()
        {
            foreach (IProgressReader progressReader in _gameFactory.ProgressReaders)
                progressReader.LoadProgressData(_progressService.ProgressData);
        }

        private async UniTask InitializeGameWorld()
        {
            LevelStaticData levelData = LevelStaticData();

            if (levelData.InitializeHeroPosition)
            {
                _enemyFactory.CreateSpawnersRoot();
                await InitializeGameWorld(levelData);
                await InitializeSpawners(levelData);
            }
        }

        private async UniTask InitUIRoot() =>
            await _uiFactory.CreateUIRoot();

        private LevelStaticData LevelStaticData()
        {
            SceneId sceneId = Enum.Parse<SceneId>(SceneManager.GetActiveScene().name);
            return _staticDataService.ForLevel(sceneId);
        }

        private async UniTask InitializeGameWorld(LevelStaticData levelData)
        {
            _hero = await InitHero(levelData);
            await InitHud(_hero);
            await InitWindows(_hero);
            InitLevelTransfer(levelData);
            _adListener.Construct(_hero, _adsService, _progressService);
            _hero.StopHero();
        }

        private async UniTask InitializeSpawners(LevelStaticData levelData)
        {
            foreach (EnemySpawnerData spawnerData in levelData.EnemySpawners)
            {
                // foreach (AreaData area in levelData.AreaDatas)
                // {
                //     if (spawnerData.AreaTypeId == area.AreaTypeId)
                //     {
                // Debug.Log($"Area {area.AreaTypeId.ToString()}");
                await _enemyFactory.CreateSpawner(spawnerData.Position.AsUnityVector(), spawnerData.EnemyTypeId
                    // , area
                );
                // }
                // }
            }
        }

        private async UniTask<GameObject> InitHero(LevelStaticData levelStaticData) =>
            await _gameFactory.CreateHero(levelStaticData.InitialHeroPosition);

        private void InitLevelTransfer(LevelStaticData levelData)
        {
            GameObject findWithTag = GameObject.FindWithTag(FinishPointTag);
            findWithTag.GetComponent<Finish>().Construct(levelData.Level, levelData.LevelTransfer.TransferTo);
        }

        private async UniTask InitHud(GameObject hero)
        {
            if (_hud == null)
                _hud = await _uiFactory.CreateHud(hero);

            _hud.GetComponentInChildren<HitRedFlashShower>().Construct(hero.GetComponent<HeroHealth>());

            HeroHealth heroHealth = hero.GetComponentInChildren<HeroHealth>();
            heroHealth.Construct(_staticDataService);

            if (_inputService is MobileInputService)
            {
                hero.GetComponent<HeroMovement>()
                    .ConstructMobilePlatform(_staticDataService, _hud.GetComponentInChildren<MoveJoystick>());
                hero.GetComponent<HeroRotating>()
                    .ConstructMobilePlatform(_hud.GetComponentInChildren<LookJoystick>(), _progressService);
            }
            else
            {
                hero.GetComponent<HeroMovement>()
                    .ConstructDesktopPlatform(_staticDataService, _inputService as DesktopInputService);
                hero.GetComponent<HeroRotating>().ConstructDesktopPlatform(_inputService, _progressService);
            }

            hero.GetComponent<HeroReloading>().Construct(_staticDataService);
            HeroReloading heroReloading = hero.GetComponent<HeroReloading>();
            HeroDeath heroDeath = hero.GetComponentInChildren<HeroDeath>();
            HeroWeaponSelection heroWeaponSelection = hero.GetComponentInChildren<HeroWeaponSelection>();
            heroWeaponSelection.Construct(heroDeath, heroReloading);
            _hud.GetComponentInChildren<Health>().Construct(heroHealth);
            _hud.GetComponentInChildren<WeaponsSelector>().Construct(heroWeaponSelection);
            _hud.GetComponentInChildren<ReloadingIndicator>().Construct(heroReloading, heroWeaponSelection);
            _hud.GetComponentInChildren<Crosshairs>().Construct(heroReloading, heroWeaponSelection);
        }

        private async UniTask InitWindows(GameObject hero)
        {
            if (_openSettings == null)
                _openSettings = _hud.GetComponent<OpenSettings>();

            if (_mobileInput == null)
                _mobileInput = _hud.GetComponentInChildren<MobileInput>();

            GameObject shopWindow = await _uiFactory.CreateShopWindow();
            shopWindow.GetComponent<ShopWindow>()
                .Construct(hero, _openSettings, _mobileInput);
            shopWindow.GetComponent<ShopItemsGenerator>()?.Construct(hero);
            GameObject deathWindow = await _uiFactory.CreateDeathWindow();
            deathWindow.GetComponent<DeathWindow>()
                .Construct(hero, _openSettings, _mobileInput);
            GameObject settingsWindow = await _uiFactory.CreateSettingsWindow();
            settingsWindow.GetComponent<SettingsWindow>()
                .Construct(hero, _openSettings, _mobileInput);
            GameObject giftsWindow = await _uiFactory.CreateGiftsWindow();
            giftsWindow.GetComponent<GiftsGenerator>()?.Construct(hero);
            giftsWindow.GetComponent<GiftsWindow>()
                ?.Construct(hero, _openSettings, _mobileInput);
            GameObject resultsWindow = await _uiFactory.CreateResultsWindow();
            resultsWindow.GetComponent<ResultsWindow>()
                ?.Construct(hero, _openSettings, _mobileInput);
            GameObject authorizationWindow = await _uiFactory.CreateAuthorizationWindow();
            authorizationWindow.GetComponent<AuthorizationWindow>()
                ?.Construct(hero, _openSettings, _mobileInput);
            GameObject leaderBoardWindow = await _uiFactory.CreateLeaderBoardWindow();
            leaderBoardWindow.GetComponent<LeaderBoardWindow>()
                ?.Construct(hero, _openSettings, _mobileInput);
            GameObject gameEndWindow = await _uiFactory.CreateGameEndWindow();
            gameEndWindow.GetComponent<GameFinishedWindow>()
                ?.Construct(hero, _openSettings, _mobileInput);
            GameObject startWindow = await _uiFactory.CreateStartWindow();
            startWindow.GetComponent<StartWindow>()
                ?.Construct(hero, _openSettings, _progressService, _adsService, _mobileInput);
            GameObject startNewGameWindow = await _uiFactory.CreateStartWindow();
            startNewGameWindow.GetComponent<StartNewGameWindow>()
                ?.Construct(hero, _openSettings, _mobileInput);

            _windowService.AddWindow(WindowId.Shop, shopWindow);
            _windowService.AddWindow(WindowId.Death, deathWindow);
            _windowService.AddWindow(WindowId.Gifts, giftsWindow);
            _windowService.AddWindow(WindowId.Result, resultsWindow);
            _windowService.AddWindow(WindowId.Authorization, authorizationWindow);
            _windowService.AddWindow(WindowId.LeaderBoard, leaderBoardWindow);
            _windowService.AddWindow(WindowId.GameEnd, gameEndWindow);
            _windowService.AddWindow(WindowId.Settings, settingsWindow);
            _windowService.AddWindow(WindowId.Start, startWindow);
            _windowService.AddWindow(WindowId.StartNewGame, startNewGameWindow);

            _windowService.Show<StartWindow>(WindowId.Start);
        }
    }
}