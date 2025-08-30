using CodeBase.Services.Localization;
using CodeBase.UI.Windows.Common;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Windows.Settings
{
    public class SettingsText : BaseText
    {
        [SerializeField] private TextMeshProUGUI _restartText;
        [SerializeField] private TextMeshProUGUI _startNewGameText;

        protected override void RuChosen()
        {
            Title.text = LocalizationConstants.SettingsTitleRu;
            _restartText.text = LocalizationConstants.RestartRu;
            _startNewGameText.text = LocalizationConstants.StartNewGameRu;
        }

        protected override void EnChosen()
        {
            Title.text = LocalizationConstants.SettingsTitleEn;
            _restartText.text = LocalizationConstants.RestartEn;
            _startNewGameText.text = LocalizationConstants.StartNewGameEn;
        }

        protected override void TrChosen()
        {
            Title.text = LocalizationConstants.SettingsTitleTr;
            _restartText.text = LocalizationConstants.RestartTr;
            _startNewGameText.text = LocalizationConstants.StartNewGameTr;
        }
    }
}