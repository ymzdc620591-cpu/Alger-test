using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Starter.Runtime
{
    // 资源路径驱动的 GameObject 对象池，参考 CamelHotCLR/PoolManager
    // 注意：使用完毕必须归还，否则会内存泄漏
    //
    // 用法（按名称）：
    //   var go = PoolManager.Get("Game/Prefabs/Bullet", parent);
    //   PoolManager.Release("Game/Prefabs/Bullet", go);
    //
    // 用法（PoolItem，推荐——自动携带名称）：
    //   var item = PoolManager.GetPoolItem("Game/Prefabs/Bullet", parent);
    //   PoolManager.Release(item);
    public static class PoolManager
    {
        // 对 GameObject 及其资源名的包裹，方便归还时不用手动传名称
        public class PoolItem
        {
            public GameObject Obj  { get; set; }
            public string     Name { get; set; }
        }

        static readonly Dictionary<string, UnityEngine.Pool.ObjectPool<GameObject>> _pools = new();
        static UnityEngine.Pool.ObjectPool<GameObject> _emptyGOPool;
        static Transform _poolRoot;

        // ── 按资源名称取/还 ──────────────────────────────────────────────────

        public static GameObject Get(string name, Transform parent = null)
        {
            if (!_pools.TryGetValue(name, out var pool))
            {
                pool = new UnityEngine.Pool.ObjectPool<GameObject>(
                    createFunc:      () => ResManager.InstantiateGameObjectSync(name),
                    actionOnGet:     OnGet,
                    actionOnRelease: OnRelease,
                    actionOnDestroy: obj => Object.Destroy(obj),
                    collectionCheck: true
                );
                _pools.Add(name, pool);
            }

            var go = pool.Get();
            go.transform.SetParent(parent, false);
            return go;
        }

        public static void Release(string name, GameObject obj)
        {
            if (_pools.TryGetValue(name, out var pool))
                pool.Release(obj);
        }

        // ── PoolItem（推荐接口）───────────────────────────────────────────────

        public static PoolItem GetPoolItem(string name, Transform parent = null)
        {
            var go   = Get(name, parent);
            var item = GenericPool<PoolItem>.Get();
            item.Obj  = go;
            item.Name = name;
            return item;
        }

        public static void Release(PoolItem item)
        {
            Release(item.Name, item.Obj);
            GenericPool<PoolItem>.Release(item);
        }

        // ── 空 GameObject 池 ─────────────────────────────────────────────────

        public static GameObject GetEmptyGO()
        {
            _emptyGOPool ??= new UnityEngine.Pool.ObjectPool<GameObject>(
                createFunc:      () => new GameObject(),
                actionOnGet:     OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: obj => Object.Destroy(obj),
                collectionCheck: true
            );
            return _emptyGOPool.Get();
        }

        public static void ReleaseEmptyGO(GameObject obj) => _emptyGOPool.Release(obj);

        // ── 清理 ─────────────────────────────────────────────────────────────

        public static void Clear()
        {
            _emptyGOPool?.Clear();
            foreach (var pool in _pools.Values)
                pool.Clear();
            _pools.Clear();
        }

        // ── 内部回调 ──────────────────────────────────────────────────────────

        static void OnGet(GameObject obj)
        {
            obj.transform.localScale    = Vector3.one;
            obj.transform.localRotation = Quaternion.identity;
            obj.SetActive(true);
        }

        // 归还时挂到 PoolRoot 下隐藏，避免对象散落在场景
        static void OnRelease(GameObject obj)
        {
            EnsurePoolRoot();
            obj.transform.SetParent(_poolRoot);
            obj.SetActive(false);
        }

        static void EnsurePoolRoot()
        {
            if (_poolRoot != null) return;
            var go = new GameObject("[PoolRoot]");
            Object.DontDestroyOnLoad(go);
            _poolRoot = go.transform;
        }
    }
}
