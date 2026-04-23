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

        float _yaw;
        float _pitch = 45f;

        void Start()
        {
            // 鼠标状态由游戏流程（Bootstrap / MainMenu）统一管理，相机不在此处抢占
        }

        void LateUpdate()
        {
            if (_target == null) return;

            // 背包/UI 打开时（光标解锁）不响应鼠标旋转
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                _yaw   += Input.GetAxis("Mouse X") * _sensitivityX;
                _pitch -= Input.GetAxis("Mouse Y") * _sensitivityY;
                _pitch  = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
            }

            var pivot  = _target.position + Vector3.up * _heightOffset;
            var offset = Quaternion.Euler(_pitch, _yaw, 0f) * new Vector3(0f, 0f, -_distance);

            // 简单相机碰撞：射线从 pivot 向相机方向检测，避免穿墙
            var desiredPos = pivot + offset;
            var dir        = offset.normalized;
            if (Physics.Raycast(pivot, dir, out var hit, _distance, ~LayerMask.GetMask("Ignore Raycast")))
                transform.position = pivot + dir * Mathf.Max(0.5f, hit.distance - 0.15f);
            else
                transform.position = desiredPos;

            transform.LookAt(pivot);
        }
    }
}
