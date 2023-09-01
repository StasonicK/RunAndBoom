using CodeBase.Data;
using CodeBase.Services.PersistentProgress;
using NTC.Global.Cache;
using TMPro;
using UnityEngine;

namespace CodeBase.UI.Elements.Hud
{
    public class ScoreUI : MonoCache, IProgressReader
    {
        [SerializeField] private TextMeshProUGUI _score;

        private string _sector;
        private MoneyData _moneyData;

        public void LoadProgress(PlayerProgress progress)
        {
            _moneyData = progress.AllStats.AllMoney;
            _moneyData.MoneyChanged += SetMoney;
            SetMoney();
        }

        private void SetMoney() =>
            _score.text = $"{_moneyData.Money}";
    }
}