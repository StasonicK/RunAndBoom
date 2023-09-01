using System;
using CodeBase.Data;
using NTC.Global.Cache;
using UnityEngine;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(Collider))]
    public class TriggerObserver : MonoCache
    {
        public event Action<Collider> TriggerEnter;
        public event Action<Collider> TriggerExit;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareByTag(Constants.HeroTag))
                TriggerEnter?.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareByTag(Constants.HeroTag))
                TriggerExit?.Invoke(other);
        }
    }
}