using UnityEngine;
using Starter.Core;

namespace Game.Portia
{
    public enum PlantingState { Empty, Growing, ReadyToHarvest }

    public class PlantingBox : MonoBehaviour, IInteractable
    {
        [SerializeField] CropData[] _crops;
        [SerializeField] float      _plantYOffset = 0.5f;

        PlantingState    _state      = PlantingState.Empty;
        CropData         _activeCrop;
        float            _timeLeft;
        float            _totalTime;
        GameObject       _plantModel;
        WorldProgressBar _bar;
        WorldHarvestBadge _badge;

        // ── IInteractable ──────────────────────────────────────────────────────

        public string PromptText => _state switch
        {
            PlantingState.ReadyToHarvest => "按 E 收获",
            PlantingState.Empty          => "按 E 种植",
            _                            => null,   // Growing：不显示 HUD 提示
        };

        public void Interact(GameObject initiator)
        {
            switch (_state)
            {
                case PlantingState.Empty:
                    PlantingPanel.Show(this, _crops);
                    break;
                case PlantingState.ReadyToHarvest:
                    Harvest();
                    break;
                // Growing: 忽略
            }
        }

        // ── Unity ──────────────────────────────────────────────────────────────

        void Update()
        {
            if (_state != PlantingState.Growing) return;

            _timeLeft -= Time.deltaTime;
            _bar?.SetFill(1f - _timeLeft / _totalTime);

            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                _state    = PlantingState.ReadyToHarvest;
                SwapToMatureModel();

                if (_bar != null) { Destroy(_bar.gameObject); _bar = null; }
                _badge = WorldHarvestBadge.Attach(transform, _plantYOffset + 1.5f);

                EventBus.Emit(new InteractTargetChangedEvent { Target = this });
            }
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void StartGrowing(CropData crop)
        {
            if (_badge != null) { Destroy(_badge.gameObject); _badge = null; }

            _activeCrop = crop;
            _state      = PlantingState.Growing;
            _timeLeft   = crop.growTime;
            _totalTime  = crop.growTime;

            SpawnPlantModel(crop.seedlingPrefab);

            _bar = WorldProgressBar.Attach(transform, new Color(0.3f, 0.85f, 0.25f), _plantYOffset + 1.5f);
            _bar.SetFill(0f);

            EventBus.Emit(new InteractTargetChangedEvent { Target = this });
        }

        public void Configure(CropData[] crops)
        {
            if (crops != null && crops.Length > 0) _crops = crops;
        }

        // ── Private ────────────────────────────────────────────────────────────

        void SpawnPlantModel(GameObject prefab)
        {
            if (_plantModel != null) { Destroy(_plantModel); _plantModel = null; }
            if (prefab == null) return;

            _plantModel = Instantiate(prefab,
                transform.position + Vector3.up * _plantYOffset,
                Quaternion.identity,
                transform);
        }

        void SwapToMatureModel()
        {
            if (_activeCrop?.maturePrefab != null)
                SpawnPlantModel(_activeCrop.maturePrefab);
        }

        void Harvest()
        {
            InventoryManager.Instance?.Add(_activeCrop.outputGid, _activeCrop.outputCount);
            EventBus.Emit(new ItemReceivedEvent
            {
                Gid = _activeCrop.outputGid,
                Count = _activeCrop.outputCount
            });
            Debug.Log($"[种植] 收获 {InventoryManager.GetItemName(_activeCrop.outputGid)} ×{_activeCrop.outputCount}");

            if (_badge != null) { Destroy(_badge.gameObject); _badge = null; }
            if (_plantModel != null) { Destroy(_plantModel); _plantModel = null; }
            _activeCrop = null;
            _state      = PlantingState.Empty;
            EventBus.Emit(new InteractTargetChangedEvent { Target = this });
        }
    }
}
