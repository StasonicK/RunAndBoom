using System;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Shop
{
    public class ShopWindow : WindowBase
    {
        [SerializeField] private Button _skipButton;
        [SerializeField] private Button _refreshButton;
        [SerializeField] private Button _refreshWithAdsButton;
        [SerializeField] private ItemsGeneratorBase _generator;

        private const int MinItemValue = 5;

        private int _currentRefreshCount = 0;
        private int _maxRefreshCount;
        private int _watchAdsNumber;

        private void Awake()
        {
            _generator.GenerationStarted += DisableRefreshButtons;
            _generator.GenerationEnded += CheckRefreshButtons;
        }

        private void OnEnable()
        {
            _skipButton.onClick.AddListener(Hide);
            _refreshButton.onClick.AddListener(GenerateShopItems);
            _refreshWithAdsButton.onClick.AddListener(ShowAdsAndGenerate);
        }

        private void OnDisable()
        {
            _skipButton.onClick.RemoveListener(Hide);
            _refreshButton.onClick.RemoveListener(GenerateShopItems);
            _refreshWithAdsButton.onClick.RemoveListener(ShowAdsAndGenerate);
        }

        public override void Show(bool showCursor)
        {
            if (Progress.Stats.CurrentLevelStats.MoneyData.IsMoneyEnough(MinItemValue))
                base.Show(showCursor);
        }

        protected override void Hide()
        {
            Cursor.lockState = CursorLockMode.Locked;
            base.Hide();
        }

        public void Construct(GameObject hero) =>
            base.Construct(hero, WindowId.Shop);

        public void AddCounts(int maxRefreshCount, int watchAdsNumber)
        {
            _maxRefreshCount = maxRefreshCount;
            _watchAdsNumber = watchAdsNumber;
            CheckCounts();
        }

        private void DisableRefreshButtons()
        {
            _refreshButton.enabled = false;
            _refreshWithAdsButton.enabled = false;
        }

        private void CheckRefreshButtons()
        {
            _refreshButton.enabled = true;
            _refreshWithAdsButton.enabled = true;

            _currentRefreshCount++;
            CheckCounts();
        }

        private void CheckCounts()
        {
            if (NeedShowRefreshButtons())
            {
                CheckCurrentEqualsWatchAdsNumber();
            }
            else
            {
                HideRefreshButton();
                HideRefreshWithAdsButton();
            }
        }

        private bool NeedShowRefreshButtons()
        {
            if (_maxRefreshCount == Decimal.Zero || _maxRefreshCount == _currentRefreshCount)
            {
                return false;
            }

            return true;
        }

        private void CheckCurrentEqualsWatchAdsNumber()
        {
            if (_watchAdsNumber == _currentRefreshCount)
            {
                ShowRefreshWithAdsButton();
                HideRefreshButton();
            }
            else
            {
                ShowRefreshButton();
                HideRefreshWithAdsButton();
            }
        }

        private void ShowRefreshButton() =>
            _refreshButton.gameObject.SetActive(true);

        private void HideRefreshButton() =>
            _refreshButton.gameObject.SetActive(false);

        private void ShowRefreshWithAdsButton() =>
            _refreshWithAdsButton.gameObject.SetActive(true);

        private void HideRefreshWithAdsButton() =>
            _refreshWithAdsButton.gameObject.SetActive(false);

        private void Start() =>
            Cursor.lockState = CursorLockMode.Confined;

        private void ShowAdsAndGenerate()
        {
            //TODO ShowAds screen
            GenerateShopItems();
        }

        private void GenerateShopItems()
        {
            SoundInstance.InstantiateOnTransform(
                audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.Generate), transform: transform,
                Volume);
            _generator.Generate();
        }
    }
}