using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Settings
{
    public class SettingsWindow : WindowBase
    {
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _closeButton;

        protected override void OnEnabled()
        {
            _restartButton.onClick.AddListener(RestartLevel);
            _closeButton.onClick.AddListener(Close);
        }

        protected override void OnDisabled()
        {
            _restartButton.onClick.RemoveListener(RestartLevel);
            _closeButton.onClick.RemoveListener(Close);
        }

        protected override void Run()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Close();
        }

        private void Close() =>
            Hide();

        public void Construct(GameObject hero) =>
            base.Construct(hero, WindowId.Settings);
    }
}