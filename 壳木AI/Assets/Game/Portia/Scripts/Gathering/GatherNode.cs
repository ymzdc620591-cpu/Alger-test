using System;
using System.Collections;
using UnityEngine;
using Starter.Core;

namespace Game.Portia
{
    public class GatherNode : MonoBehaviour, IInteractable
    {
        [SerializeField] string       _promptText      = "按 F 采集";
        [SerializeField] float        _pressInterval   = 0.3f;
        [SerializeField] int          _requiredPresses = 3;
        [SerializeField] GatherDrop[] _drops;

        int   _pressCount;
        float _lastPressTime = -10f;
        bool  _done;

        public string PromptText => _pressCount > 0
            ? $"{_promptText}  [{_pressCount}/{_requiredPresses}]"
            : _promptText;

        public void Interact(GameObject initiator)
        {
            if (_done) return;
            if (Time.time - _lastPressTime < _pressInterval) return;

            _lastPressTime = Time.time;
            _pressCount++;

            // 刷新提示文字
            EventBus.Emit(new InteractTargetChangedEvent { Target = this });

            StopAllCoroutines();
            StartCoroutine(WobbleRoutine());

            if (_pressCount >= _requiredPresses)
                StartCoroutine(CompleteAfterWobble());
        }

        IEnumerator WobbleRoutine()
        {
            var originalRot = transform.localRotation;
            float duration = 0.25f, elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float angle = Mathf.Sin(elapsed / duration * Mathf.PI * 4f) * 6f;
                transform.localRotation = originalRot * Quaternion.Euler(0f, 0f, angle);
                yield return null;
            }
            transform.localRotation = originalRot;
        }

        IEnumerator CompleteAfterWobble()
        {
            yield return new WaitForSeconds(0.25f);
            _done = true;

            foreach (var d in _drops)
            {
                InventoryManager.Instance?.Add(d.Gid, d.Count);
                Debug.Log($"[采集] {gameObject.name} → {InventoryManager.GetItemName(d.Gid)} ×{d.Count}");
            }

            EventBus.Emit(new InteractTargetChangedEvent { Target = null });
            // TODO P1: 隐藏模型，启动 CD 重生计时
            gameObject.SetActive(false);
        }

        [Serializable]
        public struct GatherDrop
        {
            public int Gid;
            public int Count;
        }
    }
}
