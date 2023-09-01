using CodeBase.Data;
using CodeBase.Services;
using CodeBase.Services.Localization;
using CodeBase.Services.PersistentProgress;
using CodeBase.StaticData.Weapons;
using NTC.Global.Cache;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Elements.Hud.WeaponsPanel
{
    public class WeaponsCounter : MonoCache, IProgressReader
    {
        [SerializeField] private TextMeshProUGUI _grenadeLaucher;
        [SerializeField] private TextMeshProUGUI _rpg;
        [SerializeField] private TextMeshProUGUI _rocketLaucher;
        [SerializeField] private TextMeshProUGUI _mortar;

        private ILocalizationService _localizationService;
        private PlayerProgress _progress;

        protected override void OnEnabled()
        {
            if (_progress != null)
            {
                _progress.WeaponsData.WeaponsAmmoData.GrenadeLauncherAmmoChanged +=
                    ChangeGrenadeLauncherAmmo;
                _progress.WeaponsData.WeaponsAmmoData.RpgAmmoChanged += ChangeRpgAmmo;
                _progress.WeaponsData.WeaponsAmmoData.RocketLauncherAmmoChanged +=
                    ChangeRocketLauncherAmmo;
                _progress.WeaponsData.WeaponsAmmoData.MortarAmmoChanged += ChangeMortarAmmo;
            }

            if (_localizationService == null)
                _localizationService = AllServices.Container.Single<ILocalizationService>();

            _localizationService.LanguageChanged += RefreshAmmo;
        }

        protected override void OnDisabled()
        {
            if (_progress?.WeaponsData != null)
            {
                _progress.WeaponsData.WeaponsAmmoData.GrenadeLauncherAmmoChanged -=
                    ChangeGrenadeLauncherAmmo;
                _progress.WeaponsData.WeaponsAmmoData.RpgAmmoChanged -= ChangeRpgAmmo;
                _progress.WeaponsData.WeaponsAmmoData.RocketLauncherAmmoChanged -=
                    ChangeRocketLauncherAmmo;
                _progress.WeaponsData.WeaponsAmmoData.MortarAmmoChanged -= ChangeMortarAmmo;
            }

            if (_localizationService != null)
                _localizationService.LanguageChanged -= RefreshAmmo;
        }

        private void Construct()
        {
            _progress.WeaponsData.WeaponsAmmoData.GrenadeLauncherAmmoChanged +=
                ChangeGrenadeLauncherAmmo;
            _progress.WeaponsData.WeaponsAmmoData.RpgAmmoChanged += ChangeRpgAmmo;
            _progress.WeaponsData.WeaponsAmmoData.RocketLauncherAmmoChanged += ChangeRocketLauncherAmmo;
            _progress.WeaponsData.WeaponsAmmoData.MortarAmmoChanged += ChangeMortarAmmo;
            _progress.WeaponsData.WeaponsAmmoData.AmmoChanged(HeroWeaponTypeId.GrenadeLauncher);
            _progress.WeaponsData.WeaponsAmmoData.AmmoChanged(HeroWeaponTypeId.RPG);
            _progress.WeaponsData.WeaponsAmmoData.AmmoChanged(HeroWeaponTypeId.RocketLauncher);
            _progress.WeaponsData.WeaponsAmmoData.AmmoChanged(HeroWeaponTypeId.Mortar);
        }

        private void ChangeGrenadeLauncherAmmo(int ammo) =>
            _grenadeLaucher.text = $"{ammo}";

        private void ChangeRpgAmmo(int ammo) =>
            _rpg.text = $"{ammo}";

        private void ChangeRocketLauncherAmmo(int ammo) =>
            _rocketLaucher.text = $"{ammo}";

        private void ChangeMortarAmmo(int ammo) =>
            _mortar.text = $"{ammo}";

        private void RefreshAmmo()
        {
            ChangeGrenadeLauncherAmmo(
                _progress.WeaponsData.WeaponsAmmoData.Amunition.Dictionary[HeroWeaponTypeId.GrenadeLauncher]);
            ChangeRpgAmmo(_progress.WeaponsData.WeaponsAmmoData.Amunition.Dictionary[HeroWeaponTypeId.RPG]);
            ChangeRocketLauncherAmmo(
                _progress.WeaponsData.WeaponsAmmoData.Amunition.Dictionary[HeroWeaponTypeId.RocketLauncher]);
            ChangeMortarAmmo(_progress.WeaponsData.WeaponsAmmoData.Amunition.Dictionary[HeroWeaponTypeId.Mortar]);
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _progress = progress;
            Construct();
        }
    }
}