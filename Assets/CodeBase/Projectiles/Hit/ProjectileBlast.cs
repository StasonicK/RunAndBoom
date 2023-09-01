using System.Collections;
using System.Linq;
using CodeBase.Data;
using CodeBase.Data.Upgrades;
using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Items;
using CodeBase.StaticData.Items.Shop.WeaponsUpgrades;
using CodeBase.StaticData.Weapons;
using Plugins.SoundInstance.Core.Static;
using UnityEngine;

namespace CodeBase.Projectiles.Hit
{
    [RequireComponent(typeof(AudioSource))]
    public class ProjectileBlast : BaseProjectileHit, IProgressReader
    {
        [SerializeField] private DestroyWithBlast _destroyWithBlast;
        [SerializeField] private LayerMask _layerMask;

        private const float BaseRatio = 1f;
        private const float BlastDuration = 2f;
        private const float SphereCastRadius = 0.2f;

        private IStaticDataService _staticDataService;
        private PlayerProgress _progress;
        private float _baseBlastRadius;
        private UpgradeItemData _blastItemData;
        private float _blastRadiusRatio = BaseRatio;
        private GameObject _prefab;
        private float _sphereRadius;
        private ParticleSystem _particleSystem;
        private GameObject _blastVfx;
        private float _damage;
        private CapsuleCollider _hitCollider;
        private HeroWeaponTypeId? _heroWeaponTypeId;
        private AudioSource _audioSource;
        private float _volume = 1f;

        private void Awake()
        {
            _hitCollider = Get<CapsuleCollider>();
            _audioSource = Get<AudioSource>();
            Tags = new[]
            {
                Constants.EnemyTag, Constants.ObstacleTag, Constants.BarrierTag, Constants.DestructableTag,
                Constants.WallTag, Constants.GroundTag
            };
        }

        protected override void OnEnabled()
        {
            HideBlast();

            if (_blastItemData != null)
                _blastItemData.LevelChanged += ChangeBlastSize;
        }

        protected override void OnDisabled()
        {
            if (_blastItemData != null)
                _blastItemData.LevelChanged -= ChangeBlastSize;
        }

        private void OnTriggerEnter(Collider other)
        {
            string targetTag = other.gameObject.tag;

            if (IsTargetTag(targetTag))
            {
                if (_prefab != null)
                {
                    // RaycastHit hit;
                    // int count = Physics.SphereCastNonAlloc(other.transform.position, SphereCastRadius,
                    //     other.transform.forward, out RaycastHit hit);
                    // if (Physics.SphereCast(other.transform.position, SphereCastRadius,
                    //         other.transform.forward, out RaycastHit hit))
                    // if (Physics.Raycast(transform.position, transform.forward, out hit, 1f, _layerMask))
                    // if (count > 0)
                    // {
                    // ShowBlast(hit.point, hit.normal);
                    ShowBlast();
                    PlaySound();
                    Trail?.HideTrace();
                    _destroyWithBlast.HitAllAround(_sphereRadius, _damage);
                    StartCoroutine(DestroyBlast());
                    Movement.Stop();
                    // }
                }
            }
        }

        public void Construct(GameObject prefab, float radius, float damage, HeroWeaponTypeId? heroWeaponTypeId = null)
        {
            _heroWeaponTypeId = heroWeaponTypeId;
            _staticDataService = AllServices.Container.Single<IStaticDataService>();

            _prefab = prefab;
            _sphereRadius = radius;
            _damage = damage;
        }

        public void OffCollider() =>
            _hitCollider.enabled = false;

        public void OnCollider() =>
            _hitCollider.enabled = transform;

        private void PlaySound()
        {
            switch (_heroWeaponTypeId)
            {
                case HeroWeaponTypeId.GrenadeLauncher:
                    SoundInstance.InstantiateOnPos(
                        audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.BlastGrenadeLauncher),
                        position: transform.position, _volume, _audioSource);
                    break;
                case HeroWeaponTypeId.RPG:
                    SoundInstance.InstantiateOnPos(
                        audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.BlastRocketLauncherAndRpg),
                        position: transform.position, _volume, _audioSource);
                    break;
                case HeroWeaponTypeId.RocketLauncher:
                    SoundInstance.InstantiateOnPos(
                        audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.BlastRocketLauncherAndRpg),
                        position: transform.position, _volume, _audioSource);
                    break;
                case HeroWeaponTypeId.Mortar:
                    SoundInstance.InstantiateOnPos(
                        audioClip: SoundInstance.GetClipFromLibrary(AudioClipAddresses.BlastMortar),
                        position: transform.position, _volume, _audioSource);
                    break;
            }
        }

        private void SetBlastSize()
        {
            _blastItemData = _progress.WeaponsData.UpgradesData.UpgradeItemDatas.First(x =>
                x.WeaponTypeId == _heroWeaponTypeId && x.UpgradeTypeId == UpgradeTypeId.BlastSize);
            _blastItemData.LevelChanged += ChangeBlastSize;
            ChangeBlastSize();
        }

        private void ChangeBlastSize()
        {
            _blastItemData = _progress.WeaponsData.UpgradesData.UpgradeItemDatas.First(x =>
                x.WeaponTypeId == _heroWeaponTypeId && x.UpgradeTypeId == UpgradeTypeId.BlastSize);

            if (_blastItemData.LevelTypeId == LevelTypeId.None)
                _blastRadiusRatio = BaseRatio;
            else
                _blastRadiusRatio = _staticDataService
                    .ForUpgradeLevelsInfo(_blastItemData.UpgradeTypeId, _blastItemData.LevelTypeId).Value;

            _sphereRadius = _baseBlastRadius * _blastRadiusRatio;
        }

        private void ShowBlast(Vector3 point, Vector3 normal)
        {
            if (_particleSystem == null)
            {
                _blastVfx = Instantiate(_prefab, point + normal, Quaternion.identity, null);
                _particleSystem = _blastVfx.GetComponent<ParticleSystem>();
            }
            else
            {
                _blastVfx.transform.position = transform.position;
            }

            _blastVfx.transform.LookAt(point + normal);
            _particleSystem.Play(true);
        }

        private void ShowBlast()
        {
            if (_particleSystem == null)
            {
                _blastVfx = Instantiate(_prefab, transform.position, Quaternion.identity, null);
                _particleSystem = _blastVfx.GetComponent<ParticleSystem>();
            }
            else
            {
                _blastVfx.transform.position = transform.position;
            }

            _particleSystem.Play(true);
        }

        private void HideBlast()
        {
            if (_particleSystem != null)
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private IEnumerator DestroyBlast()
        {
            yield return new WaitForSeconds(BlastDuration);
            HideBlast();
        }

        public void LoadProgress(PlayerProgress progress)
        {
            _progress = progress;
            _progress.SettingsData.SoundSwitchChanged += SwitchChanged;
            _progress.SettingsData.SoundVolumeChanged += VolumeChanged;
            VolumeChanged();
            SwitchChanged();

            if (_heroWeaponTypeId != null)
                SetBlastSize();
        }

        private void VolumeChanged() =>
            _volume = _progress.SettingsData.SoundVolume;

        private void SwitchChanged() =>
            _volume = _progress.SettingsData.SoundOn ? _progress.SettingsData.SoundVolume : Constants.Zero;
    }
}