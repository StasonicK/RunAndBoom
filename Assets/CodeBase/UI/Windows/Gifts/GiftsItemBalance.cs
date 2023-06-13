using CodeBase.Data;
using CodeBase.Services;
using CodeBase.Services.PersistentProgress;

namespace CodeBase.UI.Windows.Gifts
{
    public class GiftsItemBalance
    {
        private PlayerProgress _progress;

        public GiftsItemBalance() =>
            _progress = AllServices.Container.Single<IPlayerProgressService>().Progress;

        public void AddMoney(int value) =>
            _progress.Stats.AddMoney(value);
    }
}