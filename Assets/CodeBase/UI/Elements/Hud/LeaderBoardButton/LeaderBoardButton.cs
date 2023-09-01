using CodeBase.Services;
using CodeBase.Services.Ads;
using CodeBase.Services.Input;
using CodeBase.Services.PlayerAuthorization;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Authorization;
using CodeBase.UI.Windows.LeaderBoard;
using NTC.Global.Cache;
using NTC.Global.System;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements.Hud.LeaderBoardButton
{
    public class LeaderBoardButton : MonoCache
    {
        [SerializeField] private TutorialPanel.TutorialPanel _tutorialPanel;
        [SerializeField] private Button _button;

        private IWindowService _windowService;
        private IInputService _inputService;
        private IAdsService _adsService;
        private IAuthorization _authorization;
        private bool _isTutorialVisible;

        protected override void OnEnabled()
        {
            _isTutorialVisible = true;
            _button.gameObject.SetActive(_inputService is MobileInputService);

            if (_inputService is MobileInputService)
                _button.onClick.AddListener(CheckAuthorization);

            if (_windowService == null)
                _windowService = AllServices.Container.Single<IWindowService>();

            if (_inputService == null)
                _inputService = AllServices.Container.Single<IInputService>();

            if (_adsService == null)
                _adsService = AllServices.Container.Single<IAdsService>();

            if (_authorization == null)
                _authorization = AllServices.Container.Single<IAuthorization>();
        }

        protected override void OnDisabled()
        {
            _button.gameObject.Disable();

            if (_inputService is MobileInputService)
                _button.onClick.RemoveListener(CheckAuthorization);
        }

        private void Update()
        {
            if (_inputService is MobileInputService || !Input.GetKeyUp(KeyCode.Tab))
                return;

            CheckAuthorization();

            if (!_isTutorialVisible)
                return;

            _tutorialPanel.HidePanel();
            _isTutorialVisible = false;
        }

        private void CheckAuthorization()
        {
            if (Application.isEditor)
            {
                ToLeaderBoardWindow();
                return;
            }

            if (_adsService.IsInitialized())
                Authorize();
            else
                InitializeAds();
        }

        private void Authorize()
        {
            if (_authorization.IsAuthorized())
                ToLeaderBoardWindow();
            else
                ToAuthorizationWindow();
        }

        private void InitializeAds()
        {
            _adsService.OnInitializeSuccess += Authorize;
            StartCoroutine(_adsService.Initialize());
        }

        private void ToAuthorizationWindow() =>
            _windowService.Show<AuthorizationWindow>(WindowId.Authorization, false);

        private void ToLeaderBoardWindow() =>
            _windowService.Show<LeaderBoardWindow>(WindowId.LeaderBoard, false);
    }
}