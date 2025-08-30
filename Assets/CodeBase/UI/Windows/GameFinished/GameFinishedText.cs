using CodeBase.Services.Localization;
using CodeBase.UI.Windows.Common;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Windows.GameFinished
{
    public class GameFinishedText : BaseText
    {
        [SerializeField] private TextMeshProUGUI _writeReviewText;
        [SerializeField] private TextMeshProUGUI _startNewStandardGameText;
        [SerializeField] private TextMeshProUGUI _startNewAsianGameText;

        protected override void RuChosen()
        {
            Title.text = LocalizationConstants.GameFinishedTitleRu;
            _writeReviewText.text = LocalizationConstants.GameEndWriteReviewRu;
            _startNewStandardGameText.text = LocalizationConstants.GameEndStartNewStardardGameRu;
            _startNewAsianGameText.text = LocalizationConstants.GameEndStartNewAsianGameRu;
        }

        protected override void EnChosen()
        {
            Title.text = LocalizationConstants.GameFinishedTitleEn;
            _writeReviewText.text = LocalizationConstants.GameEndWriteReviewEn;
            _startNewStandardGameText.text = LocalizationConstants.GameEndStartNewStardardGameEn;
            _startNewAsianGameText.text = LocalizationConstants.GameEndStartNewAsianGameEn;
        }

        protected override void TrChosen()
        {
            Title.text = LocalizationConstants.GameFinishedTitleTr;
            _writeReviewText.text = LocalizationConstants.GameEndWriteReviewTr;
            _startNewStandardGameText.text = LocalizationConstants.GameEndStartNewStardardGameTr;
            _startNewAsianGameText.text = LocalizationConstants.GameEndStartNewAsianGameTr;
        }
    }
}