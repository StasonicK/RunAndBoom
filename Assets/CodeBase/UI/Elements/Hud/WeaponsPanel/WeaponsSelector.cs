using CodeBase.Hero;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.Weapons;
using NTC.Global.Cache;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Elements.Hud.WeaponsPanel
{
    public class WeaponsSelector : MonoCache
    {
        [SerializeField] private GameObject _grenadeLauncher;
        [SerializeField] private GameObject _rpg;
        [SerializeField] private GameObject _rocketLauncher;
        [SerializeField] private GameObject _mortar;
        [SerializeField] private Sprite _selected;
        [SerializeField] private Sprite _unselected;

        private HeroWeaponSelection _heroWeaponSelection;
        private Image _grenadeLauncherImage;
        private Image _rpgImage;
        private Image _rocketLauncherImage;
        private Image _mortarImage;
        private Button _grenadeLauncherButton;
        private Button _rpgButton;
        private Button _rocketLauncherButton;
        private Button _mortarButton;

        private void Awake()
        {
            _grenadeLauncherImage = _grenadeLauncher.GetComponent<Image>();
            _rpgImage = _rpg.GetComponent<Image>();
            _rocketLauncherImage = _rocketLauncher.GetComponent<Image>();
            _mortarImage = _mortar.GetComponent<Image>();
            _grenadeLauncherButton = _grenadeLauncher.GetComponent<Button>();
            _rpgButton = _rpg.GetComponent<Button>();
            _rocketLauncherButton = _rocketLauncher.GetComponent<Button>();
            _mortarButton = _mortar.GetComponent<Button>();
        }

        protected override void OnEnabled()
        {
            _grenadeLauncherButton.onClick.AddListener(SelectGrenadeLauncher);
            _rpgButton.onClick.AddListener(SelectRpg);
            _rocketLauncherButton.onClick.AddListener(SelectRocketLauncher);
            _mortarButton.onClick.AddListener(SelectMortar);
        }

        protected override void OnDisabled()
        {
            _grenadeLauncherButton.onClick.RemoveListener(SelectGrenadeLauncher);
            _rpgButton.onClick.RemoveListener(SelectRpg);
            _rocketLauncherButton.onClick.RemoveListener(SelectRocketLauncher);
            _mortarButton.onClick.RemoveListener(SelectMortar);
        }

        public void Construct(HeroWeaponSelection heroWeaponSelection)
        {
            _heroWeaponSelection = heroWeaponSelection;
            _heroWeaponSelection.WeaponSelected += HighlightWeapon;
        }

        private void SelectGrenadeLauncher() =>
            _heroWeaponSelection.SelectWeapon(HeroWeaponTypeId.GrenadeLauncher);

        private void SelectRpg() =>
            _heroWeaponSelection.SelectWeapon(HeroWeaponTypeId.RPG);

        private void SelectRocketLauncher() =>
            _heroWeaponSelection.SelectWeapon(HeroWeaponTypeId.RocketLauncher);

        private void SelectMortar() =>
            _heroWeaponSelection.SelectWeapon(HeroWeaponTypeId.Mortar);

        private void HighlightWeapon(GameObject o, HeroWeaponStaticData heroWeaponStaticData, TrailStaticData t) =>
            HighlightWeapon(heroWeaponStaticData.WeaponTypeId);

        private void HighlightWeapon(HeroWeaponTypeId typeId)
        {
            switch (typeId)
            {
                case HeroWeaponTypeId.GrenadeLauncher:
                    _grenadeLauncherImage.sprite = _selected;
                    _rpgImage.sprite = _unselected;
                    _rocketLauncherImage.sprite = _unselected;
                    _mortarImage.sprite = _unselected;
                    break;

                case HeroWeaponTypeId.RPG:
                    _rpgImage.sprite = _selected;
                    _grenadeLauncherImage.sprite = _unselected;
                    _rocketLauncherImage.sprite = _unselected;
                    _mortarImage.sprite = _unselected;
                    break;

                case HeroWeaponTypeId.RocketLauncher:
                    _rocketLauncherImage.sprite = _selected;
                    _grenadeLauncherImage.sprite = _unselected;
                    _rpgImage.sprite = _unselected;
                    _mortarImage.sprite = _unselected;
                    break;

                case HeroWeaponTypeId.Mortar:
                    _mortarImage.sprite = _selected;
                    _grenadeLauncherImage.sprite = _unselected;
                    _rpgImage.sprite = _unselected;
                    _rocketLauncherImage.sprite = _unselected;
                    break;
            }
        }
    }
}