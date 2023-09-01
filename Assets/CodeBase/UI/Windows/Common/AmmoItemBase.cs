using CodeBase.Data;
using CodeBase.Services.Input;
using CodeBase.StaticData.Items.Shop.Ammo;
using CodeBase.UI.Services;

namespace CodeBase.UI.Windows.Common
{
    public abstract class AmmoItemBase : ItemBase
    {
        private AmmoCountType _countType;
        protected AmmoItem _ammoItem;
        protected ShopAmmoStaticData _shopAmmoStaticData;

        protected override void OnEnabled() =>
            Button?.onClick.AddListener(Clicked);

        protected override void OnDisabled() =>
            Button?.onClick.RemoveListener(Clicked);

        protected void Construct(AmmoItem ammoItem, PlayerProgress progress)
        {
            base.Construct(progress);
            _ammoItem = ammoItem;
            FillData();
        }

        protected override void FillData()
        {
            _shopAmmoStaticData = StaticDataService.ForShopAmmo(_ammoItem.WeaponTypeId, _ammoItem.CountType);

            BackgroundIcon.ChangeImageAlpha(Constants.Visible);
            BackgroundIcon.color = Constants.ShopItemAmmo;
            MainIcon.sprite = _shopAmmoStaticData.MainImage;
            MainIcon.ChangeImageAlpha(Constants.Visible);
            LevelIcon.ChangeImageAlpha(Constants.Invisible);
            AdditionalIcon.ChangeImageAlpha(Constants.Invisible);

            if (CostText != null)
                CostText.text = $"{_shopAmmoStaticData.Cost} $";

            int count = GetCount(_shopAmmoStaticData.Count);

            CountText.text = $"{count}";
            TitleText.text =
                LocalizationService.GetText(russian: $"{count} {_shopAmmoStaticData.RuTitle}",
                    turkish: $"{count} {_shopAmmoStaticData.TrTitle}",
                    english: $"{count} {_shopAmmoStaticData.EnTitle}");
        }

        protected int GetCount(int baseCount)
        {
            if (InputService is MobileInputService)
                return (int)(baseCount * Constants.MobileAmmoMultiplier);
            else
                return baseCount;
        }
    }
}