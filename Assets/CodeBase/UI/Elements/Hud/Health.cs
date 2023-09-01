using CodeBase.Data;
using CodeBase.Logic;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.UI.Elements.Hud
{
    public class Health : MonoCache
    {
        [SerializeField] private HpBar _hpBar;

        private IHealth _health;
        private PlayerProgress _progress;

        public void Construct(IHealth health)
        {
            _health = health;
            _health.HealthChanged += UpdateHpBar;
        }

        private void UpdateHpBar() =>
            _hpBar.SetValue(_health.Current, _health.Max);
    }
}