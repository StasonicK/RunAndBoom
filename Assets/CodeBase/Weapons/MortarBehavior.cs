using System.Collections;
using CodeBase.Hero;
using UnityEngine;

namespace CodeBase.Weapons
{
    public class MortarBehavior : HeroWeaponAppearance
    {
        [SerializeField] private WeaponRotation _weaponRotation;

        private void Awake() =>
            _weaponRotation.GotTarget += SetTarget;

        private void SetTarget(Vector3 targetPosition) =>
            _targetPosition = targetPosition;

        protected IEnumerator CoroutineShootTo()
        {
            Launch(_targetPosition);
            yield return _launchProjectileCooldown;
        }
    }
}