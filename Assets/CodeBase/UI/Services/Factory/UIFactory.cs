using System.Threading.Tasks;
using CodeBase.Hero;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.Registrator;
using CodeBase.UI.Elements.Hud.MobileInputPanel;
using CodeBase.UI.Elements.Hud.TutorialPanel;
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

        public async Task CreateUIRoot()
        {
            GameObject root = await _assets.Load<GameObject>(AssetAddresses.UIRoot);
            GameObject gameObject = Object.Instantiate(root);
            _uiRoot = gameObject.transform;
        }

        public Transform GetUIRoot() =>
            _uiRoot;

        public async Task<GameObject> CreateHud(GameObject hero)
        {
            var hudGameObject = await _registratorService.InstantiateRegisteredAsync(AssetAddresses.Hud);
            _tutorialPanel = hudGameObject.GetComponentInChildren<TutorialPanel>();

            // if (Application.isMobilePlatform)
            //     hudGameObject.GetComponentInChildren<LookArea>().Construct(hero.GetComponent<HeroRotating>());

            return hudGameObject;
        }

        public async Task<GameObject> CreateShopWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.ShopWindow, _uiRoot);

        public async Task<GameObject> CreateDeathWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.DeathWindow, _uiRoot);

        public async Task<GameObject> CreateSettingsWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.SettingsWindow, _uiRoot);

        public async Task<GameObject> CreateGiftsWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.GiftsWindow, _uiRoot);

        public async Task<GameObject> CreateResultsWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.ResultWindow, _uiRoot);

        public Task<GameObject> CreateAuthorizationWindow() =>
            _registratorService.InstantiateRegisteredAsync(AssetAddresses.AuthorizationWindow, _uiRoot);

        public Task<GameObject> CreateLeaderBoardWindow() =>
            _registratorService.InstantiateRegisteredAsync(AssetAddresses.LeaderBoardWindow, _uiRoot);

        public async Task<GameObject> CreateGameEndWindow() =>
            await _registratorService.InstantiateRegisteredAsync(AssetAddresses.GameEndWindow, _uiRoot);

        public TutorialPanel GetTutorialPanel() =>
            _tutorialPanel;
    }
}