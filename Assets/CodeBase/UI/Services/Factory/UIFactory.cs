using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.Registrator;
using CodeBase.UI.Elements.Hud.TutorialPanel;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.UI.Services.Factory
{
    public class UIFactory : IUIFactory
    {
        private IAssets _assets;
        private IRegistratorService _registratorService;
        private Transform _uiRoot;
        private TutorialPanel _tutorialPanel;

        public UIFactory(IAssets assets, IRegistratorService registratorService)
        {
            _assets = assets;
            _registratorService = registratorService;
        }

        public async UniTask CreateUIRoot()
        {
            GameObject root = await _assets.Load<GameObject>(AssetAddresses.UIRoot);
            GameObject gameObject = Object.Instantiate(root);
            _uiRoot = gameObject.transform;
        }

        public Transform GetUIRoot() =>
            _uiRoot;

        public async UniTask<GameObject> CreateHud(GameObject hero)
        {
            var hudGameObject = await _registratorService.InstantiateRegisteredAsync(AssetAddresses.Hud);
            _tutorialPanel = hudGameObject.GetComponentInChildren<TutorialPanel>();

            // if (Application.isMobilePlatform)
            //     hudGameObject.GetComponentInChildren<LookArea>().Construct(hero.GetComponent<HeroRotating>());

            return hudGameObject;
        }

        public async UniTask<GameObject> CreateShopWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.ShopWindow, _uiRoot);

        public async UniTask<GameObject> CreateDeathWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.DeathWindow, _uiRoot);

        public async UniTask<GameObject> CreateSettingsWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.SettingsWindow, _uiRoot);

        public async UniTask<GameObject> CreateGiftsWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.GiftsWindow, _uiRoot);

        public async UniTask<GameObject> CreateResultsWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.ResultWindow, _uiRoot);

        public async UniTask<GameObject> CreateAuthorizationWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.AuthorizationWindow, _uiRoot);

        public async UniTask<GameObject> CreateLeaderBoardWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.LeaderBoardWindow, _uiRoot);

        public async UniTask<GameObject> CreateGameEndWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.GameEndWindow, _uiRoot);

        public async UniTask<GameObject> CreateStartWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.StartWindow, _uiRoot);

        public async UniTask<GameObject> CreateStartNewGameWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.StartNewGameWindow, _uiRoot);

        public TutorialPanel GetTutorialPanel() =>
            _tutorialPanel;
    }
}