using UnityEngine;
using Game.System;
using Starter.Core;

namespace Game.Portia
{
    public class InteractionDetector : MonoBehaviour
    {
        [SerializeField] float   _maxDistance = 3.5f;
        [SerializeField] KeyCode _interactKey = KeyCode.F;

        Camera        _cam;
        IInteractable _current;

        void Awake() => _cam = Camera.main;

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

            DetectTarget();

            if (_current != null && Input.GetKeyDown(_interactKey))
                _current.Interact(gameObject);
        }

        void DetectTarget()
        {
            IInteractable found = null;

            if (_cam != null)
            {
                var ray = new Ray(_cam.transform.position, _cam.transform.forward);
                if (Physics.Raycast(ray, out var hit, _maxDistance + 8f, ~0, QueryTriggerInteraction.Collide))
                {
                    if (Vector3.Distance(transform.position, hit.point) <= _maxDistance)
                        found = hit.collider.GetComponentInParent<IInteractable>();
                }
            }

            if (found == _current) return;

            _current = found;
            EventBus.Emit(new InteractTargetChangedEvent { Target = _current });
        }
    }
}
