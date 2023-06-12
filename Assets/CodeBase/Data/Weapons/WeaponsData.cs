using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Data.Upgrades;
using CodeBase.StaticData.Weapons;

namespace CodeBase.Data.Weapons
{
    [Serializable]
    public class WeaponsData
    {
        private const int InitialRpgAmmoCount = 8;
        private const int InitialRlAmmoCount = 12;
        private const int InitialMortarAmmoCount = 6;

        private List<HeroWeaponTypeId> _typeIds = DataExtensions.GetValues<HeroWeaponTypeId>().ToList();
        public List<WeaponData> WeaponDatas;
        public WeaponsAmmoData WeaponsAmmoData;
        public UpgradesData UpgradesData;
        public HeroWeaponTypeId CurrentHeroWeaponTypeId;

        public event Action<HeroWeaponTypeId> SetAvailable;

        public WeaponsData(Scene scene)
        {
            WeaponDatas = new List<WeaponData>(_typeIds.Count);
            WeaponsAmmoData = new WeaponsAmmoData(WeaponDatas, scene);
            UpgradesData = new UpgradesData();
            FillAvailableWeapons();
            CurrentHeroWeaponTypeId = WeaponDatas.First(x => x.IsAvailable).WeaponTypeId;
        }

        private void FillAvailableWeapons()
        {
            WeaponDatas.Add(new WeaponData(HeroWeaponTypeId.GrenadeLauncher, true));
            WeaponDatas.Add(new WeaponData(HeroWeaponTypeId.RPG, false));
            // WeaponDatas.Add(new WeaponData(HeroWeaponTypeId.RPG, true));
            WeaponDatas.Add(new WeaponData(HeroWeaponTypeId.RocketLauncher, false));
            // WeaponDatas.Add(new WeaponData(HeroWeaponTypeId.RocketLauncher, true));
            WeaponDatas.Add(new WeaponData(HeroWeaponTypeId.Mortar, false));
            // WeaponDatas.Add(new WeaponData(HeroWeaponTypeId.Mortar, true));

            SetAvailableWeapons();
        }

        private void SetAvailableWeapons()
        {
            foreach (WeaponData weaponData in WeaponDatas)
                if (weaponData.IsAvailable)
                    SetAvailable?.Invoke(weaponData.WeaponTypeId);
        }

        public void SetCurrentWeapon(HeroWeaponTypeId typeId)
        {
            CurrentHeroWeaponTypeId = typeId;
            WeaponsAmmoData.SetCurrentWeapon(typeId);
        }

        public void SetAvailableWeapon(HeroWeaponTypeId typeId)
        {
            WeaponDatas.First(x => x.WeaponTypeId == typeId).SetWeaponAvailable();

            switch (typeId)
            {
                case HeroWeaponTypeId.RPG:
                    WeaponsAmmoData.AddAmmo(HeroWeaponTypeId.RPG, InitialRpgAmmoCount);
                    break;
                case HeroWeaponTypeId.RocketLauncher:
                    WeaponsAmmoData.AddAmmo(HeroWeaponTypeId.RocketLauncher, InitialRlAmmoCount);
                    break;
                case HeroWeaponTypeId.Mortar:
                    WeaponsAmmoData.AddAmmo(HeroWeaponTypeId.Mortar, InitialMortarAmmoCount);
                    break;
            }

            SetAvailable?.Invoke(typeId);
        }
    }
}