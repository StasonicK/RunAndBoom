using CodeBase.Data;
using CodeBase.Data.Weapons;
using CodeBase.Services.PersistentProgress;
using CodeBase.StaticData.Weapons;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.UI.Elements.Hud.WeaponsPanel
{
    public class WeaponsVisibility : MonoCache, IProgressReader
    {
        [SerializeField] private GameObject _grenadeLauncher;
        [SerializeField] private GameObject _rpg;
        [SerializeField] private GameObject _rocketLauncher;
        [SerializeField] private GameObject _mortar;

        private PlayerProgress _progress;

        private void ShowAvailable()
        {
            foreach (WeaponData weaponsData in _progress.WeaponsData.WeaponData)
                if (weaponsData.IsAvailable)
                    SetVisibility(weaponsData.WeaponTypeId, true);
                else
                    SetVisibility(weaponsData.WeaponTypeId, false);
        }

        private void SetVisibility(HeroWeaponTypeId typeId, bool isVisible)
        {
            switch (typeId)
            {
                case HeroWeaponTypeId.GrenadeLauncher:
                    if (_grenadeLauncher.activeInHierarchy != isVisible)
                        _grenadeLauncher.SetActive(isVisible);
                    break;
                case HeroWeaponTypeId.RPG:
                    if (_rpg.activeInHierarchy != isVisible)
                        _rpg.SetActive(isVisible);
                    break;
                case HeroWeaponTypeId.RocketLauncher:
                    if (_rocketLauncher.activeInHierarchy != isVisible)
                        _rocketLauncher.SetActive(isVisible);
                    break;
                case HeroWeaponTypeId.Mortar:
                    if (_mortar.activeInHierarchy != isVisible)
                        _mortar.SetActive(isVisible);
                    break;
            }
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _progress = progress;
            _progress.WeaponsData.SetAvailable += Show;
            ShowAvailable();
        }

        private void Show(HeroWeaponTypeId typeId) =>
            SetVisibility(typeId, true);
    }
}