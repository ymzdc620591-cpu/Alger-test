using UnityEngine;
using Game.System;
using Game.Player;
using Starter.Core;

namespace Game.Portia
{
    public class InteractionDetector : MonoBehaviour
    {
        [SerializeField] float   _radius      = 3.5f;
        const            KeyCode InteractKey  = KeyCode.E;

        IInteractable _current;

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

            DetectNearest();

            if (_current != null && Input.GetKeyDown(InteractKey))
            {
                _current.Interact(gameObject);
                if (_current is GatherNode)
                    GetComponent<PlayerController>()?.PlayAttack();
            }
        }

        void DetectNearest()
        {
            IInteractable best     = null;
            float         bestDist = float.MaxValue;

            var hits = Physics.OverlapSphere(transform.position, _radius, ~0, QueryTriggerInteraction.Collide);
            foreach (var col in hits)
            {
                var candidate = col.GetComponentInParent<IInteractable>();
                if (candidate == null) continue;

                float d = Vector3.Distance(transform.position, col.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best     = candidate;
                }
            }

            if (best == _current) return;

            _current = best;
            EventBus.Emit(new InteractTargetChangedEvent { Target = _current });
        }
    }
}
