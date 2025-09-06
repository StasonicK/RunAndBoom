using System.Collections;
using CodeBase.StaticData.Projectiles;
using UnityEngine;

namespace CodeBase.Projectiles
{
    public class ProjectileTrail : MonoBehaviour
    {
        [SerializeField] private Transform _trailPosition;
        [SerializeField] private GameObject _trailVfx;

        private float _startDelay;
        private float _endDelay;
        private ParticleSystem _particleSystem;
        private WaitForSeconds _coroutineShowTrace;
        private WaitForSeconds _coroutineHideTrace;

        private void OnEnable() =>
            Hide();

        public void Construct(TrailStaticData trailStaticData)
        {
            _startDelay = trailStaticData.StartDelay;
            _endDelay = trailStaticData.EndDelay;

            if (_coroutineShowTrace == null)
                _coroutineShowTrace = new WaitForSeconds(_startDelay);

            if (_coroutineHideTrace == null)
                _coroutineHideTrace = new WaitForSeconds(_endDelay);

            CreateTrailVfx(trailStaticData.Prefab);
        }

        private void CreateTrailVfx(GameObject prefab)
        {
            // Debug.Log($"trailVfx {_trailVfx}");
            if (_trailVfx == null)
                _trailVfx = Instantiate(prefab, _trailPosition.position, Quaternion.identity, _trailPosition);
        }

        public void ShowTrail() =>
            StartCoroutine(CoroutineShowTrace());

        private IEnumerator CoroutineShowTrace()
        {
            if (_trailVfx != null)
            {
                // if (_particleSystem == null) 
                //     _particleSystem = _trailVfx.GetComponent<ParticleSystem>();

                yield return _coroutineShowTrace;
                _trailVfx.SetActive(true);
                // _particleSystem?.Play(true);
            }
        }

        public void HideTrace()
        {
            StartCoroutine(CoroutineHideTrace());
        }

        private IEnumerator CoroutineHideTrace()
        {
            yield return _coroutineHideTrace;
            Hide();
        }

        private void Hide()
        {
            if (_trailVfx != null)
                _trailVfx.SetActive(false);
            // _particleSystem?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}