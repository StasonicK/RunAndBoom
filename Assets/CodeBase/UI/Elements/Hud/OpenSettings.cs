using CodeBase.Services;
using CodeBase.Services.Input;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Settings;
using NTC.Global.Cache;
using NTC.Global.System;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements.Hud
{
    public class OpenSettings : MonoCache
    {
        [SerializeField] private Button _settingsButton;

        private IWindowService _windowService;

        private void Start()
        {
            _windowService = AllServices.Container.Single<IWindowService>();

            if (AllServices.Container.Single<IInputService>() is DesktopInputService)
            {
                _settingsButton.gameObject.Disable();
            }
            else
            {
                _settingsButton.gameObject.Enable();
                _settingsButton.onClick.AddListener(ShowSettingsWindow);
            }
        }

        protected override void Run()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                ShowSettingsWindow();
        }

        private void ShowSettingsWindow() =>
            _windowService.Show<SettingsWindow>(WindowId.Settings);
    }
}