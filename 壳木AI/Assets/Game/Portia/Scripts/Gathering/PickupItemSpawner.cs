using System;
using UnityEngine;

namespace Game.Portia
{
    public class PickupItemSpawner : MonoBehaviour
    {
        [Serializable]
        public struct Entry
        {
            public GameObject prefab;
            public int        gid;
            public int        itemCount;
            public int        spawnCount;
            public float      scale;
        }

        [SerializeField] Entry[] _entries;
        [SerializeField] float   _spawnRadius  = 40f;
        [SerializeField] float   _minRadius    = 4f;
        [SerializeField] float   _raycastFromY = 200f;

        void Start()
        {
            var player = GameObject.FindWithTag("Player");
            var center = player != null ? player.transform.position : Vector3.zero;
            var mask   = ~LayerMask.GetMask("Player", "Ignore Raycast");

            foreach (var e in _entries)
                Spawn(e, center, mask);
        }

        void Spawn(Entry e, Vector3 center, int mask)
        {
            if (e.prefab == null) return;

            int spawned  = 0;
            int attempts = e.spawnCount * 10;

            while (spawned < e.spawnCount && attempts-- > 0)
            {
                var rand2d = UnityEngine.Random.insideUnitCircle * _spawnRadius;
                if (rand2d.sqrMagnitude < _minRadius * _minRadius) continue;

                var origin = new Vector3(center.x + rand2d.x, center.y + _raycastFromY, center.z + rand2d.y);
                if (!Physics.Raycast(origin, Vector3.down, out var hit, _raycastFromY * 2f, mask)) continue;

                var go = Instantiate(e.prefab, hit.point, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
                go.transform.localScale = Vector3.one * e.scale;

                EnsureTrigger(go);

                var pickup = go.GetComponent<PickupItem>() ?? go.AddComponent<PickupItem>();
                pickup.Init(e.gid, e.itemCount);

                spawned++;
            }

            if (spawned < e.spawnCount)
                Debug.LogWarning($"[PickupItemSpawner] {e.prefab.name}: 只生成了 {spawned}/{e.spawnCount}，地面射线未全部命中。");
        }

        static void EnsureTrigger(GameObject go)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>())
                if (c.isTrigger) return;

            var sc       = go.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius    = 0.6f;
            sc.center    = new Vector3(0f, 0.3f, 0f);
        }
    }
}
