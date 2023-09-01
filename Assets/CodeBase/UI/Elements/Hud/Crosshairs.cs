using CodeBase.Hero;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.Weapons;
using NTC.Global.Cache;
using NTC.Global.System;
using UnityEngine;

namespace CodeBase.UI.Elements.Hud
{
    public class Crosshairs : MonoCache
    {
        [SerializeField] private GameObject _grenadeLauncher;
        [SerializeField] private GameObject _rpg;
        [SerializeField] private GameObject _rocketLauncher;
        [SerializeField] private GameObject _mortar;

        private HeroWeaponSelection _heroWeaponSelection;
        private HeroWeaponTypeId _heroWeaponTypeId;
        private HeroReloading _heroReloading;

        public void Construct(HeroReloading heroShooting, HeroWeaponSelection heroWeaponSelection)
        {
            _heroReloading = heroShooting;
            _heroWeaponSelection = heroWeaponSelection;

            _heroReloading.OnStartReloading += Hide;
            _heroReloading.OnStopReloading += Show;
            _heroWeaponSelection.WeaponSelected += ChangeCrosshair;
        }

        private void ChangeCrosshair(GameObject arg1, HeroWeaponStaticData weaponStaticData, TrailStaticData arg3)
        {
            _heroWeaponTypeId = weaponStaticData.WeaponTypeId;
            Show();
        }

        private void Hide(float f)
        {
            _grenadeLauncher.Disable();
            _rpg.Disable();
            _rocketLauncher.Disable();
            _mortar.Disable();
        }

        private void Show()
        {
            switch (_heroWeaponTypeId)
            {
                case HeroWeaponTypeId.GrenadeLauncher:
                    _grenadeLauncher.Enable();
                    _rpg.Disable();
                    _rocketLauncher.Disable();
                    _mortar.Disable();
                    break;

                case HeroWeaponTypeId.RPG:
                    _rpg.Enable();
                    _grenadeLauncher.Disable();
                    _rocketLauncher.Disable();
                    _mortar.Disable();
                    break;

                case HeroWeaponTypeId.RocketLauncher:
                    _rocketLauncher.Enable();
                    _grenadeLauncher.Disable();
                    _rpg.Disable();
                    _mortar.Disable();
                    break;

                case HeroWeaponTypeId.Mortar:
                    _mortar.Enable();
                    _rocketLauncher.Disable();
                    _grenadeLauncher.Disable();
                    _rpg.Disable();
                    break;
            }
        }
    }
}