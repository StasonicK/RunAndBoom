using System.Collections.Generic;
using System.Linq;
using CodeBase.Data;
using CodeBase.UI.Windows.Authorization;
using CodeBase.UI.Windows.Common;
using CodeBase.UI.Windows.Death;
using CodeBase.UI.Windows.GameEnd;
using CodeBase.UI.Windows.Gifts;
using CodeBase.UI.Windows.LeaderBoard;
using CodeBase.UI.Windows.Results;
using CodeBase.UI.Windows.Settings;
using CodeBase.UI.Windows.Shop;
using NTC.Global.System;
using UnityEngine;

namespace CodeBase.UI.Services.Windows
{
    public class WindowService : IWindowService
    {
        private Dictionary<WindowId, GameObject> _windows;

        public WindowService()
        {
            _windows = new Dictionary<WindowId, GameObject>(DataExtensions.GetValues<WindowId>().Count());
        }

        public WindowBase? Show<TWindowBase>(WindowId windowId, bool hideOthers = true)
        {
            WindowBase? window = null;

            switch (windowId)
            {
                case WindowId.Unknown:
                    break;
                case WindowId.Settings:
                    window = ShowWindow<SettingsWindow>(WindowId.Settings);
                    break;
                case WindowId.Death:
                    window = ShowWindow<DeathWindow>(WindowId.Death);
                    break;
                case WindowId.Authorization:
                    window = ShowWindow<AuthorizationWindow>(WindowId.Authorization);
                    break;
                case WindowId.LeaderBoard:
                    window = ShowWindow<LeaderBoardWindow>(WindowId.LeaderBoard);
                    break;
                case WindowId.Shop:
                    window = ShowWindow<ShopWindow>(WindowId.Shop);
                    break;
                case WindowId.Result:
                    window = ShowWindow<ResultsWindow>(WindowId.Result);
                    break;
                case WindowId.Gifts:
                    window = ShowWindow<GiftsWindow>(WindowId.Gifts);
                    break;
                case WindowId.GameEnd:
                    window = ShowWindow<GameEndWindow>(WindowId.GameEnd);
                    break;
            }

            if (hideOthers)
                HideOthers(windowId);

            return window;
        }

        public void HideAll()
        {
            foreach (var vk in _windows)
            {
                if (vk.Value.activeInHierarchy)
                    vk.Value.Disable();
            }

            _windows.Clear();
        }

        private void HideOthers(WindowId windowId)
        {
            foreach (var vk in _windows)
            {
                if (vk.Key != windowId && vk.Value.activeInHierarchy)
                    vk.Value.Disable();
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