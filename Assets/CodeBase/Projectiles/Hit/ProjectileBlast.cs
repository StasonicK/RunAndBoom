using System.Collections;
using System.Linq;
using CodeBase.Data.Progress;
using CodeBase.Data.Progress.Upgrades;
using CodeBase.Services;
using CodeBase.Services.Audio;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Items;
using CodeBase.StaticData.Items.Shop.WeaponsUpgrades;
using CodeBase.StaticData.Weapons;
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
        private IAudioService _audioService;
        private ProgressData _progressData;
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
        private WaitForSeconds _waitForSecondsBlast;

        private void Awake()
        {
            _hitCollider = GetComponent<CapsuleCollider>();
            _audioSource = GetComponent<AudioSource>();
            Tags = new[]
            {
                Constants.EnemyTag, Constants.ObstacleTag, Constants.BarrierTag, Constants.DestructableTag,
                Constants.WallTag, Constants.GroundTag
            };
        }

        public void OffCollider() =>
            _hitCollider.enabled = false;

        public void OnCollider() =>
            _hitCollider.enabled = transform;

        private void OnEnable()
        {
            HideBlast();

            if (_blastItemData != null)
                _blastItemData.LevelChanged += ChangeBlastSize;
        }

        private void OnDisable()
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
                    ShowBlast();
                    PlaySound();
                    Trail?.HideTrace();
                    _destroyWithBlast.HitAllAround(_sphereRadius, _damage);

                    if (gameObject.activeInHierarchy)
                        StartCoroutine(DestroyBlast());

                    Movement.Stop();
                }
            }
        }

        public void Construct(GameObject prefab, float radius, float damage, HeroWeaponTypeId? heroWeaponTypeId = null)
        {
            _heroWeaponTypeId = heroWeaponTypeId;
            _staticDataService = AllServices.Container.Single<IStaticDataService>();
            _audioService = AllServices.Container.Single<IAudioService>();

            _prefab = prefab;
            _sphereRadius = radius;
            _damage = damage;
            _waitForSecondsBlast = new WaitForSeconds(BlastDuration);
        }

        private void PlaySound()
        {
            switch (_heroWeaponTypeId)
            {
                case HeroWeaponTypeId.GrenadeLauncher:
                    _audioService.LaunchBlastSound(BlastSoundId.BlastGrenadeLauncher, transform, _audioSource);
                    break;
                case HeroWeaponTypeId.RPG:
                    _audioService.LaunchBlastSound(BlastSoundId.BlastRocketLauncherAndRpg, transform, _audioSource);
                    break;
                case HeroWeaponTypeId.RocketLauncher:
                    _audioService.LaunchBlastSound(BlastSoundId.BlastRocketLauncherAndRpg, transform, _audioSource);
                    break;
                case HeroWeaponTypeId.Mortar:
                    _audioService.LaunchBlastSound(BlastSoundId.BlastMortar, transform, _audioSource);
                    break;
            }
        }

        private void SetBlastSize()
        {
            _blastItemData = _progressData.WeaponsData.UpgradesData.UpgradeItemDatas.First(x =>
                x.WeaponTypeId == _heroWeaponTypeId && x.UpgradeTypeId == UpgradeTypeId.BlastSize);
            _blastItemData.LevelChanged += ChangeBlastSize;
            ChangeBlastSize();
        }

        private void ChangeBlastSize()
        {
            _blastItemData = _progressData.WeaponsData.UpgradesData.UpgradeItemDatas.First(x =>
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
            yield return _waitForSecondsBlast;
            HideBlast();
        }

        public void LoadProgressData(ProgressData progressData)
        {
            _progressData = progressData;

            if (_heroWeaponTypeId != null)
                SetBlastSize();
        }
    }
}