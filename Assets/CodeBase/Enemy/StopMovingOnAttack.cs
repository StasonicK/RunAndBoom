using CodeBase.Logic;
using NTC.Global.Cache;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(EnemyAnimator))]
    public class StopMovingOnAttack : MonoCache
    {
        private EnemyAnimator _animator;
        private NavMeshAgent _agent;

        private void Awake()
        {
            _agent = Get<NavMeshAgent>();
            _animator = Get<EnemyAnimator>();
        }

        private void Start()
        {
            _animator.StateEntered += SwitchMovementOff;
            _animator.StateExited += SwitchMovementOn;
        }

        private void OnDestroy()
        {
            _animator.StateEntered -= SwitchMovementOff;
            _animator.StateExited -= SwitchMovementOn;
        }

        private void SwitchMovementOn(AnimatorState animatorState)
        {
            if (animatorState == AnimatorState.Attack)
                _agent.isStopped = false;
        }

        private void SwitchMovementOff(AnimatorState animatorState)
        {
            if (animatorState == AnimatorState.Attack)
                _agent.isStopped = true;
        }
    }
}