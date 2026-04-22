using System.Collections.Generic;
using UnityEngine;

namespace Starter.Runtime
{
    // 用法：var pool = new ObjectPool<Bullet>(bulletPrefab, preload: 10);
    //       var b = pool.Get(pos, rot);   pool.Return(b);
    public class ObjectPool<T> where T : Component
    {
        readonly T _prefab;
        readonly Transform _parent;
        readonly Queue<T> _pool = new();

        public int CountInactive => _pool.Count;

        public ObjectPool(T prefab, int preload = 0, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < preload; i++)
                _pool.Enqueue(Create());
        }

        public T Get(Vector3 position = default, Quaternion rotation = default)
        {
            var obj = _pool.Count > 0 ? _pool.Dequeue() : Create();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }

        T Create()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            return obj;
        }
    }
}
