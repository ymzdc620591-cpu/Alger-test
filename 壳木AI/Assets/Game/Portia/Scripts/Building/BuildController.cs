using UnityEngine;
using Game.System;

namespace Game.Portia
{
    [global::System.Serializable]
    public struct MachineEntry
    {
        [Tooltip("面板显示名称")]                          public string       displayName;
        [Tooltip("面板说明文字")]                          public string       description;
        [Tooltip("3D模型Prefab")]                          public GameObject   prefab;
        [Tooltip("设置到ProcessingMachine的机器名")]       public string       machineName;
        [Tooltip("放置缩放（默认3，使模型与角色等高）")]  public float        scale;
        [Tooltip("该机器支持的配方列表")]                  public RecipeData[] recipes;
    }

    /// <summary>
    /// 挂在 Player 上。B 键打开选择面板，选择后进入放置模式。
    /// 左键确认放置，ESC 取消。
    /// </summary>
    public class BuildController : MonoBehaviour
    {
        [Header("可建造机器（在 Inspector 中拖入 Prefab）")]
        [SerializeField] MachineEntry[] _machines;

        [Header("放置参数")]
        [SerializeField] LayerMask _groundMask      = -1;
        [SerializeField] float     _maxPlaceDistance = 20f;

        enum BuildState { Idle, Selecting, Placing }

        BuildState          _state = BuildState.Idle;
        BuildingSelectPanel _panel;
        GameObject          _previewGo;
        int                 _selectedIdx;
        Material            _ghostMat;
        float               _groundOffset;   // pivot 到模型底部的距离（世界单位）

        // ── Unity ──────────────────────────────────────────────────────────────

        void Awake()
        {
            _ghostMat = MakeGhostMat();

            var panelGo = new GameObject("[BuildingSelectPanel]");
            panelGo.transform.SetParent(transform, false);
            _panel = panelGo.AddComponent<BuildingSelectPanel>();
            _panel.Init(_machines, OnMachineSelected);
            _panel.Hide();
        }

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

            switch (_state)
            {
                case BuildState.Idle:
                    // 只在鼠标锁定（无其他UI）时响应 B 键
                    if (Input.GetKeyDown(KeyCode.B) && Cursor.lockState == CursorLockMode.Locked)
                        OpenPanel();
                    break;

                case BuildState.Selecting:
                    if (Input.GetKeyDown(KeyCode.Escape))
                        ClosePanel();
                    break;

                case BuildState.Placing:
                    UpdatePreview();
                    if (Input.GetMouseButtonDown(0))
                        ConfirmPlace();
                    else if (Input.GetKeyDown(KeyCode.Escape))
                        CancelPlace();
                    break;
            }
        }

        // ── State: Selecting ──────────────────────────────────────────────────

        void OpenPanel()
        {
            _state = BuildState.Selecting;
            _panel.Show();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        void ClosePanel()
        {
            _state = BuildState.Idle;
            _panel.Hide();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }

        void OnMachineSelected(int idx)
        {
            _selectedIdx = idx;
            _panel.Hide();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            _state = BuildState.Placing;
            SpawnPreview();
        }

        // ── State: Placing ────────────────────────────────────────────────────

        void SpawnPreview()
        {
            var entry = _machines[_selectedIdx];
            if (entry.prefab == null) { _state = BuildState.Idle; return; }

            _previewGo      = Instantiate(entry.prefab);
            _previewGo.name = "[BuildPreview]";
            float s = entry.scale > 0f ? entry.scale : 1f;
            _previewGo.transform.localScale = Vector3.one * s;

            // 禁用所有脚本和碰撞体，避免干扰游戏逻辑
            foreach (var mb in _previewGo.GetComponentsInChildren<MonoBehaviour>())
                mb.enabled = false;
            foreach (var col in _previewGo.GetComponentsInChildren<Collider>())
                col.enabled = false;

            // 替换所有材质为半透明蓝色预览材质
            foreach (var mr in _previewGo.GetComponentsInChildren<MeshRenderer>())
            {
                var mats = new Material[mr.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = _ghostMat;
                mr.sharedMaterials = mats;
            }

            // 将预览体暂置于原点，测量包围盒底部到轴心的距离
            // 这样 UpdatePreview 可以把模型底部精确贴到地面命中点
            _previewGo.transform.position = Vector3.zero;
            float minY = 0f;
            foreach (var mr in _previewGo.GetComponentsInChildren<MeshRenderer>())
                minY = Mathf.Min(minY, mr.bounds.min.y);
            _groundOffset = -minY;   // minY < 0 时为正，表示需要抬高的量

            UpdatePreview();         // 立即定位，避免第一帧闪到原点
        }

        void UpdatePreview()
        {
            if (_previewGo == null) return;
            var cam = Camera.main;
            if (cam == null) return;

            var ray = new Ray(cam.transform.position, cam.transform.forward);
            Vector3 targetPos;

            if (Physics.Raycast(ray, out var hit, _maxPlaceDistance, _groundMask, QueryTriggerInteraction.Ignore)
                && hit.normal.y > 0.3f)
            {
                // 准星命中地面 / 水平面
                targetPos = hit.point;
            }
            else
            {
                // 准星未命中：将射线投影到玩家脚下的水平面
                float groundY = transform.position.y;
                float denom   = ray.direction.y;
                if (Mathf.Abs(denom) > 0.001f)
                {
                    float t = (groundY - ray.origin.y) / denom;
                    targetPos = t > 0f
                        ? ray.origin + ray.direction * Mathf.Min(t, _maxPlaceDistance)
                        : transform.position + transform.forward * 2f;
                }
                else
                {
                    targetPos = transform.position + transform.forward * 2f;
                }
            }

            _previewGo.transform.position = targetPos + Vector3.up * _groundOffset;
            _previewGo.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        }

        void ConfirmPlace()
        {
            if (_previewGo == null) return;

            var pos   = _previewGo.transform.position;
            var rot   = _previewGo.transform.rotation;
            var entry = _machines[_selectedIdx];

            Destroy(_previewGo);
            _previewGo = null;

            if (entry.prefab != null)
            {
                var placed = Instantiate(entry.prefab, pos, rot);
                float s = entry.scale > 0f ? entry.scale : 1f;
                placed.transform.localScale = Vector3.one * s;
                SetupPlacedMachine(placed, entry);
            }

            _state = BuildState.Idle;
        }

        void CancelPlace()
        {
            Destroy(_previewGo);
            _previewGo = null;
            _state     = BuildState.Idle;
        }

        // ── Post-placement ────────────────────────────────────────────────────

        void SetupPlacedMachine(GameObject go, MachineEntry entry)
        {
            // 如果 Prefab 没有 ProcessingMachine，自动添加
            var pm = go.GetComponentInChildren<ProcessingMachine>();
            if (pm == null) pm = go.AddComponent<ProcessingMachine>();
            pm.Configure(
                string.IsNullOrEmpty(entry.machineName) ? entry.displayName : entry.machineName,
                entry.recipes != null && entry.recipes.Length > 0 ? entry.recipes : null
            );

            // 如果 Prefab 没有任何 Collider，添加 BoxCollider 供交互射线检测
            if (go.GetComponentInChildren<Collider>() == null)
                go.AddComponent<BoxCollider>();
        }

        // ── Ghost Material ────────────────────────────────────────────────────

        static Material MakeGhostMat()
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.3f, 0.8f, 1f, 0.45f);
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            return mat;
        }
    }
}
