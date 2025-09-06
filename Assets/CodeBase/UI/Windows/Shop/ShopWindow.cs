using System;
using CodeBase.UI.Elements.Hud;
using CodeBase.UI.Elements.Hud.MobileInputPanel;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Shop
{
    public class ShopWindow : WindowBase
    {
        [SerializeField] private Button _skipButton;
        [SerializeField] private Button _refreshButton;
        [SerializeField] private ItemsGeneratorBase _generator;

        private int _currentRefreshCount = 0;
        private int _maxRefreshCount;
        private int _watchAdsNumber;

        private void OnEnable()
        {
            _skipButton.onClick.AddListener(CloseShop);
            _refreshButton.onClick.AddListener(GenerateShopItems);
            _generator.GenerationStarted += DisableRefreshButtons;
            _generator.GenerationEnded += CheckRefreshButtons;
        }

        private void OnDisable()
        {
            _skipButton.onClick.RemoveListener(CloseShop);
            _refreshButton.onClick.RemoveListener(GenerateShopItems);
            _generator.GenerationStarted -= DisableRefreshButtons;
            _generator.GenerationEnded -= CheckRefreshButtons;
        }

        public void Construct(GameObject hero, OpenSettings openSettings, MobileInput mobileInput) =>
            base.Construct(hero, WindowId.Shop, openSettings, mobileInput);

        public void AddCounts(int maxRefreshCount, int watchAdsNumber)
        {
            _maxRefreshCount = maxRefreshCount;
            _watchAdsNumber = watchAdsNumber;
            CheckCounts();
        }

        private void DisableRefreshButtons() =>
            _refreshButton.enabled = false;

        private void CheckRefreshButtons()
        {
            _refreshButton.enabled = true;
            _currentRefreshCount++;
            CheckCounts();
        }

        private void CheckCounts()
        {
            if (NeedShowRefreshButtons())
                CheckCurrentEqualsWatchAdsNumber();
            else
                HideRefreshButton();
        }

        private bool NeedShowRefreshButtons()
        {
            if (_maxRefreshCount == Decimal.Zero || _maxRefreshCount == _currentRefreshCount)
                return false;

            return true;
        }

        private void CheckCurrentEqualsWatchAdsNumber()
        {
            if (_watchAdsNumber == _currentRefreshCount)
                HideRefreshButton();
            else
                ShowRefreshButton();
        }

        private void ShowRefreshButton() =>
            _refreshButton.gameObject.SetActive(true);

        private void HideRefreshButton() =>
            _refreshButton.gameObject.SetActive(false);

        private void Start() =>
            Cursor.lockState = CursorLockMode.Confined;

        private void CloseShop() =>
            Close();

        private void GenerateShopItems() =>
            _generator.Generate();
    }
}