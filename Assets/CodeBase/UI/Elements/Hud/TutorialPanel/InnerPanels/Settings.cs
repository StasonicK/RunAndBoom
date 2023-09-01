using NTC.Global.System;
using UnityEngine;

namespace CodeBase.UI.Elements.Hud.TutorialPanel.InnerPanels
{
    public class Settings : IconsPanel
    {
        [SerializeField] private Action _esc;

        public override void ShowForPc() =>
            gameObject.Enable();

        public override void ShowForMobile() =>
            gameObject.Disable();

        protected override void RuChosen()
        {
        }

        protected override void TrChosen()
        {
        }

        protected override void EnChosen()
        {
        }
    }
}