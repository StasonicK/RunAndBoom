using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class Aggro : MonoCache
    {
        [SerializeField] private TriggerObserver _triggerObserver;
        [SerializeField] private Follow _follow;
        [SerializeField] private RotateToHero _rotateToHero;

        private bool _hasAggroTarget;
        private Coroutine _aggroCoroutine;

        private void Start()
        {
            _triggerObserver.TriggerEnter += TriggerEnter;

            if (_follow != null)
            {
                _follow.Stop();
                _follow.enabled = false;
                _rotateToHero.enabled = false;
            }
        }

        private void OnDestroy() =>
            _triggerObserver.TriggerEnter -= TriggerEnter;

        private void TriggerEnter(Collider obj)
        {
            if (_hasAggroTarget)
                return;

            StopAggroCoroutine();
            SwitchFollowOn();
        }

        public void Construct(float radius) =>
            _triggerObserver.GetComponent<SphereCollider>().radius = radius;

        private void StopAggroCoroutine()
        {
            if (_aggroCoroutine == null)
                return;

            StopCoroutine(_aggroCoroutine);
        }

        private void SwitchFollowOn()
        {
            if (_follow != null)
            {
                _hasAggroTarget = true;
                _follow.Move();
                _rotateToHero.enabled = true;
                _follow.enabled = true;
            }
        }
    }
}