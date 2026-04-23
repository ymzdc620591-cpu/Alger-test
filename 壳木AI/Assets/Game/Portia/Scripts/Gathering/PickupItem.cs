using UnityEngine;
using Starter.Core;

namespace Game.Portia
{
    public class PickupItem : MonoBehaviour, IInteractable
    {
        [SerializeField] int _gid   = 1;
        [SerializeField] int _count = 1;

        public string PromptText =>
            $"[E] 拾取 {InventoryManager.GetItemName(_gid)} ×{_count}";

        public void Interact(GameObject initiator)
        {
            InventoryManager.Instance?.Add(_gid, _count);
            EventBus.Emit(new InteractTargetChangedEvent { Target = null });
            Destroy(gameObject);
        }

        // 供 Editor 脚本初始化
        public void Init(int gid, int count)
        {
            _gid   = gid;
            _count = count;
        }
    }
}
