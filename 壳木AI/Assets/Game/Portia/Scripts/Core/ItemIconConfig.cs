using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Portia
{
    [CreateAssetMenu(fileName = "ItemIconConfig", menuName = "Game/ItemIconConfig")]
    public class ItemIconConfig : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public int    gid;
            public Sprite icon;
        }

        public List<Entry> entries = new();

        Dictionary<int, Sprite> _map;

        void OnEnable() => _map = null; // 进入 PlayMode 时重建

        public Sprite GetIcon(int gid)
        {
            _map ??= BuildMap();
            return _map.TryGetValue(gid, out var s) ? s : null;
        }

        Dictionary<int, Sprite> BuildMap()
        {
            var d = new Dictionary<int, Sprite>(entries.Count);
            foreach (var e in entries)
                if (e.icon != null) d[e.gid] = e.icon;
            return d;
        }
    }
}
