using System.Collections.Generic;
using UnityEngine;
using Starter.Core;

namespace Game.Portia
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        readonly Dictionary<int, int> _items = new Dictionary<int, int>();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Add(int gid, int count)
        {
            if (!_items.ContainsKey(gid)) _items[gid] = 0;
            _items[gid] += count;
            EventBus.Emit(new InventoryChangedEvent { Gid = gid, NewCount = _items[gid] });
        }

        public bool Remove(int gid, int count)
        {
            if (!_items.TryGetValue(gid, out var have) || have < count) return false;
            _items[gid] = have - count;
            EventBus.Emit(new InventoryChangedEvent { Gid = gid, NewCount = _items[gid] });
            return true;
        }

        public int GetCount(int gid)
        {
            _items.TryGetValue(gid, out var c);
            return c;
        }

        public IReadOnlyDictionary<int, int> AllItems => _items;

        public static string GetItemName(int gid)
        {
            switch ((ItemGid)gid)
            {
                case ItemGid.Wood:        return "木材";
                case ItemGid.Stone:       return "石块";
                case ItemGid.IronOre:     return "铁矿石";
                case ItemGid.Coal:        return "煤";
                case ItemGid.CopperOre:   return "铜矿石";
                case ItemGid.Mushroom:    return "蘑菇";
                case ItemGid.Plank:       return "木板";
                case ItemGid.IronIngot:   return "铁锭";
                case ItemGid.CopperIngot: return "铜锭";
                case ItemGid.SteelIngot:  return "钢锭";
                case ItemGid.CookingPot:  return "烹饪锅";
                case ItemGid.WheatSeed:   return "小麦种子";
                case ItemGid.Wheat:       return "小麦";
                case ItemGid.Food:        return "菜肴";
                case ItemGid.Fish:        return "鱼";
                case ItemGid.Coin:        return "金币";
                default:                  return $"道具({gid})";
            }
        }
    }
}
