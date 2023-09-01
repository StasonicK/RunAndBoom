using System;
using UnityEngine;

namespace CodeBase.Projectiles.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class BombMovement : ProjectileMovement
    {
        private Rigidbody Rigidbody { get; set; }
        private Vector3 _targetPosition;

        public override event Action Stoped;

        private void Awake() =>
            Rigidbody = Get<Rigidbody>();

        public void SetTargetPosition(Vector3 targetPosition) =>
            _targetPosition = targetPosition;

        public override void Launch()
        {
            StartCoroutine(LaunchTime());

            Rigidbody.isKinematic = false;
            Vector3 aim = _targetPosition - transform.position;
            float lenght = Vector3.Distance(_targetPosition, transform.position);
            float time = lenght / Speed;
            float antiGravity = -Physics.gravity.y * time / 2;
            float deltaY = (_targetPosition.y - transform.position.y) / time;
            Vector3 bombSpeed = aim.normalized * Speed;
            bombSpeed.y = antiGravity + deltaY;
            Rigidbody.velocity = bombSpeed;
            transform.forward = _targetPosition;
        }

        public override void Stop()
        {
            OffMove();
            Rigidbody.isKinematic = true;
            Stoped?.Invoke();
        }
    }
}