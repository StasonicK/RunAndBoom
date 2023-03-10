using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroMovement : MonoBehaviour
    {
        // private IPlatformInputService _platformInputService;

        private float _moveSpeed = 5f;
        private Vector3 _movement = Vector3.zero;

        private void Update()
        {
            // transform.Translate(_movement * _moveSpeed * Time.deltaTime);

            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            transform.Translate(new Vector3(horizontalInput, 0, verticalInput) * _moveSpeed * Time.deltaTime);
        }

        private void Awake()
        {
            // _platformInputService = AllServices.Container.Single<IPlatformInputService>();
            // _platformInputService.Moved += MoveTo;
        }

        private void MoveTo(Vector2 direction) =>
            _movement = new Vector3(direction.x, 0, direction.y);
    }
}