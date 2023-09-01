using CodeBase.Weapons;
using UnityEngine;

namespace CodeBase.Enemy.Attacks
{
    public class WithShotgunAttack : Attack
    {
        private EnemyWeaponAppearance _enemyWeaponAppearance;

        private void Awake() =>
            _enemyWeaponAppearance = ChildrenGet<EnemyWeaponAppearance>();

        public new void Construct(Transform heroTransform, float attackCooldown) =>
            base.Construct(heroTransform, attackCooldown);

        protected override void OnAttack() =>
            _enemyWeaponAppearance.Shoot();
    }
}