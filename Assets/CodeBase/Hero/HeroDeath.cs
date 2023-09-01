using System;
using CodeBase.Logic;
using CodeBase.Services;
using CodeBase.UI.Services.Windows;
using CodeBase.UI.Windows.Death;
using NTC.Global.Cache;

namespace CodeBase.Hero
{
    public class HeroDeath : MonoCache, IDeath
    {
        private IWindowService _windowService;
        private IHealth _health;

        public event Action Died;

        private void Awake()
        {
            _windowService = AllServices.Container.Single<IWindowService>();
            _health = Get<IHealth>();
        }

        protected override void OnEnabled() =>
            _health.HealthChanged += HealthChanged;

        protected override void OnDisabled() =>
            _health.HealthChanged -= HealthChanged;

        private void HealthChanged()
        {
            if (_health.Current <= 0)
                Die();
        }

        public void Die()
        {
            Died?.Invoke();
            _windowService.Show<DeathWindow>(WindowId.Death);
        }
    }
}