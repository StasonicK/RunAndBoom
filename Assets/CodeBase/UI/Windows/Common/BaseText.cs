using CodeBase.Data.Settings;
using CodeBase.Services;
using CodeBase.Services.Localization;
using NTC.Global.Cache;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Windows.Common
{
    public abstract class BaseText : MonoCache
    {
        [SerializeField] protected TextMeshProUGUI Title;

        private ILocalizationService _localizationService;

        protected override void OnEnabled()
        {
            _localizationService = AllServices.Container.Single<ILocalizationService>();
            _localizationService.LanguageChanged += ChangeText;
        }

        private void Start() =>
            ChangeText();

        protected abstract void RuChosen();
        protected abstract void TrChosen();
        protected abstract void EnChosen();

        private void ChangeText()
        {
            switch (AllServices.Container.Single<ILocalizationService>().Language)
            {
                case Language.RU:
                    RuChosen();
                    break;
                case Language.TR:
                    TrChosen();
                    break;
                default:
                    EnChosen();
                    break;
            }
        }
    }
}