﻿using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.StaticData.Weapons;

namespace CodeBase.Data.Progress.Weapons
{
    [Serializable]
    public class WeaponsAmmoData : ItemData
    {
        // private const int InitialGlAmmoCount = 27;
        private const int InitialGlAmmoCount = 12;
        private const int InitialRpgAmmoCount = 6;
        private const int InitialRlAmmoCount = 9;
        private const int InitialMortarAmmoCount = 3;

        private HeroWeaponTypeId _currentHeroWeaponTypeId;
        private List<WeaponData> _weaponDatas;
        public AmunitionDataDictionary Amunition = new();
        public BarrelDataDictionary Barrels = new();

        public event Action<int> GrenadeLauncherAmmoChanged;
        public event Action<int> RpgAmmoChanged;
        public event Action<int> RocketLauncherAmmoChanged;
        public event Action<int> MortarAmmoChanged;

        public WeaponsAmmoData(List<WeaponData> weaponDatas, SceneId sceneId, bool isHardMode)
        {
            _weaponDatas = weaponDatas;

            if (sceneId == SceneId.Level_1)
                FillAmmo(isHardMode);

            InvokeChanges();
            FillWeaponsBarrels();
        }

        private void FillAmmo(bool isHardMode)
        {
            Amunition.Dictionary[HeroWeaponTypeId.GrenadeLauncher] = InitialGlAmmoCount;

            if (!isHardMode)
                Amunition.Dictionary[HeroWeaponTypeId.RPG] = InitialRpgAmmoCount;
            else
                Amunition.Dictionary[HeroWeaponTypeId.RPG] = 0;

            if (!isHardMode)
                Amunition.Dictionary[HeroWeaponTypeId.RocketLauncher] = InitialRlAmmoCount;
            else
                Amunition.Dictionary[HeroWeaponTypeId.RocketLauncher] = 0;

            if (!isHardMode)
                Amunition.Dictionary[HeroWeaponTypeId.Mortar] = InitialMortarAmmoCount;
            else
                Amunition.Dictionary[HeroWeaponTypeId.Mortar] = 0;
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