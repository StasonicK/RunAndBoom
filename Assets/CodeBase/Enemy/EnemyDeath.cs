using System;
using System.Collections;
using CodeBase.Enemy.Attacks;
using CodeBase.Hero;
using CodeBase.Logic;
using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using NTC.Global.Cache;
using NTC.Global.System;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyAnimator))]
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(Attack))]
    public class EnemyDeath : MonoCache, IDeath
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

        public event Action Died;

        private void Awake()
        {
            _progressService = AllServices.Container.Single<IPlayerProgressService>();
            _enemyAnimator = Get<EnemyAnimator>();
            _agentMoveToHero = Get<AgentMoveToHero>();
            _health = Get<IHealth>();
            _diedBox.Disable();
        }

        protected override void OnEnabled() =>
            _health.HealthChanged += HealthChanged;

        protected override void OnDisabled() =>
            _health.HealthChanged -= HealthChanged;

        private void Start() =>
            _diedBox.Disable();

        private void OnDestroy() =>
            _health.HealthChanged -= HealthChanged;

        public void Construct(HeroHealth heroHealth, int reward)
        {
            _heroHealth = heroHealth;
            _reward = reward;
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
            _progressService.Progress.AllStats.AddMoney(_reward);
            _progressService.Progress.AllStats.CurrentLevelStats.KillsData.Increment();
            _enemyAnimator.PlayDeath();
            Destroy(Get<StopMovingOnAttack>());
            _agentMoveToHero.Stop();
            _hitBox.SetActive(false);
            _diedBox.SetActive(true);
            StartCoroutine(CoroutineDestroyTimer());
            Destroy(Get<RotateToHero>());
            Destroy(Get<Aggro>());
            Destroy(Get<AnimateAlongAgent>());
            Destroy(Get<CheckAttackRange>());
            Destroy(Get<NavMeshAgent>(), 1);
            Destroy(Get<AgentMoveToHero>());
            Destroy(Get<BoxCollider>());
        }

        private IEnumerator CoroutineDestroyTimer()
        {
            yield return new WaitForSeconds(DestroyDelay);
            Destroy(gameObject);
        }
    }
}