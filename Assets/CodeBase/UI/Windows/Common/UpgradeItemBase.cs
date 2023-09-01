using CodeBase.Data;
using CodeBase.Data.Upgrades;
using CodeBase.StaticData.Items.Shop.WeaponsUpgrades;
using CodeBase.UI.Services;

namespace CodeBase.UI.Windows.Common
{
    public abstract class UpgradeItemBase : ItemBase
    {
        private UpgradeItemData _upgradeItemData;
        protected ShopUpgradeStaticData _upgradeStaticData;
        protected UpgradableWeaponStaticData _upgradableWeaponStaticData;
        protected UpgradeLevelInfoStaticData _upgradeLevelInfoStaticData;
        private ShopUpgradeLevelStaticData _shopUpgradeLevelStaticData;

        protected override void OnEnabled() =>
            Button?.onClick.AddListener(Clicked);

        protected override void OnDisabled() =>
            Button?.onClick.RemoveListener(Clicked);

        protected void Construct(UpgradeItemData upgradeItemData, PlayerProgress progress)
        {
            base.Construct(progress);
            _upgradeItemData = upgradeItemData;
            FillData();
        }

        protected override void FillData()
        {
            _upgradableWeaponStaticData = StaticDataService.ForUpgradableWeapon(_upgradeItemData.WeaponTypeId);
            _upgradeStaticData = StaticDataService.ForShopUpgrade(_upgradeItemData.UpgradeTypeId);
            _upgradeLevelInfoStaticData =
                StaticDataService.ForUpgradeLevelsInfo(_upgradeItemData.UpgradeTypeId, _upgradeItemData.LevelTypeId);
            _shopUpgradeLevelStaticData = StaticDataService.ForShopUpgradeLevel(_upgradeItemData.LevelTypeId);

            BackgroundIcon.color = Constants.ShopItemUpgrade;
            BackgroundIcon.ChangeImageAlpha(Constants.Visible);
            MainIcon.sprite = _upgradableWeaponStaticData.MainImage;
            MainIcon.ChangeImageAlpha(Constants.Visible);

            if (_shopUpgradeLevelStaticData.MainImage != null)
                LevelIcon.sprite = _shopUpgradeLevelStaticData.MainImage;

            LevelIcon.ChangeImageAlpha(_shopUpgradeLevelStaticData.MainImage != null
                ? Constants.Visible
                : Constants.Invisible);

            AdditionalIcon.sprite = _upgradeStaticData.MainImage;
            AdditionalIcon.ChangeImageAlpha(Constants.Visible);

            if (CostText != null)
                CostText.text = $"{_upgradeLevelInfoStaticData.Cost} $";

            CountText.text = "";
            TitleText.text =
                $"{LocalizationService.GetText(russian: _upgradeStaticData.RuTitle, turkish: _upgradeStaticData.TrTitle, english: _upgradeStaticData.EnTitle)} {_shopUpgradeLevelStaticData.Level} {LocalizationService.GetText(russian: _upgradableWeaponStaticData.RuTitle, turkish: _upgradableWeaponStaticData.TrTitle, english: _upgradableWeaponStaticData.EnTitle)}";
        }
    }
}