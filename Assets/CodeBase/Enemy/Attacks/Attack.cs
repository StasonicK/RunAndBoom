using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Enemy.Attacks
{
    [RequireComponent(typeof(EnemyAnimator))]
    public abstract class Attack : MonoCache
    {
        [SerializeField] private EnemyAnimator _animator;

        private float _attackCooldown;
        private Transform _heroTransform;
        private float _currentAttackCooldown;
        private bool _isAttacking;

        protected override void Run()
        {
            UpdateCooldown();

            if (CanAttack())
                StartAttack();
        }

        protected void Construct(Transform heroTransform, float attackCooldown)
        {
            _heroTransform = heroTransform;
            _attackCooldown = attackCooldown;
        }

        private void UpdateCooldown()
        {
            if (!CooldownUp())
                _currentAttackCooldown -= Time.deltaTime;
        }

        private void StartAttack()
        {
            transform.LookAt(_heroTransform);
            _animator.PlayAttack();
            _isAttacking = true;
        }

        private void OnAttackEnded()
        {
            _currentAttackCooldown = _attackCooldown;
            _isAttacking = false;
        }

        private bool CooldownUp() =>
            _currentAttackCooldown <= 0;

        private bool CanAttack() =>
            !_isAttacking && CooldownUp();

        protected abstract void OnAttack();
    }
}