using CodeBase.Weapons;
using UnityEngine;

namespace CodeBase.Enemy.Attacks
{
    public class WithPistolAttack : Attack
    {
        private EnemyWeaponAppearance _enemyWeaponAppearance;
        private Transform _hero;

        private void Awake() =>
            _enemyWeaponAppearance = ChildrenGet<EnemyWeaponAppearance>();

        public new void Construct(Transform heroTransform, float attackCooldown)
        {
            base.Construct(heroTransform, attackCooldown);
            _hero = heroTransform;
        }

        protected override void OnAttack() =>
            _enemyWeaponAppearance.Shoot(_hero.position);
    }
}