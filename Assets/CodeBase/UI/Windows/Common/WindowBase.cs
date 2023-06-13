using CodeBase.Data;
using CodeBase.Hero;
using CodeBase.Infrastructure.States;
using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.SaveLoad;
using CodeBase.Services.StaticData;
using CodeBase.UI.Services.Windows;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;

namespace CodeBase.UI.Windows.Common
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class WindowBase : MonoBehaviour, IProgressReader
    {
        protected IWindowService WindowService;

        // protected IPlayerProgressService ProgressService;
        protected ISaveLoadService SaveLoadService;
        protected IGameStateMachine GameStateMachine;
        protected IStaticDataService StaticDataService;
        protected AudioSource AudioSource;
        protected GameObject Hero;
        [HideInInspector] public PlayerProgress Progress;
        protected float Volume;
        private WindowId _windowId;

        protected void Construct(GameObject hero, WindowId windowId)
        {
            WindowService = AllServices.Container.Single<IWindowService>();
            // ProgressService = AllServices.Container.Single<IPlayerProgressService>();
            SaveLoadService = AllServices.Container.Single<ISaveLoadService>();
            GameStateMachine = AllServices.Container.Single<IGameStateMachine>();
            StaticDataService = AllServices.Container.Single<IStaticDataService>();
            AudioSource = GetComponent<AudioSource>();
            Hero = hero;
            _windowId = windowId;
            Hide();
        }

        protected virtual void Hide()
        {
            gameObject.SetActive(false);
            PlayCloseSound();

            if (!WindowService.IsAnotherActive(_windowId))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Hero.GetComponent<HeroShooting>().TurnOn();
                Hero.GetComponent<HeroMovement>().TurnOn();
                Hero.GetComponent<HeroRotating>().TurnOn();
                Hero.GetComponent<HeroReloading>().TurnOn();
                Hero.GetComponentInChildren<HeroWeaponSelection>().TurnOn();
                Hero.GetComponentInChildren<PlayTimer>().enabled = true;
                Time.timeScale = 1;
            }
        }

        public virtual void Show(bool showCursor)
        {
            gameObject.SetActive(true);
            Hero.GetComponent<HeroShooting>().TurnOff();
            Hero.GetComponent<HeroReloading>().TurnOff();
            Hero.GetComponent<HeroMovement>().TurnOff();
            Hero.GetComponent<HeroRotating>().TurnOff();
            Hero.GetComponentInChildren<HeroWeaponSelection>().TurnOff();
            Hero.GetComponentInChildren<PlayTimer>().enabled = false;
            Time.timeScale = 0;
            ShowCursor(showCursor);
            PlayOpenSound();
        }

        private void ShowCursor(bool showCursor) =>
            Cursor.lockState = showCursor ? CursorLockMode.Confined : CursorLockMode.Locked;

        protected virtual void PlayCloseSound()
        {
            SoundInstance.InstantiateOnTransform(
                audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.MenuClose), transform: transform,
                Volume, AudioSource);
        }

        protected virtual void PlayOpenSound()
        {
            SoundInstance.InstantiateOnTransform(
                audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.MenuOpen), transform: transform,
                Volume, AudioSource);
        }

        public void LoadProgress(PlayerProgress progress)
        {
            Progress = progress;
            Progress.SettingsData.SoundSwitchChanged += SwitchChanged;
            Progress.SettingsData.SoundVolumeChanged += VolumeChanged;
            VolumeChanged();
            SwitchChanged();
        }

        private void VolumeChanged() =>
            Volume = Progress.SettingsData.SoundVolume;

        private void SwitchChanged() =>
            Volume = Progress.SettingsData.SoundOn ? Progress.SettingsData.SoundVolume : Constants.Zero;

        protected void RestartLevel()
        {
            WindowService.HideAll();
            Progress.Stats.RestartedLevel();
            SoundInstance.StopRandomMusic();
            AllServices.Container.Single<IGameStateMachine>().Enter<LoadPlayerProgressState>();
        }
    }
}