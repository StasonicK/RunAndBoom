using CodeBase.Services.Localization;
using CodeBase.UI.Windows.Common;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Windows.StartNewGame
{
    public class StartNewGameText : BaseText
    {
        [SerializeField] private TextMeshProUGUI _startNewStandardGameButtonText;
        [SerializeField] private TextMeshProUGUI _startNewAsianGameButtonText;

        protected override void RuChosen()
        {
            Title.text = LocalizationConstants.StartNewGameRu;
            _startNewStandardGameButtonText.text = LocalizationConstants.GameEndStartNewStardardGameRu;
            _startNewAsianGameButtonText.text = LocalizationConstants.GameEndStartNewAsianGameRu;
        }

        protected override void EnChosen()
        {
            Title.text = LocalizationConstants.StartNewGameEn;
            _startNewStandardGameButtonText.text = LocalizationConstants.GameEndStartNewStardardGameEn;
            _startNewAsianGameButtonText.text = LocalizationConstants.GameEndStartNewAsianGameEn;
        }

        protected override void TrChosen()
        {
            Title.text = LocalizationConstants.StartNewGameTr;
            _startNewStandardGameButtonText.text = LocalizationConstants.GameEndStartNewStardardGameTr;
            _startNewAsianGameButtonText.text = LocalizationConstants.GameEndStartNewAsianGameTr;
        }
    }
}