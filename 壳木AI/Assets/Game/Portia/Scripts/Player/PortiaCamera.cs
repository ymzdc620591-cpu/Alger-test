using UnityEngine;

namespace Game.Portia
{
    public class PortiaCamera : MonoBehaviour
    {
        [SerializeField] Transform _target;
        [SerializeField] float _distance     = 12f;
        [SerializeField] float _minPitch     = 20f;
        [SerializeField] float _maxPitch     = 75f;
        [SerializeField] float _sensitivityX =  3f;
        [SerializeField] float _sensitivityY =  2f;
        [SerializeField] float _heightOffset = 1.4f;

        [SerializeField] float _collisionRadius      = 0.25f;
        [SerializeField] float _collisionSmoothSpeed = 10f;

        float _yaw;
        float _pitch = 45f;
        Vector3 _currentCollisionOffset;
        readonly RaycastHit[] _collisionHits = new RaycastHit[8];

        void Start()
        {
            // Cursor lock is managed by the game flow.
        }

        void LateUpdate()
        {
            if (_target == null) return;

            // Ignore mouse look while UI has unlocked the cursor.
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                _yaw   += Input.GetAxis("Mouse X") * _sensitivityX;
                _pitch -= Input.GetAxis("Mouse Y") * _sensitivityY;
                _pitch  = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
            }

            var pivot  = _target.position + Vector3.up * _heightOffset;
            var offset = Quaternion.Euler(_pitch, _yaw, 0f) * new Vector3(0f, 0f, -_distance);

            // Ignore self hits so sprinting does not cause camera collision flicker.
            var desiredPos = pivot + offset;
            var dir        = offset.normalized;
            float targetOffset = 0f;
            float nearestDistance = _distance;
            bool blocked = false;

            int hitCount = Physics.SphereCastNonAlloc(
                pivot,
                _collisionRadius,
                dir,
                _collisionHits,
                _distance,
                ~LayerMask.GetMask("Ignore Raycast"),
                QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                var hit = _collisionHits[i];
                if (hit.collider == null)
                    continue;

                if (hit.collider.transform == _target || hit.collider.transform.IsChildOf(_target))
                    continue;

                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    blocked = true;
                }
            }

            if (blocked)
                targetOffset = Mathf.Max(0.5f, nearestDistance - 0.15f) - _distance;

            // Smooth the collision offset so the camera settles instead of vibrating.
            _currentCollisionOffset = Vector3.Lerp(
                _currentCollisionOffset,
                dir * targetOffset,
                _collisionSmoothSpeed * Time.deltaTime);

            transform.position = desiredPos + _currentCollisionOffset;
            transform.LookAt(pivot);
        }
    }
}
