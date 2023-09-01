using System;
using System.Collections.Generic;
using CodeBase.Data.Perks;
using CodeBase.Data.Upgrades;
using CodeBase.StaticData.Items.Shop.Ammo;
using CodeBase.StaticData.Items.Shop.Items;
using CodeBase.StaticData.Weapons;
using CodeBase.UI.Windows.Common;
using CodeBase.UI.Windows.Shop.Items;
using NTC.Global.System;
using UnityEngine;

namespace CodeBase.UI.Windows.Shop
{
    public class ShopItemsGenerator : ItemsGeneratorBase
    {
        public override event Action GenerationStarted;
        public override event Action GenerationEnded;

        public new void Construct(GameObject hero) =>
            base.Construct(hero);

        public override void Generate()
        {
            if (Progress == null)
                return;

            GenerationStarted?.Invoke();
            SetHighlightingVisibility(false);
            GetMoney();
            InitializeEmptyData();
            CreateAllItems();
            GenerateAllItems();
            SetHighlightingVisibility(true);
            GenerationEnded?.Invoke();
        }

        protected override void CreateAllItems()
        {
            CreateNextLevelPerks();
            CreateNextLevelUpgrades();
            CreateAmmunition();
            CreateWeapons();
            CreateItems();
        }

        protected override void GenerateAllItems()
        {
            GenerateItems();
            GenerateWeapons();
            GenerateAmmo();
            GeneratePerks();
            GenerateUpgrades();
        }

        protected override void CreateAmmoItem(GameObject hero, GameObject parent, List<AmmoItem> list,
            bool isClickable)
        {
            AmmoItem ammoItem = RandomService.NextFrom(list);
            AmmoShopItem view = parent.GetComponent<ShopCell>().GetView(typeof(AmmoShopItem)) as AmmoShopItem;
            view?.Construct(hero.transform, ammoItem, Progress);
            view?.ChangeClickability(isClickable);
            parent.Enable();
        }

        protected override void CreateItemItem(GameObject hero, GameObject parent, ItemTypeId itemTypeId,
            bool isClickable)
        {
            ItemShopItem view =
                parent.GetComponent<ShopCell>().GetView(typeof(ItemShopItem)) as ItemShopItem;
            view?.Construct(hero.transform, itemTypeId, Health, Progress);
            view?.ChangeClickability(isClickable);
            parent.Enable();
        }

        protected override void CreateUpgradeItem(GameObject hero, GameObject parent, List<UpgradeItemData> list,
            bool isClickable)
        {
            UpgradeItemData upgradeItemData = RandomService.NextFrom(list);
            UpgradeShopItem view = parent.GetComponent<ShopCell>().GetView(typeof(UpgradeShopItem)) as UpgradeShopItem;
            view?.Construct(hero.transform, upgradeItemData, Progress);
            view?.ChangeClickability(isClickable);
            parent.Enable();
        }

        protected override void CreatePerkItem(GameObject hero, GameObject parent, List<PerkItemData> list,
            bool isClickable)
        {
            PerkItemData perkItemData = RandomService.NextFrom(list);
            PerkShopItem view = parent.GetComponent<ShopCell>().GetView(typeof(PerkShopItem)) as PerkShopItem;
            view?.Construct(hero.transform, perkItemData, Progress);
            view?.ChangeClickability(isClickable);
            parent.Enable();
        }

        protected override void CreateWeaponItem(GameObject hero, GameObject parent, List<HeroWeaponTypeId> list,
            bool isClickable)
        {
            HeroWeaponTypeId weaponTypeId = RandomService.NextFrom(list);
            WeaponShopItem view = parent.GetComponent<ShopCell>().GetView(typeof(WeaponShopItem)) as WeaponShopItem;
            view?.Construct(hero.transform, weaponTypeId, Progress);
            view?.ChangeClickability(isClickable);
            parent.Enable();
        }
    }
}