using System.Collections.Generic;
using System.Linq;
using CodeBase.Data;
using CodeBase.UI.Windows.Authorization;
using CodeBase.UI.Windows.Common;
using CodeBase.UI.Windows.Death;
using CodeBase.UI.Windows.GameFinished;
using CodeBase.UI.Windows.Gifts;
using CodeBase.UI.Windows.LeaderBoard;
using CodeBase.UI.Windows.Results;
using CodeBase.UI.Windows.Settings;
using CodeBase.UI.Windows.Shop;
using CodeBase.UI.Windows.StartNewGame;
using JetBrains.Annotations;
using UnityEngine;

namespace CodeBase.UI.Services.Windows
{
    public class WindowService : IWindowService
    {
        private Dictionary<WindowId, GameObject> _windows;

        private bool _isActive;
        private WindowBase? _window;

        public WindowService()
        {
            _windows = new Dictionary<WindowId, GameObject>(DataExtensions.GetValues<WindowId>().Count());
        }

        public WindowBase? Show<TWindowBase>(WindowId windowId, [CanBeNull] List<WindowId> nonhidableWindows = null,
            bool hideOthers = true)
        {
            _isActive = false;

            if (nonhidableWindows != null)
            {
                if (nonhidableWindows.Count > 0)
                {
                    foreach (WindowId id in nonhidableWindows)
                    {
                        if (IsAnotherActive(id))
                            _isActive = true;
                    }
                }
            }

            if (_isActive)
                return _window;

            switch (windowId)
            {
                case WindowId.Unknown:
                    break;
                case WindowId.Settings:
                    _window = ShowWindow<SettingsWindow>(WindowId.Settings);
                    break;
                case WindowId.Death:
                    _window = ShowWindow<DeathWindow>(WindowId.Death);
                    break;
                case WindowId.Authorization:
                    _window = ShowWindow<AuthorizationWindow>(WindowId.Authorization);
                    break;
                case WindowId.LeaderBoard:
                    _window = ShowWindow<LeaderBoardWindow>(WindowId.LeaderBoard);
                    break;
                case WindowId.Shop:
                    _window = ShowWindow<ShopWindow>(WindowId.Shop);
                    break;
                case WindowId.Result:
                    _window = ShowWindow<ResultsWindow>(WindowId.Result);
                    break;
                case WindowId.Gifts:
                    _window = ShowWindow<GiftsWindow>(WindowId.Gifts);
                    break;
                case WindowId.GameEnd:
                    _window = ShowWindow<GameFinishedWindow>(WindowId.GameEnd);
                    break;
                // case WindowId.Start:
                //     _window = ShowWindow<StartWindow>(WindowId.Start);
                //     break;
                case WindowId.StartNewGame:
                    _window = ShowWindow<StartNewGameWindow>(WindowId.StartNewGame);
                    break;
            }

            if (hideOthers)
                HideOthers(windowId);

            return _window;
        }

        public void ClearAll()
        {
            foreach (var vk in _windows)
            {
                if (vk.Value.activeInHierarchy)
                    vk.Value.SetActive(false);
            }

            _windows.Clear();
        }

        public void HideAll()
        {
            foreach (KeyValuePair<WindowId, GameObject> pair in _windows)
            {
                WindowBase window = pair.Value?.GetComponent<WindowBase>();
                window?.Hide();
            }
        }

        private void HideOthers(WindowId windowId)
        {
            foreach (var vk in _windows)
            {
                if (vk.Key != windowId && vk.Value.activeInHierarchy)
                    vk.Value.SetActive(false);
            }
        }

        public void AddWindow(WindowId windowId, GameObject window) =>
            _windows[windowId] = window;

        private T ShowWindow<T>(WindowId windowId) where T : WindowBase
        {
            _windows.TryGetValue(windowId, out GameObject windowGameObject);
            T window = windowGameObject?.GetComponent<T>();
            window.Show();
            return window;
        }

        public bool IsAnotherActive(WindowId windowId)
        {
            foreach (var vk in _windows)
            {
                if (vk.Key != windowId && vk.Value.activeInHierarchy)
                    return true;
            }

            return false;
        }
    }
}