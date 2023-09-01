using System.Collections;
using CodeBase.Services;
using CodeBase.Services.Pool;
using CodeBase.StaticData.ShotVfxs;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Weapons
{
    public class ShotVfxsContainer : MonoCache
    {
        [SerializeField] private GameObject _shotVfx;

        private IObjectsPoolService _objectsPoolService;
        private float _shotVfxLifetime;
        private int _index;
        private Transform _root;
        private ShotVfxTypeId _shotVfxTypeId;

        public void Construct(float shotVfxLifetime, ShotVfxTypeId shotVfxTypeId, Transform root)
        {
            _shotVfxTypeId = shotVfxTypeId;
            _objectsPoolService = AllServices.Container.Single<IObjectsPoolService>();
            _shotVfxLifetime = shotVfxLifetime;
            _root = root;
        }

        public void ShowShotVfx(Transform muzzleTransform)
        {
            _shotVfx = _objectsPoolService.GetShotVfx(_shotVfxTypeId);
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
            yield return new WaitForSeconds(_shotVfxLifetime);
            ReturnShotVfx();
        }

        public void ReturnShotVfx()
        {
            if (_objectsPoolService == null || _shotVfx == null)
                return;

            _objectsPoolService.ReturnShotVfx(_shotVfx);
            _shotVfx = null;
        }
    }
}