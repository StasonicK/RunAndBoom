﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Data;
using CodeBase.Data.Perks;
using CodeBase.Data.Upgrades;
using CodeBase.Data.Weapons;
using CodeBase.Hero;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.Randomizer;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Items;
using CodeBase.StaticData.Items.Gifts;
using CodeBase.StaticData.Items.Shop.Ammo;
using CodeBase.StaticData.Items.Shop.Items;
using CodeBase.StaticData.Items.Shop.Weapons;
using CodeBase.StaticData.Items.Shop.WeaponsUpgrades;
using CodeBase.StaticData.Weapons;
using CodeBase.UI.Windows.Finish.Items;
using CodeBase.UI.Windows.Shop;
using CodeBase.UI.Windows.Shop.Items;
using UnityEngine;

namespace CodeBase.UI.Windows.Common
{
    public abstract class ItemsGeneratorBase : MonoBehaviour
    {
        [SerializeField] private GameObject[] _shopItems;

        private const float DangerousHealthLevel = 0.5f;
        private const string Ammunition = "Ammo";
        private const string Weapons = "Weapons";
        private const string Perks = "Perks";
        private const string Upgrades = "Upgrades";
        private const string Items = "Items";
        private const string Money = "Money";

        private IStaticDataService _staticDataService;
        private IRandomService _randomService;
        private HashSet<int> _shopItemsNumbers;
        private List<AmmoItem> _unavailableAmmunition;
        private List<ItemTypeId> _unavailableItems;
        private List<UpgradeItemData> _unavailableUpgrades;
        private List<HeroWeaponTypeId> _unavailableWeapons;
        private List<PerkItemData> _unavailablePerks;
        private List<AmmoItem> _availableAmmunition;
        private List<ItemTypeId> _availableItems;
        private List<UpgradeItemData> _availableUpgrades;
        private List<HeroWeaponTypeId> _availableWeapons;
        private List<PerkItemData> _availablePerks;
        private List<MoneyTypeId> _availableMoney;
        private Coroutine _coroutineShopItemsGeneration;
        private WaitForSeconds _delayShopItemsDisplaying = new WaitForSeconds(0.5f);
        private PlayerProgress _progress;
        private HeroHealth _health;
        private int _money;

        public virtual event Action GenerationStarted;
        public virtual event Action GenerationEnded;

        public void Construct(IPlayerProgressService progressService, IStaticDataService staticDataService,
            IRandomService randomService, HeroHealth health)
        {
            _health = health;
            _progress = progressService.Progress;
            _staticDataService = staticDataService;
            _randomService = randomService;

            _money = _progress.CurrentLevelStats.MoneyData.Money;
        }

        private void InitializeEmptyData()
        {
            _shopItemsNumbers = new HashSet<int>(_shopItems.Length) { 0, 1, 2 };

            foreach (GameObject shopItem in _shopItems)
                shopItem.SetActive(false);

            _unavailableAmmunition = new List<AmmoItem>();
            _unavailableItems = new List<ItemTypeId>();
            _unavailableUpgrades = new List<UpgradeItemData>();
            _unavailableWeapons = new List<HeroWeaponTypeId>();
            _unavailablePerks = new List<PerkItemData>();
            _availableAmmunition = new List<AmmoItem>();
            _availableItems = new List<ItemTypeId>();
            _availableUpgrades = new List<UpgradeItemData>();
            _availableWeapons = new List<HeroWeaponTypeId>();
            _availablePerks = new List<PerkItemData>();
            _availableMoney = new List<MoneyTypeId>();
        }

        public void Generate()
        {
            GenerationStarted?.Invoke();
            SetHighlightingVisibility(false);
            InitializeEmptyData();

            CreateAllItems();

            // StartCoroutine(CoroutineShowShopItems());

            GenerateAllItems();

            // HideEmpty();

            SetHighlightingVisibility(true);
            GenerationEnded?.Invoke();
        }

        protected abstract void GenerateAllItems();
        protected abstract void CreateAllItems();

