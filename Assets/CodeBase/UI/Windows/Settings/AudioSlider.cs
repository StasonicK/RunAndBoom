using CodeBase.Data;
using CodeBase.Services.PersistentProgress;
using NTC.Global.Cache;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Windows.Settings
{
    public abstract class AudioSlider : MonoCache, IProgressReader
    {
        [SerializeField] protected Slider Slider;

        protected PlayerProgress Progress;
        protected float Volume;
        protected float PreviousVolume;
        protected bool IsSwitched;

        private void Awake() =>
            Slider.onValueChanged.AddListener(ChangeValue);

        public void LoadProgress(PlayerProgress progress)
        {
            Progress = progress;
            SetListeners();
            SwitchChanged();
            // VolumeChanged();
        }

        protected abstract void ChangeValue(float value);

        protected abstract void SetListeners();
        protected abstract void SwitchChanged();
        protected abstract void ChangeVolume(float value);
        protected abstract void VolumeChanged();
    }
}