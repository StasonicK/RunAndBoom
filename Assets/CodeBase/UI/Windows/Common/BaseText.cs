using CodeBase.Data.Settings;
using CodeBase.Services;
using CodeBase.Services.Localization;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Windows.Common
{
    public abstract class BaseText : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI Title;

        private ILocalizationService _localizationService;

        private void OnEnable()
        {
            _localizationService = AllServices.Container.Single<ILocalizationService>();
            _localizationService.LanguageChanged += ChangeText;
        }

        private void Start() =>
            ChangeText();

        protected abstract void RuChosen();
        protected abstract void EnChosen();
        protected abstract void TrChosen();

        public void ChangeText()
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