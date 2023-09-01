using CodeBase.Data;
using CodeBase.Services;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Common;
using CodeBase.UI.Windows.Results;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Logic.Level
{
    public class Finish : MonoCache
    {
        [SerializeField] private GameObject pickupEffect;
        [SerializeField] private int _maxPrice;

        private IWindowService _windowService;
        private Scene _nextLevel;
        private Scene _currentLevel;

        private void Awake() =>
            _windowService = AllServices.Container.Single<IWindowService>();

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareByTag(Constants.HeroTag))
            {
                if (pickupEffect != null)
                    Pickup();

                WindowBase resultWindow = _windowService.Show<ResultsWindow>(WindowId.Result);
                (resultWindow as ResultsWindow)?.AddData(_currentLevel, _nextLevel, _maxPrice);
                (resultWindow as ResultsWindow)?.CalculateScore();
                (resultWindow as ResultsWindow)?.ShowData();
            }
        }

        public void Construct(Scene currentLevel, Scene nextLevel)
        {
            _currentLevel = currentLevel;
            _nextLevel = nextLevel;
        }

        private void Pickup()
        {
            Instantiate(pickupEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}