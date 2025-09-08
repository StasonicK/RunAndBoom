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
            if (_trailVfx == null)
                _trailVfx = Instantiate(prefab, _trailPosition.position, Quaternion.identity, _trailPosition);
        }

        public void ShowTrail()
        {
            if (!isActiveAndEnabled)
                return;

            StartCoroutine(CoroutineShowTrace());
        }

        private IEnumerator CoroutineShowTrace()
        {
            if (_trailVfx != null)
            {
                var wait = _coroutineShowTrace ?? new WaitForSeconds(0f);
                yield return wait;
                _trailVfx.SetActive(true);
            }
        }

        public void HideTrace()
        {
            if (!isActiveAndEnabled)
            {
                // If this component or its GameObject is inactive, perform immediate hide to avoid StartCoroutine on inactive object.
                Hide();
                return;
            }

            StartCoroutine(CoroutineHideTrace());
        }

        private IEnumerator CoroutineHideTrace()
        {
            var wait = _coroutineHideTrace ?? new WaitForSeconds(0f);
            yield return wait;
            Hide();
        }

        private void Hide()
        {
            if (_trailVfx != null)
                _trailVfx.SetActive(false);
        }
    }
}
