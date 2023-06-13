using System;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.Weapons;
using UnityEngine;

namespace CodeBase.Hero
{
    public class WeaponRotation : MonoBehaviour
    {
        [SerializeField] private HeroWeaponSelection _weaponSelection;
        [SerializeField] private LayerMask _collidableLayers;
        [SerializeField] private Camera _mainCamera;

        private HeroWeaponStaticData _weaponStaticData;
        private GameObject _currentWeapon;
        private float _centralPosition = 0.5f;
        private float _rotateDuration = 0.5f;
        private float _maxDistance = 25f;

        public Action<Vector3> GotTarget;

        private void Awake() =>
            _weaponSelection.WeaponSelected += WeaponChosen;

        private void WeaponChosen(GameObject selectedWeapon, HeroWeaponStaticData weaponStaticData,
            TrailStaticData arg3)
        {
            _currentWeapon = selectedWeapon;
            _weaponStaticData = weaponStaticData;
        }

        private void FixedUpdate()
        {
            if (_currentWeapon != null)
            {
                Debug.Log($"mainCamera rect: {_mainCamera.rect}");
                Ray ray = _mainCamera.ViewportPointToRay(new Vector3(_centralPosition, _centralPosition, 0));
                Debug.Log($"ray: {ray}");
                var targetPosition = MaxDistancePosition(ray);
                WeaponLookAt(targetPosition);
            }
        }

        private void WeaponLookAt(Vector3 targetPosition)
        {
            _currentWeapon.transform.LookAt(targetPosition);
            Debug.DrawLine(transform.position, targetPosition, Color.red);

            if (_weaponStaticData.WeaponTypeId == HeroWeaponTypeId.Mortar)
                GotTarget?.Invoke(targetPosition);
        }

        private Vector3 MaxDistancePosition(Ray ray)
        {
            RaycastHit[] results = new RaycastHit[1];
            int count = Physics.RaycastNonAlloc(ray, results, _maxDistance, _collidableLayers);
            return count > 0 ? results[0].point : ray.GetPoint(_maxDistance);
        }
    }
}