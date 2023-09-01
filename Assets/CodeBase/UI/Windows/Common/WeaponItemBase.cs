using CodeBase.Data;
using CodeBase.StaticData.Items.Shop.Weapons;
using CodeBase.StaticData.Weapons;
using CodeBase.UI.Services;

namespace CodeBase.UI.Windows.Common
{
    public abstract class WeaponItemBase : ItemBase
    {
        protected HeroWeaponTypeId _weaponTypeId;
        protected ShopWeaponStaticData _weaponStaticData;

        protected override void OnEnabled() =>
            Button?.onClick.AddListener(Clicked);

        protected override void OnDisabled() =>
            Button?.onClick.RemoveListener(Clicked);

        protected void Construct(HeroWeaponTypeId weaponTypeId, PlayerProgress progress)
        {
            base.Construct(progress);
            _weaponTypeId = weaponTypeId;
            FillData();
        }

        protected override void FillData()
        {
            _weaponStaticData = StaticDataService.ForShopWeapon(_weaponTypeId);

            BackgroundIcon.color = Constants.ShopItemWeapon;
            BackgroundIcon.ChangeImageAlpha(Constants.Visible);
            MainIcon.sprite = _weaponStaticData.MainImage;
            MainIcon.ChangeImageAlpha(Constants.Visible);
            LevelIcon.ChangeImageAlpha(Constants.Invisible);
            AdditionalIcon.ChangeImageAlpha(Constants.Invisible);

            if (CostText != null)
                CostText.text = $"{_weaponStaticData.Cost} $";

            CountText.text = "";
            TitleText.text =
                $"{LocalizationService.GetText(russian: _weaponStaticData.RuTitle, turkish: _weaponStaticData.TrTitle, english: _weaponStaticData.EnTitle)}";
        }
    }
}