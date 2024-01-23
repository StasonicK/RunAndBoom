using System.Collections;
using CodeBase.Services;
using CodeBase.Services.Pool;
using CodeBase.StaticData.ShotVfxs;
using UnityEngine;

namespace CodeBase.Weapons
{
    public class ShotVfxsContainer : MonoBehaviour
    {
        [SerializeField] private GameObject _shotVfx;

        // private IObjectsPoolService _objectsPoolService;

        private IVfxsPoolService _vfxsPoolService;
        private float _shotVfxLifetime;
        private int _index;
        private Transform _root;
        private ShotVfxTypeId _shotVfxTypeId;
        private WaitForSeconds _coroutineLaunchShotVfx;

        public void Construct(float shotVfxLifetime, ShotVfxTypeId shotVfxTypeId, Transform root)
        {
            _shotVfxTypeId = shotVfxTypeId;
            // _objectsPoolService = AllServices.Container.Single<IObjectsPoolService>();
            _vfxsPoolService = AllServices.Container.Single<IVfxsPoolService>();
            _shotVfxLifetime = shotVfxLifetime;
            _root = root;

            if (_coroutineLaunchShotVfx == null)
                _coroutineLaunchShotVfx = new WaitForSeconds(_shotVfxLifetime);
        }

        public void ShowShotVfx(Transform muzzleTransform)
        {
            _shotVfx = _vfxsPoolService.GetFromPool(_shotVfxTypeId);
            // _shotVfx = _objectsPoolService.GetShotVfx(_shotVfxTypeId);
            _shotVfx.transform.SetParent(_root);
            SetShotVfx(_shotVfx, muzzleTransform);
            StartCoroutine(CoroutineLaunchShotVfx());
        }

        private void SetShotVfx(GameObject shotVfx, Transform muzzleTransform)
        {
            shotVfx.transform.position = muzzleTransform.position;
            shotVfx.transform.rotation = muzzleTransform.rotation;
        }

        private IEnumerator CoroutineLaunchShotVfx()
        {
            _shotVfx.SetActive(true);
            yield return _coroutineLaunchShotVfx;
            ReturnShotVfx();
        }

        public void ReturnShotVfx()
        {
            if (_vfxsPoolService == null || _shotVfx == null)
            // if (_objectsPoolService == null || _shotVfx == null)
                return;

            _vfxsPoolService.Return(_shotVfx);
            // _objectsPoolService.ReturnShotVfx(_shotVfx);
            _shotVfx = null;
        }
    }
}