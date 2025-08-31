using CodeBase.Data.Settings;
using CodeBase.Logic;
using CodeBase.Projectiles;
using CodeBase.Projectiles.Hit;
using CodeBase.Projectiles.Movement;
using CodeBase.Services;
using CodeBase.Services.Audio;
using CodeBase.Services.Constructor;
using CodeBase.Services.PersistentProgress;
using CodeBase.Services.Pool;
using CodeBase.Services.StaticData;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.ShotVfxs;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeBase.Weapons
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class BaseWeaponAppearance : MonoBehaviour
    {
        [FormerlySerializedAs("ProjectilesRespawns")] [SerializeField]
        public Transform[] _projectilesRespawns;

        [FormerlySerializedAs("ShotVfxsRespawns")]
        [FormerlySerializedAs("MuzzlesRespawns")]
        [FormerlySerializedAs("_muzzlesRespawns")]
        [SerializeField]
        protected Transform[] _shotVfxsRespawns;

        [FormerlySerializedAs("ShowProjectiles")] [SerializeField]
        protected bool _showProjectiles;

        [FormerlySerializedAs("ShotVfxsContainer")] [SerializeField]
        protected ShotVfxsContainer _shotVfxsContainer;

        protected AudioSource _audioSource;
        protected IAudioService _audioService;
        private SettingsData _settingsData;
        protected IStaticDataService _staticDataService;
        protected IConstructorService _constructorService;
        protected IObjectsPoolService _poolService;
        private bool _initialVisibility;
        protected ProjectileTypeId? _projectileTypeId;
        private IDeath _death;
        protected bool _enabled = true;
        protected float _volume = 1f;
        protected GameObject _projectile;

        protected WaitForSeconds _launchProjectileCooldown { get; private set; }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable() =>
            Enable();

        private void OnDisable() =>
            Disable();

        protected void Construct(IDeath death, float shotVfxLifeTime, float cooldown, ProjectileTypeId projectileTypeId,
            ShotVfxTypeId shotVfxTypeId)
        {
            _death = death;
            _poolService = AllServices.Container.Single<IObjectsPoolService>();
            _settingsData = AllServices.Container.Single<IPlayerProgressService>().SettingsData;
            _staticDataService = AllServices.Container.Single<IStaticDataService>();
            _constructorService = AllServices.Container.Single<IConstructorService>();
            _audioService = AllServices.Container.Single<IAudioService>();
            _shotVfxsContainer.Construct(shotVfxLifeTime, shotVfxTypeId, transform);
            _launchProjectileCooldown = new WaitForSeconds(cooldown);
            _projectileTypeId = projectileTypeId;
        }

        private void Enable()
        {
            _enabled = true;

            if (_death != null)
                _death.Died += Disable;

            if (_settingsData == null)
                return;

            _settingsData.SoundSwitchChanged += SwitchChanged;
            _settingsData.SoundVolumeChanged += VolumeChanged;
            VolumeChanged();
            SwitchChanged();
        }

        private void Disable()
        {
            _enabled = false;

            if (_death != null)
                _death.Died -= Disable;

            if (_settingsData == null)
                return;

            _settingsData.SoundSwitchChanged -= SwitchChanged;
            _settingsData.SoundVolumeChanged -= VolumeChanged;
        }

        protected async UniTask<GameObject> SetNewProjectile(Transform respawn)
        {
            // Debug.Log("SetNewProjectile");
            GameObject projectile = await GetProjectile();
            projectile.transform.SetParent(respawn);
            projectile.transform.localPosition = Vector3.zero;
            projectile.transform.rotation = respawn.rotation;
            // Debug.Log($"SetNewProjectile {projectile}");
            return projectile;
        }

        protected async UniTask<GameObject> SetNewProjectile(Transform respawn, Vector3 targetPosition)
        {
            GameObject projectile = await GetProjectile();
            projectile.transform.SetParent(respawn);
            projectile.transform.localPosition = Vector3.zero;
            projectile.transform.rotation = RotationTo(targetPosition, respawn.position);
            return projectile;
        }

        private Quaternion RotationTo(Vector3 targetPosition, Vector3 respawnPosition)
        {
            Vector3 positionDelta = targetPosition - respawnPosition;
            Vector3 directionToLook = new Vector3(positionDelta.x, positionDelta.y, positionDelta.z).normalized;
            return Quaternion.LookRotation(directionToLook);
        }

        protected void TuneProjectileBeforeLaunch(GameObject projectile, ProjectileMovement projectileMovement)
        {
            projectile.GetComponentInChildren<MeshRenderer>().enabled = true;
            projectile.GetComponentInChildren<ProjectileBlast>()?.OnCollider();
            projectile.SetActive(true);
            projectileMovement.Launch();
            projectile.transform.SetParent(null);
            ShowTrail(projectile);
        }

        private void ShowTrail(GameObject projectile)
        {
            if (_projectileTypeId != null)
                projectile.GetComponent<ProjectileTrail>().ShowTrail();
        }

        protected abstract void PlayShootSound();

        protected abstract UniTask<GameObject> GetProjectile();

        protected abstract void Launch();

        protected abstract void Launch(Vector3 targetPosition);

        private void VolumeChanged() =>
            _volume = _settingsData.SoundVolume;

        private void SwitchChanged() =>
            _volume = _settingsData.SoundOn ? _settingsData.SoundVolume : Constants.Zero;
    }
}