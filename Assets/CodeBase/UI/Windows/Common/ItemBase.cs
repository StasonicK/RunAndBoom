using CodeBase.Data;
using CodeBase.Services;
using CodeBase.Services.Input;
using CodeBase.Services.Localization;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.StaticData;
using CodeBase.UI.Services;
using CodeBase.UI.Windows.Gifts;
using CodeBase.UI.Windows.Shop;
using NTC.Global.Cache;
using NTC.Global.System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Common
{
    public abstract class ItemBase : MonoCache, IProgressReader
    {
        [SerializeField] protected Image BackgroundIcon;
        [SerializeField] protected Image MainIcon;
        [SerializeField] protected Image LevelIcon;
        [SerializeField] protected Image AdditionalIcon;
        [SerializeField] protected TextMeshProUGUI CostText;
        [SerializeField] protected TextMeshProUGUI CountText;
        [SerializeField] protected TextMeshProUGUI TitleText;
        [SerializeField] protected Button Button;

        private ShopItemHighlighter _shopItemHighlighter;
        protected IStaticDataService StaticDataService;
        protected ILocalizationService LocalizationService;
        protected IInputService InputService;
        protected PlayerProgress Progress;
        protected ShopItemBalance ShopItemBalance;
        protected GiftsItemBalance GiftsItemBalance;
        private PlayerProgress _progress;
        protected float Volume;
        protected AudioSource AudioSource;

        private void Awake()
        {
            var parent = transform.parent;
            _shopItemHighlighter = parent.GetComponent<ShopItemHighlighter>();
            AudioSource = parent.GetComponent<AudioSource>();
        }

        protected void Construct(PlayerProgress progress)
        {
            Progress = progress;
            StaticDataService = AllServices.Container.Single<IStaticDataService>();
            LocalizationService = AllServices.Container.Single<ILocalizationService>();
            InputService = AllServices.Container.Single<IInputService>();
            ShopItemBalance = new ShopItemBalance();
            GiftsItemBalance = new GiftsItemBalance();
        }

        protected void ClearData()
        {
            if (BackgroundIcon != null)
                BackgroundIcon.ChangeImageAlpha(Constants.Invisible);

            if (MainIcon != null)
                MainIcon.ChangeImageAlpha(Constants.Invisible);

            if (LevelIcon != null)
                LevelIcon.ChangeImageAlpha(Constants.Invisible);

            if (AdditionalIcon != null)
                AdditionalIcon.ChangeImageAlpha(Constants.Invisible);

            if (CostText != null)
                CostText.text = "";

            if (CountText != null)
                CountText.text = "";

            if (TitleText != null)
                TitleText.text = "";

            _shopItemHighlighter.enabled = false;
            gameObject.Disable();
        }

        public void ChangeClickability(bool isClickable) =>
            Button.enabled = isClickable;

        protected abstract void FillData();
        protected abstract void Clicked();

        public void LoadProgress(PlayerProgress progress)
        {
            _progress = progress;
            _progress.SettingsData.SoundSwitchChanged += SwitchChanged;
            _progress.SettingsData.SoundVolumeChanged += VolumeChanged;
            VolumeChanged();
            SwitchChanged();
        }

        private void VolumeChanged() =>
            Volume = _progress.SettingsData.SoundVolume;

        private void SwitchChanged() =>
            Volume = _progress.SettingsData.SoundOn ? _progress.SettingsData.SoundVolume : Constants.Zero;
    }
}