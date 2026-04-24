using UnityEngine;
using Starter.Core;

namespace Game.Portia
{
    public enum MachineState { Idle, Processing, ReadyToCollect }

    public class ProcessingMachine : MonoBehaviour, IInteractable
    {
        [SerializeField] string       _machineName = "加工机";
        [SerializeField] RecipeData[] _recipes;

        MachineState      _state       = MachineState.Idle;
        RecipeData        _activeRecipe;
        float             _timeLeft;
        float             _totalTime;
        WorldProgressBar  _bar;
        WorldHarvestBadge _badge;

        public string MachineName => _machineName;
        public RecipeData[] GetRecipes() => _recipes;

        // ── IInteractable ──────────────────────────────────────────────────────

        public string PromptText => _state switch
        {
            MachineState.ReadyToCollect => "按 E 收取成品",
            MachineState.Idle           => $"按 E 打开 [{_machineName}]",
            _                           => null,   // Processing：不显示 HUD 提示
        };

        public void Interact(GameObject initiator)
        {
            switch (_state)
            {
                case MachineState.Idle:
                    ProcessingMachinePanel.Show(this, _recipes);
                    break;
                case MachineState.ReadyToCollect:
                    CollectOutput();
                    break;
                // Processing: 忽略
            }
        }

        // ── Unity ──────────────────────────────────────────────────────────────

        void Update()
        {
            if (_state != MachineState.Processing) return;

            _timeLeft -= Time.deltaTime;
            _bar?.SetFill(1f - _timeLeft / _totalTime);

            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                _state    = MachineState.ReadyToCollect;

                if (_bar != null) { Destroy(_bar.gameObject); _bar = null; }
                _badge = WorldHarvestBadge.Attach(transform, 5f);

                EventBus.Emit(new InteractTargetChangedEvent { Target = this });
            }
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void StartProcessing(RecipeData recipe)
        {
            if (InventoryManager.Instance == null) return;
            if (recipe.inputs == null || recipe.inputs.Length == 0) return;

            foreach (var inp in recipe.inputs)
                if (InventoryManager.Instance.GetCount(inp.gid) < inp.count) return;

            foreach (var inp in recipe.inputs)
                InventoryManager.Instance.Remove(inp.gid, inp.count);

            if (_badge != null) { Destroy(_badge.gameObject); _badge = null; }

            _activeRecipe = recipe;
            _state        = MachineState.Processing;
            _timeLeft     = recipe.processTime;
            _totalTime    = recipe.processTime;

            _bar = WorldProgressBar.Attach(transform, new Color(0.15f, 0.75f, 0.95f), 5f);
            _bar.SetFill(0f);

            EventBus.Emit(new InteractTargetChangedEvent { Target = this });
        }

        public void Configure(string machineName, RecipeData[] recipes)
        {
            if (!string.IsNullOrEmpty(machineName)) _machineName = machineName;
            if (recipes != null) _recipes = recipes;
        }

        // ── Private ────────────────────────────────────────────────────────────

        void CollectOutput()
        {
            InventoryManager.Instance?.Add(_activeRecipe.outputGid, _activeRecipe.outputCount);
            Debug.Log($"[加工] {_machineName} → {InventoryManager.GetItemName(_activeRecipe.outputGid)} ×{_activeRecipe.outputCount}");

            if (_badge != null) { Destroy(_badge.gameObject); _badge = null; }
            _activeRecipe = null;
            _state        = MachineState.Idle;
            EventBus.Emit(new InteractTargetChangedEvent { Target = this });
        }
    }
}
