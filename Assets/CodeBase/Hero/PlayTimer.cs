using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Hero
{
    public class PlayTimer : MonoCache
    {
        private IPlayerProgressService _progressService;
        private bool _isPlaying;
        private float _playTime;

        private void Start() =>
            _progressService = AllServices.Container.Single<IPlayerProgressService>();

        protected override void OnEnabled() =>
            _isPlaying = true;

        protected override void Run()
        {
            if (_isPlaying)
                _playTime += Time.deltaTime;
        }

        protected override void OnDisabled()
        {
            _progressService.Progress.AllStats.CurrentLevelStats.PlayTimeData.Add(_playTime);
            _isPlaying = false;
            _playTime = Constants.Zero;
        }
    }
}