﻿using CodeBase.Services.Localization;

namespace CodeBase.UI.Windows.LeaderBoard
{
    public class LeaderBoardText : BaseText
    {
        protected override void RuChosen() =>
            Title.text = LocalizationConstants.LeaderBoardTitleRu;

        protected override void TrChosen() =>
            Title.text = LocalizationConstants.LeaderBoardTitleTr;

        protected override void EnChosen() =>
            Title.text = LocalizationConstants.LeaderBoardTitleEn;
    }
}