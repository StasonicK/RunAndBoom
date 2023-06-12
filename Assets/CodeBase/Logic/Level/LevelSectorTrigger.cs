using System;
using CodeBase.Data;
using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using CodeBase.UI.Windows.Shop;
using UnityEngine;

namespace CodeBase.Logic.Level
{
    public class LevelSectorTrigger : MonoBehaviour, IProgressReader
    {
        private const int MinItemValue = 5;
        [SerializeField] private int _number;
        [SerializeField] private int _refreshCount;
        [SerializeField] private int _watchAdsNumber;

        private IWindowService _windowService;
        private IPlayerProgressService _progressService;
        private bool _isPassed = false;
        private PlayerProgress _progress;
        private MoneyData _moneyData;

        public event Action Passed;

        private void Awake()
        {
            _windowService = AllServices.Container.Single<IWindowService>();
            _progressService = AllServices.Container.Single<IPlayerProgressService>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareByTag(Constants.HeroTag) && _isPassed == false)
            {
                if (AllServices.Container.Single<IPlayerProgressService>().Progress.Stats.CurrentLevelStats.MoneyData
                    .IsMoneyEnough(MinItemValue))
                    // if (_progress.Stats.CurrentLevelStats.MoneyData.IsMoneyEnough(MinItemValue))
                    ShowShopWindow();

                SetPassed();
            }
        }

        private void ShowShopWindow()
        {
            _progressService.Progress.WorldData.LevelNameData.ChangeSector(_number.ToString());
            WindowBase shopWindow = _windowService.Show<ShopWindow>(WindowId.Shop);
            (shopWindow as ShopWindow)?.gameObject.GetComponent<ShopItemsGenerator>()?.Generate();
            (shopWindow as ShopWindow)?.AddCounts(_refreshCount, _watchAdsNumber);
        }

        private void SetPassed()
        {
            Passed?.Invoke();
            _isPassed = true;
        }

        public void LoadProgress(PlayerProgress progress) =>
            _progress = progress;
    }
}