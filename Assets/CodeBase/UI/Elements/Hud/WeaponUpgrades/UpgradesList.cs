using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Data;
using CodeBase.Data.Upgrades;
using CodeBase.Services.PersistentProgress;
using CodeBase.StaticData.Items;
using CodeBase.StaticData.Items.Shop.WeaponsUpgrades;
using CodeBase.StaticData.Weapons;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.UI.Elements.Hud.WeaponUpgrades
{
    public class UpgradesList : MonoCache, IProgressReader
    {
        [SerializeField] private Transform _container;
        [SerializeField] private UpgradeView perkView;
        [SerializeField] private HeroWeaponTypeId _weaponTypeId;

        private IEnumerable<UpgradeTypeId>
            _upgradeTypeIds = Enum.GetValues(typeof(UpgradeTypeId)).Cast<UpgradeTypeId>();

        private Dictionary<UpgradeTypeId, UpgradeView> _activeUpgrades;
        private PlayerProgress _progress;

        private void Awake() =>
            _activeUpgrades = new Dictionary<UpgradeTypeId, UpgradeView>(_upgradeTypeIds.Count());

        public void LoadProgress(PlayerProgress progress)
        {
            _progress = progress;
            _progress.WeaponsData.UpgradesData.NewUpgradeAdded += AddNewUpgrade;

            ConstructUpgrades();
        }

        private void AddNewUpgrade(HeroWeaponTypeId heroWeaponTypeId, UpgradeItemData upgrade)
        {
            if (heroWeaponTypeId != _weaponTypeId)
                return;

            UpgradeView value = Instantiate(perkView, _container);
            value.Construct(upgrade);
            _activeUpgrades.Add(upgrade.UpgradeTypeId, value);
        }

        private void ConstructUpgrades()
        {
            foreach (UpgradeItemData upgrade in _progress.WeaponsData.UpgradesData.UpgradeItemDatas)
            {
                if (upgrade.WeaponTypeId == _weaponTypeId)
                    if (upgrade.LevelTypeId != LevelTypeId.None)
                        AddNewUpgrade(_weaponTypeId, upgrade);
            }
        }
    }
}