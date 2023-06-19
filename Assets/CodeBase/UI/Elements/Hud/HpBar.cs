﻿using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements.Hud
{
    public class HpBar : MonoBehaviour
    {
        [SerializeField] private Slider _slider;

        public void SetValue(float current, float max) =>
            _slider.value = current / max;
    }
}