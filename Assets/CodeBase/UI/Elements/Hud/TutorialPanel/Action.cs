using CodeBase.UI.Services;
using NTC.Global.Cache;
using NTC.Global.System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements.Hud.TutorialPanel
{
    public class Action : MonoCache
    {
        [SerializeField] public TextMeshProUGUI Text;

        private Image _image;

        private void Awake()
        {
            _image = Get<Image>();
            _image.ChangeImageAlpha(Constants.HalfVisible);
        }

        public void Show() =>
            gameObject.Enable();

        public void Hide() =>
            gameObject.Disable();
    }
}