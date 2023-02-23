using UnityEngine;

namespace CodeBase.Enemy
{
    public class RotateToHero : MonoBehaviour
    {
        [SerializeField] private float _speed;

        private Transform _heroTransform;
        private Vector3 _positionToLook;

        private void Update()
        {
            if (_heroTransform)
                RotateTowardsHero();
        }

        public void Construct(Transform heroTransform) =>
            _heroTransform = heroTransform;

        private void RotateTowardsHero()
        {
            UpdatePositionToLookAt();

            transform.rotation = SmoothedRotation(transform.rotation, _positionToLook);
        }

        private void UpdatePositionToLookAt()
        {
            Vector3 positionDelta = _heroTransform.position - transform.position;
            _positionToLook = new Vector3(positionDelta.x, transform.position.y, positionDelta.z);
        }

        private Quaternion SmoothedRotation(Quaternion rotation, Vector3 positionToLook) =>
            Quaternion.Lerp(rotation, TargetRotation(positionToLook), SpeedFactor());

        private Quaternion TargetRotation(Vector3 position) =>
            Quaternion.LookRotation(position);

        private float SpeedFactor() =>
            _speed * Time.deltaTime;
    }
}