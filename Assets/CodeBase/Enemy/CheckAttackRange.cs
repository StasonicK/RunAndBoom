using CodeBase.Enemy.Attacks;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class CheckAttackRange : MonoCache
    {
        [SerializeField] private TriggerObserver _triggerObserver;
        [SerializeField] private Follow _follow;

        private Attack _attack;

        private void Awake() =>
            _attack = Get<Attack>();

        private void Start()
        {
            _triggerObserver.TriggerEnter += TriggerEnter;
            _triggerObserver.TriggerExit += TriggerExit;

            _attack.enabled = false;
        }

        public void Construct(float radius) =>
            _triggerObserver.GetComponent<SphereCollider>().radius = radius;

        private void TriggerEnter(Collider obj)
        {
            if (_follow != null)
            {
                _attack.enabled = true;
                _follow.Stop();
                _follow.enabled = false;
            }
        }

        private void TriggerExit(Collider obj)
        {
            if (_follow != null)
            {
                _attack.enabled = false;
                _follow.Move();
                _follow.enabled = true;
            }
        }
    }
}