using CodeBase.Services.Localization;
using CodeBase.UI.Windows.Common;

namespace CodeBase.UI.Windows.StartNewGame
{
    public class StartNewGameText: BaseText
    {
        protected override void RuChosen()
        {
           Title.text = LocalizationConstants.StartNewGameRu;
        }

        protected override void EnChosen()
        {
            Title.text = LocalizationConstants.StartNewGameEn;
        }

        protected override void TrChosen()
        {
            Title.text = LocalizationConstants.StartNewGameTr;
        }
    }
}