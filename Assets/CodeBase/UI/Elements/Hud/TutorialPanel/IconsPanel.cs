using CodeBase.UI.Windows.Common;
using NTC.Global.System;

namespace CodeBase.UI.Elements.Hud.TutorialPanel
{
    public abstract class IconsPanel : BaseText
    {
        public abstract void ShowForPc();

        public abstract void ShowForMobile();

        public void Show() =>
            gameObject.Enable();

        public void Hide() =>
            gameObject.Disable();
    }
}