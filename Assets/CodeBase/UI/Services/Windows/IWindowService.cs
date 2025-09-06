using System.Collections.Generic;
using CodeBase.Services;
using CodeBase.UI.Windows.Common;
using JetBrains.Annotations;
using UnityEngine;

namespace CodeBase.UI.Services.Windows
{
    public interface IWindowService : IService
    {
        WindowBase? Show<TWindowBase>(WindowId windowId, [CanBeNull] List<WindowId> nonhidableWindows = null,
            bool hideOthers = true);

        void AddWindow(WindowId windowId, GameObject window);
        bool IsAnotherActive(WindowId windowId);
        void ClearAll();
        void HideAll();
    }
}