        protected void CreateNextLevelUpgrades()
        {
            WeaponData[] availableWeapons = _progress.WeaponsData.WeaponDatas.Where(data => data.IsAvailable).ToArray();
            HashSet<UpgradeItemData> upgradeItemDatas = _progress.WeaponsData.UpgradesData.UpgradeItemDatas;

            foreach (WeaponData weaponData in availableWeapons)
            {
                foreach (UpgradeItemData upgradeItemData in upgradeItemDatas)
                {
                    if (weaponData.WeaponTypeId == upgradeItemData.WeaponTypeId)
                    {
                        UpgradeItemData nextLevelUpgrade =
                            _progress.WeaponsData.UpgradesData.GetNextLevelUpgrade(weaponData.WeaponTypeId,
                                upgradeItemData.UpgradeTypeId);

                        if (nextLevelUpgrade.LevelTypeId != LevelTypeId.None)
                        {
                            UpgradeLevelInfoStaticData upgradeLevelInfoStaticData =
                                _staticDataService.ForUpgradeLevelsInfo(nextLevelUpgrade.UpgradeTypeId,
                                    nextLevelUpgrade.LevelTypeId);

                            if (_money >= upgradeLevelInfoStaticData.Cost)
                                _availableUpgrades.Add(nextLevelUpgrade);

                            _unavailableUpgrades.Add(nextLevelUpgrade);
                        }
                    }
                }
            }
        }

        protected void CreateNextLevelPerks()
        {
            foreach (PerkTypeId perkTypeId in DataExtensions.GetValues<PerkTypeId>())
            {
                PerkItemData nextLevelPerk = _progress.PerksData.GetNextLevelPerk(perkTypeId);

                if (nextLevelPerk.LevelTypeId != LevelTypeId.None)
                {
                    PerkStaticData perkStaticData =
                        _staticDataService.ForPerk(nextLevelPerk.PerkTypeId, nextLevelPerk.LevelTypeId);

                    if (_money >= perkStaticData.Cost)
                        _availablePerks.Add(new PerkItemData(nextLevelPerk.PerkTypeId, nextLevelPerk.LevelTypeId));

                    _unavailablePerks.Add(new PerkItemData(nextLevelPerk.PerkTypeId, nextLevelPerk.LevelTypeId));
                }
            }
        }

        protected void CreateAmmunition()
        {
            WeaponData[] availableWeapons = _progress.WeaponsData.WeaponDatas.Where(data => data.IsAvailable).ToArray();

            foreach (WeaponData weaponData in availableWeapons)
            {
                switch (weaponData.WeaponTypeId)
                {
                    case HeroWeaponTypeId.GrenadeLauncher:
                        _progress.WeaponsData.WeaponsAmmoData.Ammo.TryGetValue(weaponData.WeaponTypeId,
                            out int grenades);

                        switch (grenades)
                        {
                            case <= 3:
                                AddAmmo(weaponData.WeaponTypeId, AmmoCountType.Ten);
                                break;

                            case <= 8:
                                AddAmmo(weaponData.WeaponTypeId, AmmoCountType.Five);
                                break;

                            case > 8:
                                AddAmmo(weaponData.WeaponTypeId, AmmoCountType.One);
                                break;
                        }

                        break;

                    case HeroWeaponTypeId.RPG:
                        _progress.WeaponsData.WeaponsAmmoData.Ammo.TryGetValue(weaponData.WeaponTypeId,
                            out int rpgRockets);

                        switch (rpgRockets)
                        {
                            case <= 3:
                                AddAmmo(weaponData.WeaponTypeId, AmmoCountType.Ten);
                                break;

                            case <= 8:
                                AddAmmo(weaponData.WeaponTypeId, AmmoCountType.Five);
                                break;

                            case > 8:
                                AddAmmo(weaponData.WeaponTypeId, AmmoCountType.One);
                                break;
                        }

                        break;

                    case HeroWeaponTypeId.RocketLauncher:
                        _progress.WeaponsData.WeaponsAmmoData.Ammo.TryGetValue(weaponData.WeaponTypeId,
                            out int rocketLauncherRockets);

                        switch (rocketLauncherRockets)
                        {
                            case <= 3:
                                AddAmmo(weaponData.WeaponTypeId, AmmoCountType.Ten);
                                break;

                            case <= 8:
                                AddAmmo(weaponData.WeaponTypeId, AmmoCountType.Five);
                                break;

                            case > 8:
                                AddAmmo(weaponData.WeaponTypeId, AmmoCountType.One);
                                break;
                        }

                        break;

                    case HeroWeaponTypeId.Mortar:
                        _progress.WeaponsData.WeaponsAmmoData.Ammo.TryGetValue(weaponData.WeaponTypeId, out int bombs);

                        switch (bombs)
                        {
                            case <= 3:
                                AddAmmo(weaponData.WeaponTypeId, AmmoCountType.Five);
                                break;

                            case <= 8:
                                AddAmmo(weaponData.WeaponTypeId, AmmoCountType.One);
                                break;

                            case > 8:
                                AddAmmo(weaponData.WeaponTypeId, AmmoCountType.One);
                                break;
                        }

                        break;
                }
            }
        }

