﻿using System;
using System.Collections;
using CodeBase.Enemy.Attacks;
using CodeBase.Hero;
using CodeBase.Logic;
using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyAnimator))]
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(Attack))]
    public class EnemyDeath : MonoBehaviour, IDeath
    {
        [SerializeField] private GameObject _hitBox;
        [SerializeField] private GameObject _diedBox;

        private const float DestroyDelay = 30f;

        private IPlayerProgressService _progressService;
        private IHealth _health;
        private HeroHealth _heroHealth;
        private AgentMoveToHero _agentMoveToHero;

        private int _reward;
        private bool _isDead;
        private EnemyAnimator _enemyAnimator;
        private WaitForSeconds _coroutineDestroyTimer;

        public event Action Died;

        private void Awake()
        {
            _progressService = AllServices.Container.Single<IPlayerProgressService>();

            _enemyAnimator = GetComponent<EnemyAnimator>();
            _agentMoveToHero = GetComponent<AgentMoveToHero>();
            _health = GetComponent<IHealth>();
            _diedBox.SetActive(false);
        }

        private void OnEnable() =>
            _health.HealthChanged += HealthChanged;

        private void OnDisable() =>
            _health.HealthChanged -= HealthChanged;

        private void Start() =>
            _diedBox.SetActive(false);

        private void OnDestroy() =>
            _health.HealthChanged -= HealthChanged;

        public void Construct(HeroHealth heroHealth, int reward)
        {
            _heroHealth = heroHealth;
            _reward = reward;
            _coroutineDestroyTimer = new WaitForSeconds(DestroyDelay);
        }

        private void HealthChanged()
        {
            if (!_isDead && _health.Current <= 0)
                Die();
        }

        public void Die()
        {
            Died?.Invoke();
            _heroHealth.Vampire(_health.Max);
            _isDead = true;
            _progressService.ProgressData.AllStats.AddMoney(_reward);
            // _progressService.ProgressData.AllStats.CurrentLevelStats.KillsData.Increment();
            _enemyAnimator.PlayDeath();
            _agentMoveToHero.Stop();
            _agentMoveToHero.enabled = false;
            _diedBox.SetActive(true);
            _hitBox.SetActive(false);
            StartCoroutine(CoroutineDestroyTimer());
            Destroy(GetComponent<RotateToHero>());
            Destroy(GetComponent<Aggro>());
            Destroy(GetComponent<AnimateAlongAgent>());
            Destroy(GetComponent<CheckAttackRange>());
            Destroy(GetComponent<StopMovingOnAttack>());
            Destroy(GetComponent<NavMeshAgent>());
            Destroy(_agentMoveToHero);
            Destroy(GetComponent<BoxCollider>());
        }

        private IEnumerator CoroutineDestroyTimer()
        {
            yield return _coroutineDestroyTimer;
            Destroy(gameObject);
        }
    }
}