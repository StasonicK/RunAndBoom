﻿using System.Collections;
using UnityEngine;

namespace CodeBase.Projectiles
{
    public class ProjectileBlast : MonoBehaviour
    {
        [SerializeField] private DestroyWithBlast _destroyWithBlast;
        [SerializeField] private ProjectileTrace _projectileTrace;
        [SerializeField] private ProjectileMovement _movement;

        private const float BlastDuration = 2f;

        private GameObject _blastVfxPrefab;
        private float _sphereRadius;
        private ParticleSystem _particleSystem;
        private GameObject _blastVfx;

        private void OnEnable() =>
            HideBlast();

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Environments") || other.gameObject.CompareTag("Floor"))
            {
                ShowBlast();

                StartCoroutine(DestroyBlast());

                // _destroyWithBlast.DestroyAllAround(_sphereRadius);
                _projectileTrace.DestroyTrace();
                _movement.Stop();
            }

            // if (other.gameObject.CompareTag("Hero"))
            // {
            //     HeroHealth heroHealth = collision.gameObject.GetComponent<HeroHealth>();
            //     heroHealth.TakeDamage(1);
            // }
        }

        public void Construct(GameObject blastVfxPrefab, float blastRadius)
        {
            _blastVfxPrefab = blastVfxPrefab;
            _sphereRadius = blastRadius;
        }

        private void ShowBlast()
        {
            Debug.Log("Blast!");
            if (_particleSystem == null)
            {
                _blastVfx = Instantiate(_blastVfxPrefab, transform.position, Quaternion.identity, null);
                _particleSystem = _blastVfx.GetComponent<ParticleSystem>();
            }
            else
            {
                _blastVfx.transform.position = transform.position;
            }

            _particleSystem.Play(true);
        }

        private void HideBlast() =>
            _particleSystem?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        private IEnumerator DestroyBlast()
        {
            yield return new WaitForSeconds(BlastDuration);
            HideBlast();
        }
    }
}