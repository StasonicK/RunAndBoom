﻿using System;
using System.Collections.Generic;
using CodeBase.Data.Perks;
using CodeBase.Data.Upgrades;
using CodeBase.Hero;
using CodeBase.StaticData.Items.Shop.Ammo;
using CodeBase.StaticData.Items.Shop.Items;
using CodeBase.StaticData.Weapons;
using CodeBase.UI.Windows.Common;
using CodeBase.UI.Windows.Shop.Items;
using UnityEngine;

namespace CodeBase.UI.Windows.Shop
{
    public class ShopItemsGenerator : ItemsGeneratorBase
    {
        public override event Action GenerationStarted;
        public override event Action GenerationEnded;

        public new void Construct(HeroHealth health)
        {
            base.Construct(health);
        }

        public override void Generate()
        {
            GenerationStarted?.Invoke();
            SetHighlightingVisibility(false);
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
            GenerateAmmo();
            GeneratePerks();
            GenerateUpgrades();
            GenerateWeapons();
        }

        protected override void CreateAmmoItem(GameObject parent, List<AmmoItem> list,
            bool isClickable)
        {
            AmmoItem ammoItem = RandomService.NextFrom(list);
            AmmoShopItem view = parent.GetComponent<ShopCell>().GetView(typeof(AmmoShopItem)) as AmmoShopItem;
            view?.Construct(ammoItem, Progress);
            view?.ChangeClickability(isClickable);
            parent.SetActive(true);
        }

        protected override void CreateItemItem(GameObject parent, ItemTypeId itemTypeId, bool isClickable)
        {
            ItemShopItem view =
                parent.GetComponent<ShopCell>().GetView(typeof(ItemShopItem)) as ItemShopItem;
            view?.Construct(itemTypeId, Progress, Health);
            view?.ChangeClickability(isClickable);
            parent.SetActive(true);
        }

        protected override void CreateUpgradeItem(GameObject parent, List<UpgradeItemData> list, bool isClickable)
        {
            UpgradeItemData upgradeItemData = RandomService.NextFrom(list);
            UpgradeShopItem view = parent.GetComponent<ShopCell>().GetView(typeof(UpgradeShopItem)) as UpgradeShopItem;
            view?.Construct(upgradeItemData, Progress);
            view?.ChangeClickability(isClickable);
            parent.SetActive(true);
        }

        protected override void CreatePerkItem(GameObject parent, List<PerkItemData> list, bool isClickable)
        {
            PerkItemData perkItemData = RandomService.NextFrom(list);
            PerkShopItem view = parent.GetComponent<ShopCell>().GetView(typeof(PerkShopItem)) as PerkShopItem;
            view?.Construct(perkItemData, Progress);
            view?.ChangeClickability(isClickable);
            parent.SetActive(true);
        }

        protected override void CreateWeaponItem(GameObject parent, List<HeroWeaponTypeId> list, bool isClickable)
        {
            HeroWeaponTypeId weaponTypeId = RandomService.NextFrom(list);
            WeaponShopItem view = parent.GetComponent<ShopCell>().GetView(typeof(WeaponShopItem)) as WeaponShopItem;
            view?.Construct(weaponTypeId, Progress);
            view?.ChangeClickability(isClickable);
            parent.SetActive(true);
        }
    }
}