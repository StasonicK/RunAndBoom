using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.UI.Elements.Hud.MobileInputPanel.Joysticks
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class FloatingJoystick : MonoCache
    {
        [HideInInspector] public RectTransform RectTransform;
        public RectTransform Knob;

        private void Awake() =>
            RectTransform = Get<RectTransform>();
    }
}