﻿using UnityEngine;

namespace CodeBase.Enemy.Attacks
{
    [RequireComponent(typeof(EnemyAnimator))]
    public class Attack : MonoBehaviour
    {
        [SerializeField] private EnemyAnimator _animator;

        // private const float YLevitation = 0.5f;

        private float _attackCooldown;
        // private float _cleavage;
        // private float _effectiveDistance;
        // private int _damage;

        private Transform _heroTransform;
        private float _currentAttackCooldown;

        private bool _isAttacking;

        // private int _layerMask;
        // private Collider[] _hits = new Collider[1];
        private bool _attackIsActive;

        // private void Awake() =>
        //     _layerMask = 1 << LayerMask.NameToLayer("Hero");

        private void Update()
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

        protected void OnAttack()
        {
        }

        public void EnableAttack() =>
            _attackIsActive = true;

        public void DisableAttack() =>
            _attackIsActive = false;

        private void OnAttackEnded()
        {
            _currentAttackCooldown = _attackCooldown;
            _isAttacking = false;
        }

        private bool CooldownUp() =>
            _currentAttackCooldown <= 0;

        private bool CanAttack() =>
            _attackIsActive && !_isAttacking && CooldownUp();
    }
}