using CodeBase.Data;
using CodeBase.Data.Settings;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factories;
using CodeBase.Services;
using CodeBase.Services.Ads;
using CodeBase.Services.Constructor;
using CodeBase.Services.Input;
using CodeBase.Services.LeaderBoard;
using CodeBase.Services.Localization;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.PlayerAuthorization;
using CodeBase.Services.Pool;
using CodeBase.Services.Randomizer;
using CodeBase.Services.Registrator;
using CodeBase.Services.SaveLoad;
using CodeBase.Services.StaticData;
using CodeBase.UI.Services.Factory;
using CodeBase.UI.Services.Windows;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class BootstrapState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private AllServices _services;
        private Language _language;

        public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader, AllServices services,
            Language language)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _services = services;
            _language = language;

            RegisterServices();
            SetTargetFrameRate();
        }

        public void Enter() =>
            _sceneLoader.Load(scene: Scene.Initial, onLoaded: EnterLoadLevel);

        private void EnterLoadLevel() =>
            _stateMachine.Enter<LoadPlayerProgressState, bool>(false);

        private void RegisterServices()
        {
            _services.RegisterSingle<ILocalizationService>((new LocalizationService(_language)));
            _services.RegisterSingle<IAdsService>((new YandexAdsService()));
            _services.RegisterSingle<ILeaderboardService>((new YandexLeaderboardService()));
            _services.RegisterSingle<IAuthorization>((new YandexAuthorization()));
            RegisterStaticData();
            _services.RegisterSingle<IGameStateMachine>(_stateMachine);
            RegisterAssetsProvider();
            _services.RegisterSingle<IInputService>(InputService());
            _services.RegisterSingle<IRandomService>(new RandomService());
            _services.RegisterSingle<IPlayerProgressService>(new PlayerProgressService());
            _services.RegisterSingle<IRegistratorService>(new RegistratorService(_services.Single<IAssets>()));
            _services.RegisterSingle<IConstructorService>(new ConstructorService());
            _services.RegisterSingle<IObjectsPoolService>(new ObjectsPoolService(_services.Single<IAssets>(),
                _services.Single<IConstructorService>(), _services.Single<IStaticDataService>()));

            _services.RegisterSingle<IUIFactory>(
                new UIFactory(_services.Single<IAssets>(), _services.Single<IRegistratorService>())
            );

            _services.RegisterSingle<IWindowService>(new WindowService());

            _services.RegisterSingle<IGameFactory>(
                new GameFactory(
                    _services.Single<IAssets>(), _services.Single<IObjectsPoolService>(),
                    _services.Single<IRegistratorService>(), _services.Single<IGameStateMachine>()
                ));

            _services.RegisterSingle<IEnemyFactory>(
                new EnemyFactory(_services.Single<IInputService>(),
                    _services.Single<IAssets>(), _services.Single<IStaticDataService>(),
                    _services.Single<IRegistratorService>(), _services.Single<IGameFactory>()
                ));

            _services.RegisterSingle<ISaveLoadService>(
                new SaveLoadService(_services.Single<IGameFactory>()));
        }

        private void RegisterStaticData()
        {
            IStaticDataService staticData = new StaticDataService();
            staticData.Load();
            _services.RegisterSingle(staticData);
        }

        private void RegisterAssetsProvider()
        {
            var assetsProvider = new AssetsProvider();
            assetsProvider.Initialize();
            _services.RegisterSingle<IAssets>(assetsProvider);
        }

        public void Exit()
        {
        }

        private static IInputService InputService()
        {
            PlayerInput playerInput = new PlayerInput();
            return
                // Application.isMobilePlatform
                    // ? 
                    new MobileInputService()
                    // : new DesktopInputService(playerInput)
                ;
        }

        private void SetTargetFrameRate()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
        }
    }
}