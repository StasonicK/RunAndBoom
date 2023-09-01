using System;
using CodeBase.Logic;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyAnimator : MonoCache, IAnimationStateReader
    {
        private readonly int _attackTriggerHash = Animator.StringToHash("Attack");
        private readonly int _isMovingTriggerHash = Animator.StringToHash("IsMoving");
        private readonly int _dieTriggerHash = Animator.StringToHash("Die");

        private readonly int _idleStateHash = Animator.StringToHash("Idle");
        private readonly int _attackStateHash = Animator.StringToHash("Attack");
        private readonly int _walkingStateHash = Animator.StringToHash("Move");
        private readonly int _deathStateHash = Animator.StringToHash("Death");

        private Animator _animator;

        public AnimatorState State { get; private set; }

        public event Action<AnimatorState> StateEntered;
        public event Action<AnimatorState> StateExited;

        private void Awake() =>
            _animator = Get<Animator>();

        public void Move() =>
            _animator.SetBool(_isMovingTriggerHash, true);

        public void StopMoving() =>
            _animator.SetBool(_isMovingTriggerHash, false);

        public void PlayAttack() =>
            _animator.SetTrigger(_attackTriggerHash);

        public void PlayDeath() =>
            _animator.SetTrigger(_dieTriggerHash);

        public void EnteredState(int stateHash)
        {
            State = StateFor(stateHash);
            StateEntered?.Invoke(State);
        }

        public void ExitedState(int stateHash) =>
            StateExited?.Invoke(StateFor(stateHash));

        private AnimatorState StateFor(int stateHash)
        {
            AnimatorState state;

            if (stateHash == _idleStateHash)
                state = AnimatorState.Idle;
            else if (stateHash == _attackStateHash)
                state = AnimatorState.Attack;
            else if (stateHash == _walkingStateHash)
                state = AnimatorState.Walking;
            else if (stateHash == _deathStateHash)
                state = AnimatorState.Died;
            else
                state = AnimatorState.Unknown;

            return state;
        }
    }
}