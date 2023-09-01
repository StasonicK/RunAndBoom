using System;
using CodeBase.UI.Windows.Common;
using CodeBase.UI.Windows.Gifts.Items;
using CodeBase.UI.Windows.Shop.Items;
using NTC.Global.Cache;
using NTC.Global.System;
using UnityEngine;

namespace CodeBase.UI.Windows.Shop
{
    public class ShopCell : MonoCache
    {
        [SerializeField] private AmmoItemBase _ammoItemBase;
        [SerializeField] private ItemItemBase _itemItemBase;
        [SerializeField] private PerkItemBase _perkItemBase;
        [SerializeField] private UpgradeItemBase _upgradeItemBase;
        [SerializeField] private WeaponItemBase _weaponItemBase;
        [SerializeField] private MoneyItemBase _moneyItemBase;

        Type _ammoShopType = typeof(AmmoShopItem);
        Type _itemShopType = typeof(ItemShopItem);
        Type _perkShopType = typeof(PerkShopItem);
        Type _upgradeShopType = typeof(UpgradeShopItem);
        Type _weaponShopType = typeof(WeaponShopItem);
        Type _ammoGiftType = typeof(AmmoGiftItem);
        Type _itemGiftType = typeof(ItemGiftItem);
        Type _perkGiftType = typeof(PerkGiftItem);
        Type _upgradeGiftType = typeof(UpgradeGiftItem);
        Type _weaponGiftType = typeof(WeaponGiftItem);
        Type _moneyGiftType = typeof(MoneyGiftItem);

        public ItemBase GetView(Type type)
        {
            if (type == _ammoShopType || type == _ammoGiftType)
            {
                _ammoItemBase.gameObject.Enable();
                _itemItemBase.gameObject.Disable();
                _upgradeItemBase.gameObject.Disable();
                _perkItemBase.gameObject.Disable();
                _weaponItemBase.gameObject.Disable();
                _moneyItemBase?.gameObject.Disable();
                return _ammoItemBase;
            }

            if (type == _itemShopType || type == _itemGiftType)
            {
                _itemItemBase.gameObject.Enable();
                _ammoItemBase.gameObject.Disable();
                _upgradeItemBase.gameObject.Disable();
                _perkItemBase.gameObject.Disable();
                _weaponItemBase.gameObject.Disable();
                _moneyItemBase?.gameObject.Disable();
                return _itemItemBase;
            }

            if (type == _upgradeShopType || type == _upgradeGiftType)
            {
                _upgradeItemBase.gameObject.Enable();
                _ammoItemBase.gameObject.Disable();
                _itemItemBase.gameObject.Disable();
                _perkItemBase.gameObject.Disable();
                _weaponItemBase.gameObject.Disable();
                _moneyItemBase?.gameObject.Disable();
                return _upgradeItemBase;
            }

            if (type == _perkShopType || type == _perkGiftType)
            {
                _perkItemBase.gameObject.Enable();
                _ammoItemBase.gameObject.Disable();
                _itemItemBase.gameObject.Disable();
                _upgradeItemBase.gameObject.Disable();
                _weaponItemBase.gameObject.Disable();
                _moneyItemBase?.gameObject.Disable();
                return _perkItemBase;
            }

            if (type == _weaponShopType || type == _weaponGiftType)
            {
                _weaponItemBase.gameObject.Enable();
                _ammoItemBase.gameObject.Disable();
                _itemItemBase.gameObject.Disable();
                _upgradeItemBase.gameObject.Disable();
                _perkItemBase.gameObject.Disable();
                _moneyItemBase?.gameObject.Disable();
                return _weaponItemBase;
            }

            if (type == _moneyGiftType)
            {
                _moneyItemBase.gameObject.Enable();
                _weaponItemBase.gameObject.Disable();
                _ammoItemBase.gameObject.Disable();
                _itemItemBase.gameObject.Disable();
                _upgradeItemBase.gameObject.Disable();
                _perkItemBase.gameObject.Disable();
                return _moneyItemBase;
            }
            else return _weaponItemBase;
        }
    }
}