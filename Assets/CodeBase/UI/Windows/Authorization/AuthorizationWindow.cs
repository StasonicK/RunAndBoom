using CodeBase.Data.Progress;
using CodeBase.Services;
using CodeBase.Services.PlayerAuthorization;
using CodeBase.UI.Elements.Hud;
using CodeBase.UI.Elements.Hud.MobileInputPanel;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using CodeBase.UI.Windows.LeaderBoard;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Authorization
{
    public class AuthorizationWindow : WindowBase
    {
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _denyButton;

        private IAuthorization _authorization;
        private SceneId _nextScene;
        private int _maxPrice;

        private void OnEnable()
        {
            _applyButton.onClick.AddListener(Authorize);
            _denyButton.onClick.AddListener(Close);

            if (_authorization == null)
                _authorization = AllServices.Container.Single<IAuthorization>();

            if (Application.isEditor)
                _applyButton.onClick.AddListener(ToLeaderBoardWindow);
        }

        private void OnDisable()
        {
            _applyButton.onClick.RemoveListener(Authorize);
            _denyButton.onClick.RemoveListener(Close);

            if (Application.isEditor)
                _applyButton.onClick.RemoveListener(ToLeaderBoardWindow);
        }

        public void Construct(GameObject hero, OpenSettings openSettings, MobileInput mobileInput) =>
            base.Construct(hero, WindowId.LeaderBoard, openSettings, mobileInput);

        private void Authorize()
        {
            _authorization.OnAuthorizeSuccessCallback += RequestPersonalProfileDataPermission;
            _authorization.OnAuthorizeErrorCallback += ShowAuthorizeError;
            _authorization.Authorize();
        }

        private void RequestPersonalProfileDataPermission()
        {
            _authorization.OnAuthorizeSuccessCallback -= RequestPersonalProfileDataPermission;
            _authorization.OnRequestPersonalProfileDataPermissionSuccessCallback += ToLeaderBoardWindow;
            _authorization.OnRequestErrorCallback += ShowRequestError;
            _authorization.RequestPersonalProfileDataPermission();
        }

        private void ShowAuthorizeError(string error)
        {
            Debug.Log($"ServiceAuthorization ShowAuthorizeError {error}");
            _authorization.OnAuthorizeErrorCallback -= ShowAuthorizeError;
        }

        private void ShowRequestError(string error)
        {
            Debug.Log($"ServiceAuthorization ShowRequestError {error}");
            _authorization.OnRequestErrorCallback -= ShowRequestError;
        }

        private void ToLeaderBoardWindow()
        {
            _windowService.Show<LeaderBoardWindow>(WindowId.LeaderBoard, null, false);
            Close();
        }
    }
}