        protected void CreateWeapons()
        {
            WeaponData[] unavailableWeapons =
                _progress.WeaponsData.WeaponDatas.Where(data => data.IsAvailable == false).ToArray();

            foreach (WeaponData weaponData in unavailableWeapons)
            {
                ShopWeaponStaticData weaponStaticData = _staticDataService.ForShopWeapon(weaponData.WeaponTypeId);

                if (_money >= weaponStaticData.Cost)
                    _availableWeapons.Add(weaponData.WeaponTypeId);

                _unavailableWeapons.Add(weaponData.WeaponTypeId);
            }
        }

        protected void CreateItems()
        {
            foreach (ItemTypeId itemTypeId in DataExtensions.GetValues<ItemTypeId>())
            {
                ShopItemStaticData itemStaticData = _staticDataService.ForShopItem(itemTypeId);

                if (_money >= itemStaticData.Cost)
                    _availableItems.Add(itemTypeId);

                _unavailableItems.Add(itemTypeId);
            }
        }

        protected void CreateMoney()
        {
            foreach (MoneyTypeId moneyTypeId in DataExtensions.GetValues<MoneyTypeId>())
                _availableMoney.Add(moneyTypeId);
        }

        private void AddAmmo(HeroWeaponTypeId typeId, AmmoCountType ammoCount)
        {
            ShopAmmoStaticData shopAmmoStaticData = _staticDataService.ForShopAmmo(typeId, ammoCount);
            AmmoItem ammoItem = new AmmoItem(typeId, ammoCount);

            if (_money >= shopAmmoStaticData.Cost)
                _availableAmmunition.Add(ammoItem);

            _unavailableAmmunition.Add(ammoItem);
        }

        private IEnumerator CoroutineShowShopItems()
        {
            yield return ShowShopItems();
            yield return _delayShopItemsDisplaying;
            yield return ShowShopItems();
            yield return _delayShopItemsDisplaying;
        }

        private void SetHighlightingVisibility(bool isVisible)
        {
            foreach (GameObject shopItem in _shopItems)
                shopItem.GetComponent<ShopItemHighlighter>().enabled = isVisible;
        }

        private IEnumerator ShowShopItems()
        {
            ShowItems();
            yield return null;
        }

        private void ShowItems()
        {
            foreach (GameObject shopItem in _shopItems)
            {
                List<string> lists = new List<string>();
                lists.Add(Ammunition);
                lists.Add(Weapons);
                lists.Add(Perks);
                lists.Add(Upgrades);
                lists.Add(Items);
                lists.Add(Money);

                string nextFrom = _randomService.NextFrom(lists);

                switch (nextFrom)
                {
                    case Ammunition:
                    {
                        if (_unavailableAmmunition.Count != 0)
                            CreateAmmoItem(shopItem, _unavailableAmmunition, false);

                        break;
                    }

                    case Weapons:
                    {
                        if (_unavailableWeapons.Count != 0)
                            CreateWeaponItem(shopItem, _unavailableWeapons, false);

                        break;
                    }

                    case Perks:
                    {
                        if (_unavailablePerks.Count != 0)
                            CreatePerkItem(shopItem, _unavailablePerks, false);

                        break;
                    }

                    case Upgrades:
                    {
                        if (_unavailableUpgrades.Count != 0)
                            CreateUpgradeItem(shopItem, _unavailableUpgrades, false);

                        break;
                    }

                    case Items:
                    {
                        if (_unavailableItems.Count != 0)
                            CreateItemItem(shopItem, _unavailableItems, false);

                        break;
                    }

                    case Money:
                    {
                        CreateMoneyItem(shopItem, _availableMoney, false);
                        break;
                    }
                }
            }
        }

        private void CreateAmmoItem(GameObject parent, List<AmmoItem> list,
            bool isClickable)
        {
            AmmoItem ammoItem = _randomService.NextFrom(list);
            AmmoShopItem view = parent.GetComponent<ShopCell>().GetView(typeof(AmmoShopItem)) as AmmoShopItem;
            view?.Construct(ammoItem, _progress);
            view?.ChangeClickability(isClickable);
            parent.SetActive(true);
        }

        private void CreateItemItem(GameObject parent, List<ItemTypeId> list, bool isClickable)
        {
            ItemTypeId itemTypeId = _randomService.NextFrom(list);
            CreateItemItem(parent, itemTypeId, isClickable);
        }

