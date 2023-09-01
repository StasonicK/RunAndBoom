using CodeBase.Data;
using CodeBase.StaticData.Items.Gifts;
using CodeBase.UI.Services;

namespace CodeBase.UI.Windows.Common
{
    public abstract class MoneyItemBase : ItemBase
    {
        private MoneyTypeId _moneyTypeId;
        protected MoneyStaticData _moneyStaticData;

        protected override void OnEnabled() =>
            Button?.onClick.AddListener(Clicked);

        protected override void OnDisabled() =>
            Button?.onClick.RemoveListener(Clicked);

        protected void Construct(MoneyTypeId moneyTypeId, PlayerProgress progress)
        {
            base.Construct(progress);
            _moneyTypeId = moneyTypeId;
            FillData();
        }

        protected override void FillData()
        {
            _moneyStaticData = StaticDataService.ForMoney(_moneyTypeId);

            BackgroundIcon.ChangeImageAlpha(Constants.Visible);
            BackgroundIcon.color = Constants.ShopItemAmmo;
            MainIcon.sprite = _moneyStaticData.MainImage;
            MainIcon.ChangeImageAlpha(Constants.Visible);
            LevelIcon.ChangeImageAlpha(Constants.Invisible);
            AdditionalIcon.ChangeImageAlpha(Constants.Invisible);

            if (CostText != null)
                CostText.text = "";

            CountText.text = "";
            TitleText.text = $"+{_moneyStaticData.Value} $";
        }
    }
}