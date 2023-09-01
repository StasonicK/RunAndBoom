using System.Collections.Generic;
using CodeBase.Data.Perks;
using CodeBase.Data.Upgrades;
using CodeBase.StaticData.Items.Gifts;
using CodeBase.StaticData.Items.Shop.Ammo;
using CodeBase.StaticData.Items.Shop.Items;
using CodeBase.StaticData.Weapons;
using CodeBase.UI.Windows.Common;
using CodeBase.UI.Windows.Gifts.Items;
using CodeBase.UI.Windows.Shop;
using NTC.Global.System;
using UnityEngine;

namespace CodeBase.UI.Windows.Gifts
{
    public class GiftsGenerator : ItemsGeneratorBase
    {
        public new void Construct(GameObject hero) =>
            base.Construct(hero);

        public void SetMaxPrice(int maxPrice) =>
            Money = maxPrice;

        public override void Generate()
        {
            if (Progress == null)
                return;

            SetHighlightingVisibility(false);
            GetMoney();
            InitializeEmptyData();
            CreateAllItems();
            GenerateAllItems();
            SetHighlightingVisibility(true);
        }

        protected override void CreateAllItems()
        {
            CreateNextLevelPerks();
            CreateNextLevelUpgrades();
            CreateAmmunition();
            CreateWeapons();
            CreateItems();
            CreateMoney();
        }

        protected override void GenerateAllItems()
        {
            if (Progress.IsHardMode)
            {
                GenerateItems();
                GenerateMoney();
                GenerateAmmo();
                GenerateWeapons();
                GeneratePerks();
                GenerateUpgrades();
            }
            else
            {
                GenerateItems();
                GenerateAmmo();

                if (Progress.AllStats.AllMoney.Money < Constants.MinMoneyForGenerator)
                    GenerateMoney();

                GeneratePerks();
                GenerateUpgrades();
            }
        }

        protected override void CreateAmmoItem(GameObject hero, GameObject parent, List<AmmoItem> list,
            bool isClickable)
        {
            AmmoItem ammoItem = RandomService.NextFrom(list);
            AmmoGiftItem view = parent.GetComponent<ShopCell>().GetView(typeof(AmmoGiftItem)) as AmmoGiftItem;
            view?.Construct(hero.transform, ammoItem, Progress, this);
            view?.ChangeClickability(isClickable);
            parent.Enable();
        }

        protected override void CreateItemItem(GameObject hero, GameObject parent, ItemTypeId itemTypeId,
            bool isClickable)
        {
            ItemGiftItem view =
                parent.GetComponent<ShopCell>().GetView(typeof(ItemGiftItem)) as ItemGiftItem;
            view?.Construct(hero.transform, itemTypeId, Progress, Health, this);
            view?.ChangeClickability(isClickable);
            parent.Enable();
        }

        protected override void CreateUpgradeItem(GameObject hero, GameObject parent, List<UpgradeItemData> list,
            bool isClickable)
        {
            UpgradeItemData upgradeItemData = RandomService.NextFrom(list);
            UpgradeGiftItem view = parent.GetComponent<ShopCell>().GetView(typeof(UpgradeGiftItem)) as UpgradeGiftItem;
            view?.Construct(hero.transform, upgradeItemData, Progress, this);
            view?.ChangeClickability(isClickable);
            parent.Enable();
        }

        protected override void CreatePerkItem(GameObject hero, GameObject parent, List<PerkItemData> list,
            bool isClickable)
        {
            PerkItemData perkItemData = RandomService.NextFrom(list);
            PerkGiftItem view = parent.GetComponent<ShopCell>().GetView(typeof(PerkGiftItem)) as PerkGiftItem;
            view?.Construct(hero.transform, perkItemData, Progress, this);
            view?.ChangeClickability(isClickable);
            parent.Enable();
        }

        protected override void CreateWeaponItem(GameObject hero, GameObject parent, List<HeroWeaponTypeId> list,
            bool isClickable)
        {
            HeroWeaponTypeId weaponTypeId = RandomService.NextFrom(list);
            WeaponGiftItem view = parent.GetComponent<ShopCell>().GetView(typeof(WeaponGiftItem)) as WeaponGiftItem;
            view?.Construct(hero.transform, weaponTypeId, Progress, this);
            view?.ChangeClickability(isClickable);
            parent.Enable();
        }

        protected override void CreateMoneyItem(GameObject hero, GameObject parent, List<MoneyTypeId> list,
            bool isClickable)
        {
            MoneyTypeId moneyTypeId = RandomService.NextFrom(list);
            MoneyGiftItem view = parent.GetComponent<ShopCell>().GetView(typeof(MoneyGiftItem)) as MoneyGiftItem;
            view?.Construct(hero.transform, moneyTypeId, Progress, this);
            view?.ChangeClickability(isClickable);
            parent.Enable();
        }

        public void Clicked()
        {
            foreach (GameObject item in GameObjectItems)
                item.Disable();
        }
    }
}