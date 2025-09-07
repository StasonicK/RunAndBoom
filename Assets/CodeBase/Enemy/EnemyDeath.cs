using System;
using System.Collections;
using CodeBase.Enemy.Attacks;
using CodeBase.Hero;
using CodeBase.Logic;
using CodeBase.Services;
using CodeBase.Services.PersistentProgress;
using UnityEngine;

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
        private RotateToHero _rotateToHero;
        private Aggro _getComponent;
        private AnimateAlongAgent _animateAlongAgent;
        private CheckAttackRange _checkAttackRange;
        private StopMovingOnAttack _stopMovingOnAttack;

        public event Action Died;

        private void Awake()
        {
            _hitBox.SetActive(true);
            _diedBox.SetActive(false);
            _progressService = AllServices.Container.Single<IPlayerProgressService>();
            _enemyAnimator = GetComponent<EnemyAnimator>();
            _agentMoveToHero = GetComponent<AgentMoveToHero>();
            _rotateToHero = GetComponent<RotateToHero>();
            _getComponent = GetComponent<Aggro>();
            _animateAlongAgent = GetComponent<AnimateAlongAgent>();
            _checkAttackRange = GetComponent<CheckAttackRange>();
            _stopMovingOnAttack = GetComponent<StopMovingOnAttack>();
            _health = GetComponent<IHealth>();
        }

        private void OnEnable()
        {
            if (_health != null)
                _health.HealthChanged += HealthChanged;
        }

        private void OnDisable()
        {
            if (_health != null)
                _health.HealthChanged -= HealthChanged;
        }

        private void OnDestroy()
        {
            if (_health != null)
                _health.HealthChanged -= HealthChanged;
        }

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
            _diedBox.SetActive(true);
            _hitBox.SetActive(false);
            Died?.Invoke();
            _heroHealth.Vampire(_health.Max);
            _isDead = true;
            _progressService.ProgressData.AllStats.AddMoney(_reward);
            _enemyAnimator.PlayDeath();
            _agentMoveToHero.Stop();
            _agentMoveToHero.enabled = false;
            StartCoroutine(CoroutineDestroyTimer());
            _rotateToHero.Off();
            _getComponent.Off();
            _animateAlongAgent.Off();
            _checkAttackRange.Off();
            _stopMovingOnAttack.Off();
        }

        private IEnumerator CoroutineDestroyTimer()
        {
            yield return _coroutineDestroyTimer;
            Destroy(gameObject);
        }
    }
}