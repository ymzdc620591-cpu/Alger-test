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
        [SerializeField] bool         _fallOnGather    = false;
        [SerializeField] float        _barYOffset      = 7f;
        [SerializeField] GatherDrop[] _drops;

        int               _pressCount;
        float             _lastPressTime  = -10f;
        bool              _done;
        Vector3           _lastInitiatorPos;
        WorldProgressBar  _bar;

        public string PromptText => _pressCount > 0
            ? $"{_promptText}  [{_pressCount}/{_requiredPresses}]"
            : _promptText;

        public void Interact(GameObject initiator)
        {
            if (_done) return;
            if (Time.time - _lastPressTime < _pressInterval) return;

            if (initiator != null) _lastInitiatorPos = initiator.transform.position;
            _lastPressTime = Time.time;
            _pressCount++;

            if (_bar == null)
                _bar = WorldProgressBar.Attach(transform, new Color(0.95f, 0.75f, 0.1f), _barYOffset);
            _bar.SetFill((float)_pressCount / _requiredPresses);

            EventBus.Emit(new InteractTargetChangedEvent { Target = this });

            StopAllCoroutines();
            StartCoroutine(WobbleRoutine());

            if (_pressCount >= _requiredPresses)
                StartCoroutine(CompleteAfterWobble());
        }

        IEnumerator WobbleRoutine()
        {
            var pivot       = transform.position; // 手动摆放，transform.position 即为底部锚点
            var originalRot = transform.rotation;
            var axis        = transform.right;

            float duration = 0.25f, elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float angle = Mathf.Sin(elapsed / duration * Mathf.PI * 4f) * 6f;
                transform.SetPositionAndRotation(pivot, originalRot);
                transform.RotateAround(pivot, axis, angle);
                yield return null;
            }
            transform.SetPositionAndRotation(pivot, originalRot);
        }

        IEnumerator CompleteAfterWobble()
        {
            yield return new WaitForSeconds(0.25f);
            _done = true;

            if (_bar != null) { Destroy(_bar.gameObject); _bar = null; }

            foreach (var d in _drops)
            {
                InventoryManager.Instance?.Add(d.Gid, d.Count);
                Debug.Log($"[采集] {gameObject.name} → {InventoryManager.GetItemName(d.Gid)} ×{d.Count}");
            }

            EventBus.Emit(new InteractTargetChangedEvent { Target = null });

            if (_fallOnGather)
                yield return StartCoroutine(FallAndDisappear());
            else
                gameObject.SetActive(false);
        }

        IEnumerator FallAndDisappear()
        {
            var fallDir = (transform.position - _lastInitiatorPos);
            fallDir.y = 0f;
            if (fallDir.sqrMagnitude < 0.01f) fallDir = transform.forward;
            fallDir.Normalize();

            var rotAxis  = Vector3.Cross(Vector3.up, fallDir).normalized;
            var startPos = transform.position;
            var startRot = transform.rotation;

            float duration = 1f, elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float eased = 1f - Mathf.Pow(1f - elapsed / duration, 2f);
                transform.SetPositionAndRotation(startPos, startRot);
                transform.RotateAround(startPos, rotAxis, 88f * eased);
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
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
