using CodeBase.Data;
using CodeBase.Data.Settings;
using CodeBase.Services;
using CodeBase.Services.Localization;
using NTC.Global.Cache;
using NTC.Global.System;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Settings
{
    public class LanguageChanger : MonoCache
    {
        [SerializeField] private Button _ruButton;
        [SerializeField] private Button _trButton;
        [SerializeField] private Button _enButton;
        [SerializeField] private GameObject _ruSelection;
        [SerializeField] private GameObject _trSelection;
        [SerializeField] private GameObject _enSelection;

        private PlayerProgress _progress;
        private ILocalizationService _localizationService;

        protected override void OnEnabled()
        {
            _ruButton.onClick.AddListener(RuClicked);
            _trButton.onClick.AddListener(TrClicked);
            _enButton.onClick.AddListener(EnClicked);
            _localizationService = AllServices.Container.Single<ILocalizationService>();
            _localizationService.LanguageChanged += ChangeHighlighting;
            ChangeHighlighting();
        }

        protected override void OnDisabled()
        {
            _ruButton.onClick.RemoveListener(RuClicked);
            _trButton.onClick.RemoveListener(TrClicked);
            _enButton.onClick.RemoveListener(EnClicked);

            if (_localizationService != null)
                _localizationService.LanguageChanged -= ChangeHighlighting;
        }

        private void RuClicked() =>
            _localizationService.ChangeLanguage(Language.RU);

        private void TrClicked() =>
            _localizationService.ChangeLanguage(Language.TR);

        private void EnClicked() =>
            _localizationService.ChangeLanguage(Language.EN);

        private void ChangeHighlighting()
        {
            switch (_localizationService.Language)
            {
                case Language.RU:
                    _ruSelection.Enable();
                    _trSelection.Disable();
                    _enSelection.Disable();
                    break;
                case Language.TR:
                    _trSelection.Enable();
                    _ruSelection.Disable();
                    _enSelection.Disable();
                    break;
                default:
                    _enSelection.Enable();
                    _ruSelection.Disable();
                    _trSelection.Disable();
                    break;
            }
        }
    }
}