using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class RotateToHero : MonoCache
    {
        [SerializeField] private float _speed;

        private Transform _heroTransform;
        private Vector3 _directionToLook;

        protected override void Run()
        {
            if (_heroTransform)
                RotateTowardsHero();
        }

        public void Construct(Transform heroTransform) =>
            _heroTransform = heroTransform;

        private void RotateTowardsHero()
        {
            UpdatePositionToLookAt();
            transform.rotation = SmoothedRotation(transform.rotation, _directionToLook);
        }

        private void UpdatePositionToLookAt()
        {
            Vector3 positionDelta = _heroTransform.position - transform.position;
            _directionToLook = new Vector3(positionDelta.x, transform.position.y, positionDelta.z).normalized;
        }

        private Quaternion SmoothedRotation(Quaternion rotation, Vector3 positionToLook) =>
            Quaternion.Lerp(rotation, TargetRotation(positionToLook), SpeedFactor());

        private Quaternion TargetRotation(Vector3 position) =>
            Quaternion.LookRotation(position);

        private float SpeedFactor() =>
            _speed * Time.deltaTime;
    }
}