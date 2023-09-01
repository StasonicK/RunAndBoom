using CodeBase.Data;
using CodeBase.Services.PersistentProgress;
using NTC.Global.Cache;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;

namespace CodeBase.Logic.Level
{
    [RequireComponent(typeof(AudioSource))]
    public class DoorMovement : MonoCache, IProgressReader
    {
        [SerializeField] private GameObject _door;
        [SerializeField] private LevelSectorTrigger _trigger;
        [SerializeField] private float _minY;
        [SerializeField] private float _maxY;

        private const float Speed = 10f;

        private AudioSource _audioSource;
        private float _positionY;
        private float _targetY;
        private Transform _doorTransform;
        private bool _close;
        private Coroutine _movementCoroutine;
        private PlayerProgress _progress;
        private float _volume;

        private void Awake()
        {
            _audioSource = Get<AudioSource>();
            _doorTransform = _door.GetComponent<Transform>();
            _positionY = _door.transform.position.y;
            _targetY = _positionY;
            _minY = _positionY - _door.GetComponent<MeshRenderer>().bounds.size.y;
            _maxY = _positionY;

            _trigger.Passed += Close;
        }

        private void Close()
        {
            _targetY = _maxY;
            _close = true;
        }

        protected override void Run()
        {
            MoveDoor();
        }

        private void MoveDoor()
        {
            if (_doorTransform.position.y != _targetY)
                _doorTransform.position = Vector3.MoveTowards(_doorTransform.position,
                    new Vector3(_doorTransform.position.x, _targetY, _doorTransform.position.z),
                    Speed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareByTag(Constants.HeroTag) && _close == false)
            {
                _targetY = _minY;
                SoundInstance.GetClipFromLibrary(AudioClipAddresses.DoorClosing);
                SoundInstance.InstantiateOnTransform(
                    audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.DoorClosing), transform: transform,
                    _volume, _audioSource);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareByTag(Constants.HeroTag) && _close == false)
            {
                _targetY = _maxY;
                SoundInstance.GetClipFromLibrary(AudioClipAddresses.DoorOpening);
                SoundInstance.InstantiateOnTransform(
                    audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.DoorOpening), transform: transform,
                    _volume, _audioSource);
            }
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _progress = progress;
            _progress.SettingsData.SoundSwitchChanged += SwitchChanged;
            _progress.SettingsData.SoundVolumeChanged += VolumeChanged;
            VolumeChanged();
            SwitchChanged();
        }

        private void VolumeChanged() =>
            _volume = _progress.SettingsData.SoundVolume;

        private void SwitchChanged() =>
            _volume = _progress.SettingsData.SoundOn ? _progress.SettingsData.SoundVolume : Constants.Zero;
    }
}