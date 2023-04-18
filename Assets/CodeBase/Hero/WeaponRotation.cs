﻿using System;
using CodeBase.StaticData.Projectiles;
using CodeBase.StaticData.Weapons;
using UnityEngine;

namespace CodeBase.Hero
{
    public class WeaponRotation : MonoBehaviour
    {
        [SerializeField] private HeroWeaponSelection _weaponSelection;
        [SerializeField] private LayerMask _collidableLayers;

        private GameObject _currentWeapon;
        private Camera _mainCamera;
        private float _centralPosition = 0.5f;
        private float _rotateDuration = 0.5f;
        private float _maxDistance = 25f;

        public Action<Vector3> GotTarget;

        private void Start() =>
            _mainCamera = Camera.main;

        private void Awake() =>
            _weaponSelection.WeaponSelected += WeaponChosen;

        private void WeaponChosen(GameObject selectedWeapon, HeroWeaponStaticData weaponStaticData, TrailStaticData arg3) =>
            _currentWeapon = selectedWeapon;

        private void FixedUpdate()
        {
            if (_currentWeapon != null)
            {
                Ray ray = _mainCamera.ViewportPointToRay(new Vector3(_centralPosition, _centralPosition, 0));
                var targetPosition = MaxDistancePosition(ray);
                WeaponLookAt(targetPosition);
            }
        }

        private void WeaponLookAt(Vector3 targetPosition)
        {
            _currentWeapon.transform.LookAt(targetPosition);
            GotTarget?.Invoke(targetPosition);
            Debug.DrawLine(transform.position, targetPosition, Color.red);
        }

        private Vector3 MaxDistancePosition(Ray ray)
        {
            RaycastHit[] results = new RaycastHit[1];
            int count = Physics.RaycastNonAlloc(ray, results, _maxDistance, _collidableLayers);
            return count > 0 ? results[0].point : ray.GetPoint(_maxDistance);
        }
    }
}