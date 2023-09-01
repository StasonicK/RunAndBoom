using System;
using UnityEngine;

namespace CodeBase.Projectiles.Movement
{
    public class ShotMovement : ProjectileMovement
    {
        public override event Action Stoped;

        protected override void Run()
        {
            if (IsMove)
                transform.position += transform.forward * Speed * Time.deltaTime;
        }

        public override void Launch()
        {
            StartCoroutine(LaunchTime());
            IsMove = true;
        }

        public override void Stop()
        {
            OffMove();
            Stoped?.Invoke();
        }
    }
}