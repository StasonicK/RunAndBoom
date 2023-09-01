using NTC.Global.Cache;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements.Hud
{
    public class HpBar : MonoCache
    {
        [SerializeField] private Slider _slider;

        public void SetValue(float current, float max) =>
            _slider.value = current / max;
    }
}