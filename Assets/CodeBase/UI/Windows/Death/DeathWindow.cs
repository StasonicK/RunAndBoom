using CodeBase.Data;
using CodeBase.Hero;
using CodeBase.StaticData.Weapons;
using CodeBase.UI.Elements.Hud;
using CodeBase.UI.Elements.Hud.MobileInputPanel;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Death
{
    public class DeathWindow : WindowBase
    {
        [SerializeField] private Button _recoverForAdsButton;
        [SerializeField] private Button _restartButton;

        private const int MIN_GRENADE_LAUNCHER_AMMO_COUNT = 4;
        private const int MIN_RPG_AMMO_COUNT = 2;
        private const int MIN_ROCKET_LAUNCHER_AMMO_COUNT = 3;
        private const int MIN_MORTAR_AMMO_COUNT = 1;

        private void OnEnable()
        {
            if (!Application.isEditor)
                _restartButton.enabled = false;

            _recoverForAdsButton.onClick.AddListener(ShowAds);
            _restartButton.onClick.AddListener(RestartLevel);

            if (Application.isEditor)
                return;

            if (_adsService == null)
                return;

            _adsService.OnInitializeSuccess += AdsServiceInitializedSuccess;
            _adsService.OnClosedVideoAd += RecoverForAds;
            _adsService.OnShowVideoAdError += ShowError;
            InitializeAdsSDK();
        }

        private void OnDisable()
        {
            _recoverForAdsButton.onClick.RemoveListener(ShowAds);
            _restartButton.onClick.RemoveListener(RestartLevel);

            if (_adsService == null)
                return;

            _adsService.OnInitializeSuccess -= AdsServiceInitializedSuccess;
            _adsService.OnClosedVideoAd -= RecoverForAds;
            _adsService.OnShowVideoAdError -= ShowError;
        }

        public void Construct(GameObject hero, OpenSettings openSettings, MobileInput mobileInput) =>
            base.Construct(hero, WindowId.Death, openSettings, mobileInput);

        protected override void AdsServiceInitializedSuccess()
        {
            base.AdsServiceInitializedSuccess();
            _restartButton.enabled = true;
        }

        private void ShowAds()
        {
            if (Application.isEditor)
            {
                Recover();
            }
            else
            {
                _adsService.ShowVideoAd();
                SoundInstance.StopRandomMusic(false);
            }
        }

        private void ShowError(string message) =>
            Debug.Log($"OnErrorFullScreen: {message}");

        private void Recover()
        {
            RecoverHealth();
            TryAddAmmo();
            Close();
        }

        private void RecoverForAds()
        {
            Recover();
            SoundInstance.StartRandomMusic();
        }

        private void RecoverHealth() =>
            _hero.GetComponent<HeroHealth>().Recover();

        private void TryAddAmmo()
        {
            int additionalAmmo;

            if (ProgressData.WeaponsData.WeaponsAmmoData.Amunition.Dictionary[HeroWeaponTypeId.GrenadeLauncher] <
                _inputService.GetCount(MIN_GRENADE_LAUNCHER_AMMO_COUNT))
            {
                additionalAmmo = _inputService.GetCount(MIN_GRENADE_LAUNCHER_AMMO_COUNT) -
                                 ProgressData.WeaponsData.WeaponsAmmoData.Amunition.Dictionary[
                                     HeroWeaponTypeId.GrenadeLauncher];
                ProgressData.WeaponsData.WeaponsAmmoData.AddAmmo(HeroWeaponTypeId.GrenadeLauncher, additionalAmmo);
            }

            if (ProgressData.WeaponsData.WeaponsAmmoData.Amunition.Dictionary[HeroWeaponTypeId.RPG] <
                _inputService.GetCount(MIN_RPG_AMMO_COUNT))
            {
                additionalAmmo = _inputService.GetCount(MIN_RPG_AMMO_COUNT) -
                                 ProgressData.WeaponsData.WeaponsAmmoData.Amunition.Dictionary[
                                     HeroWeaponTypeId.RPG];
                ProgressData.WeaponsData.WeaponsAmmoData.AddAmmo(HeroWeaponTypeId.RPG, additionalAmmo);
            }

            if (ProgressData.WeaponsData.WeaponsAmmoData.Amunition.Dictionary[HeroWeaponTypeId.RocketLauncher] <
                _inputService.GetCount(MIN_ROCKET_LAUNCHER_AMMO_COUNT))
            {
                additionalAmmo = _inputService.GetCount(MIN_ROCKET_LAUNCHER_AMMO_COUNT) -
                                 ProgressData.WeaponsData.WeaponsAmmoData.Amunition.Dictionary[
                                     HeroWeaponTypeId.RocketLauncher];
                ProgressData.WeaponsData.WeaponsAmmoData.AddAmmo(HeroWeaponTypeId.RocketLauncher, additionalAmmo);
            }

            if (ProgressData.WeaponsData.WeaponsAmmoData.Amunition.Dictionary[HeroWeaponTypeId.Mortar] <
                _inputService.GetCount(MIN_MORTAR_AMMO_COUNT))
            {
                additionalAmmo = _inputService.GetCount(MIN_MORTAR_AMMO_COUNT) -
                                 ProgressData.WeaponsData.WeaponsAmmoData.Amunition.Dictionary[
                                     HeroWeaponTypeId.Mortar];
                ProgressData.WeaponsData.WeaponsAmmoData.AddAmmo(HeroWeaponTypeId.Mortar, additionalAmmo);
            }
        }
    }
}