        private void CreateItemItem(GameObject parent, ItemTypeId itemTypeId, bool isClickable)
        {
            ItemShopItem view =
                parent.GetComponent<ShopCell>().GetView(typeof(ItemShopItem)) as ItemShopItem;
            view?.Construct(itemTypeId, _progress, _health);
            view?.ChangeClickability(isClickable);
            parent.SetActive(true);
        }

        private void CreateUpgradeItem(GameObject parent, List<UpgradeItemData> list, bool isClickable)
        {
            UpgradeItemData upgradeItemData = _randomService.NextFrom(list);
            UpgradeShopItem view = parent.GetComponent<ShopCell>().GetView(typeof(UpgradeShopItem)) as UpgradeShopItem;
            view?.Construct(upgradeItemData, _progress);
            view?.ChangeClickability(isClickable);
            parent.SetActive(true);
        }

        private void CreatePerkItem(GameObject parent, List<PerkItemData> list, bool isClickable)
        {
            PerkItemData perkItemData = _randomService.NextFrom(list);
            PerkShopItem view = parent.GetComponent<ShopCell>().GetView(typeof(PerkShopItem)) as PerkShopItem;
            view?.Construct(perkItemData, _progress);
            view?.ChangeClickability(isClickable);
            parent.SetActive(true);
        }

        private void CreateWeaponItem(GameObject parent, List<HeroWeaponTypeId> list, bool isClickable)
        {
            HeroWeaponTypeId weaponTypeId = _randomService.NextFrom(list);
            WeaponShopItem view = parent.GetComponent<ShopCell>().GetView(typeof(WeaponShopItem)) as WeaponShopItem;
            view?.Construct(weaponTypeId, _progress);
            view?.ChangeClickability(isClickable);
            parent.SetActive(true);
        }

        private void CreateMoneyItem(GameObject parent, List<MoneyTypeId> list, bool isClickable)
        {
            MoneyTypeId moneyTypeId = _randomService.NextFrom(list);
            MoneyGiftItem view = parent.GetComponent<ShopCell>().GetView(typeof(MoneyGiftItem)) as MoneyGiftItem;
            view?.Construct(moneyTypeId, _progress);
            view?.ChangeClickability(isClickable);
            parent.SetActive(true);
        }

        protected void GenerateItems()
        {
            if (_availableItems.Count != 0 && _shopItemsNumbers.Count != 0)
            {
                float healthPercentage = _progress.HealthState.CurrentHp / _progress.HealthState.BaseMaxHp;

                ShopItemStaticData shopItemStaticData = _staticDataService.ForShopItem(ItemTypeId.HealthRecover);

                if (healthPercentage <= DangerousHealthLevel && _availableItems.Contains(ItemTypeId.HealthRecover) &&
                    _money >= shopItemStaticData.Cost)
                {
                    GameObject view = GetRandomShopItem();

                    if (view != null)
                        CreateItemItem(view, ItemTypeId.HealthRecover, true);
                }
            }
        }

        protected void GenerateAmmo()
        {
            if (_availableAmmunition.Count != 0 && _shopItemsNumbers.Count != 0)
            {
                GameObject view = GetRandomShopItem();

                if (view != null)
                    CreateAmmoItem(view, _availableAmmunition, true);
            }
        }

        protected void GenerateUpgrades()
        {
            if (_availableUpgrades.Count != 0 && _shopItemsNumbers.Count != 0)
            {
                GameObject view = GetRandomShopItem();

                if (view != null)
                    CreateUpgradeItem(view, _availableUpgrades, true);
            }
        }

        protected void GenerateWeapons()
        {
            if (_availableWeapons.Count != 0 && _shopItemsNumbers.Count != 0)
            {
                GameObject view = GetRandomShopItem();

                if (view != null)
                    CreateWeaponItem(view, _availableWeapons, true);
            }
        }

        protected void GeneratePerks()
        {
            if (_availablePerks.Count != 0 && _shopItemsNumbers.Count != 0)
            {
                GameObject view = GetRandomShopItem();

                if (view != null)
                    CreatePerkItem(view, _availablePerks, true);
            }
        }

        protected void GenerateMoney()
        {
            if (_availablePerks.Count != 0 && _shopItemsNumbers.Count != 0)
            {
                GameObject view = GetRandomShopItem();

                if (view != null)
                    CreatePerkItem(view, _availablePerks, true);
            }
        }

        private GameObject GetRandomShopItem()
        {
            int i = _randomService.NextNumberFrom(_shopItemsNumbers);
            return _shopItems[i];
        }
    }
}