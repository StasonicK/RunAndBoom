using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.StaticData.Weapons;

namespace CodeBase.Data.Weapons
{
    [Serializable]
    public class WeaponsAmmoData : ItemData
    {
        private const int InitialGlAmmoCount = 27;

        private HeroWeaponTypeId _currentHeroWeaponTypeId;
        private List<WeaponData> _weaponDatas;
        public AmunitionDataDictionary Amunition = new();
        public BarrelDataDictionary Barrels = new();

        public event Action<int> GrenadeLauncherAmmoChanged;
        public event Action<int> RpgAmmoChanged;
        public event Action<int> RocketLauncherAmmoChanged;
        public event Action<int> MortarAmmoChanged;

        public WeaponsAmmoData(List<WeaponData> weaponDatas, Scene scene)
        {
            _weaponDatas = weaponDatas;

            if (scene == Scene.Level_1)
                FillAmmo();

            InvokeChanges();
            FillWeaponsBarrels();
        }

        private void InvokeChanges()
        {
            AmmoChanged(HeroWeaponTypeId.GrenadeLauncher);
            AmmoChanged(HeroWeaponTypeId.RPG);
            AmmoChanged(HeroWeaponTypeId.RocketLauncher);
            AmmoChanged(HeroWeaponTypeId.Mortar);
        }

        public void SetCurrentWeapon(HeroWeaponTypeId typeId) =>
            _currentHeroWeaponTypeId = typeId;

        private void FillWeaponsBarrels()
        {
            Barrels.Dictionary[HeroWeaponTypeId.GrenadeLauncher] = 1;
            Barrels.Dictionary[HeroWeaponTypeId.RPG] = 1;
            Barrels.Dictionary[HeroWeaponTypeId.RocketLauncher] = 3;
            Barrels.Dictionary[HeroWeaponTypeId.Mortar] = 1;
        }

        private void FillAmmo()
        {
            Amunition.Dictionary[HeroWeaponTypeId.GrenadeLauncher] = InitialGlAmmoCount;
            // Amunition.Dictionary[HeroWeaponTypeId.RPG] = InitialGlAmmoCount;
            // Amunition.Dictionary[HeroWeaponTypeId.RocketLauncher] = InitialGlAmmoCount;
            // Amunition.Dictionary[HeroWeaponTypeId.Mortar] = InitialGlAmmoCount;
            Amunition.Dictionary[HeroWeaponTypeId.RPG] = 0;
            Amunition.Dictionary[HeroWeaponTypeId.RocketLauncher] = 0;
            Amunition.Dictionary[HeroWeaponTypeId.Mortar] = 0;
        }

        public void AddAmmo(HeroWeaponTypeId typeId, int ammo)
        {
            int current = Amunition.Dictionary[typeId];
            int result = current + ammo;
            Amunition.Dictionary[typeId] = result;
            AmmoChanged(typeId);
        }

        public bool IsAmmoAvailable() =>
            Barrels.Dictionary[_currentHeroWeaponTypeId] <= Amunition.Dictionary[_currentHeroWeaponTypeId];

        public void ReduceAmmo()
        {
            Amunition.Dictionary[_currentHeroWeaponTypeId] -= Barrels.Dictionary[_currentHeroWeaponTypeId];
            AmmoChanged(_currentHeroWeaponTypeId);
        }

        public void TryChangeAmmo(HeroWeaponTypeId typeId)
        {
            if (_weaponDatas.First(x => x.WeaponTypeId == typeId).IsAvailable)
                AmmoChanged(typeId);
        }

        public void AmmoChanged(HeroWeaponTypeId typeId)
        {
            switch (typeId)
            {
                case HeroWeaponTypeId.GrenadeLauncher:
                    GrenadeLauncherAmmoChanged?.Invoke(Amunition.Dictionary[typeId]);
                    break;
                case HeroWeaponTypeId.RPG:
                    RpgAmmoChanged?.Invoke(Amunition.Dictionary[typeId]);
                    break;
                case HeroWeaponTypeId.RocketLauncher:
                    RocketLauncherAmmoChanged?.Invoke(Amunition.Dictionary[typeId]);
                    break;
                case HeroWeaponTypeId.Mortar:
                    MortarAmmoChanged?.Invoke(Amunition.Dictionary[typeId]);
                    break;
            }
        }
    }
}