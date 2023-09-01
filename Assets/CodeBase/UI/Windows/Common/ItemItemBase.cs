using CodeBase.Data;
using CodeBase.Hero;
using CodeBase.StaticData.Items.Shop.Items;
using CodeBase.UI.Services;

namespace CodeBase.UI.Windows.Common
{
    public abstract class ItemItemBase : ItemBase
    {
        protected ShopItemStaticData _itemStaticData;
        private ItemTypeId _typeId;
        protected HeroHealth Health;

        protected override void OnEnabled() =>
            Button?.onClick.AddListener(Clicked);

        protected override void OnDisabled() =>
            Button?.onClick.RemoveListener(Clicked);

        protected void Construct(ItemTypeId typeId, HeroHealth health, PlayerProgress progress)
        {
            Health = health;
            _typeId = typeId;
            base.Construct(progress);
            FillData();
        }

        protected override void FillData()
        {
            _itemStaticData = StaticDataService.ForShopItem(_typeId);

            BackgroundIcon.color = Constants.ShopItemItem;
            BackgroundIcon.ChangeImageAlpha(Constants.Visible);
            MainIcon.sprite = _itemStaticData.MainImage;
            MainIcon.ChangeImageAlpha(Constants.Visible);
            LevelIcon.ChangeImageAlpha(Constants.Invisible);
            AdditionalIcon.ChangeImageAlpha(Constants.Invisible);

            if (CostText != null)
                CostText.text = $"{_itemStaticData.Cost} $";

            CountText.text = "";
            TitleText.text =
                $"{LocalizationService.GetText(russian: _itemStaticData.RuTitle, turkish: _itemStaticData.TrTitle, english: _itemStaticData.EnTitle)}";
        }
    }
}