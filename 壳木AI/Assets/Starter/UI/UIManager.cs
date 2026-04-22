using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using Starter.Core;
using Starter.Runtime;

namespace Starter.UI
{
    // 用法：UIManager.Inst.Init()（在 GameBootstrap 中调用一次）
    //       UIManager.Inst.PushPanel("UI/HUDPanel")
    //       UIManager.Inst.PushPanel<HUDPanel>("UI/HUDPanel")
    //       UIManager.Inst.PopPanel()
    public class UIManager
    {
        UIManager() { }
        public static UIManager Inst { get; } = new UIManager();

        readonly ExtStack<UIPanel> _panelStack = new();
        Transform _rootTrans;
        Camera _mainCamera;
        Camera _uiCamera;
        int _sortOrder;

        public Camera MainCamera => _mainCamera;
        public Camera UICamera   => _uiCamera;

        // ── 初始化 ─────────────────────────────────────────────────────────────

        public void Init()
        {
            // 优先从 Resources 加载 UIRoot 预制体，否则代码创建
            var rootGo = ResManager.InstantiateGameObjectSync("UI/UIRoot") ?? CreateUIRoot();
            Object.DontDestroyOnLoad(rootGo);
            _rootTrans = rootGo.transform;

            var uiCamTrans = rootGo.transform.Find("UICamera");
            _uiCamera = uiCamTrans != null
                ? uiCamTrans.GetComponent<Camera>()
                : CreateUICamera(_rootTrans);

            SceneManager.sceneLoaded += OnSceneLoaded;
            SetupURPStack();
            _sortOrder = 0;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) => SetupURPStack();

        void SetupURPStack()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null || _uiCamera == null) return;
            try
            {
                var mainData = _mainCamera.GetUniversalAdditionalCameraData();
                if (!mainData.cameraStack.Contains(_uiCamera))
                    mainData.cameraStack.Add(_uiCamera);
            }
            catch
            {
                // 非 URP 项目跳过相机叠加设置
            }
        }

        // ── Push / Pop ─────────────────────────────────────────────────────────

        public UIPanel PushPanel(string res)
        {
            var resGo = ResManager.LoadGameObjectSync(res);
            if (resGo == null) return null;

            var canvasPanel = InstantiateCanvasPanel();
            canvasPanel.name = resGo.name + "Canvas";
            canvasPanel.transform.SetParent(_rootTrans, false);

            var content = canvasPanel.transform.GetChild(0);
            var go      = Object.Instantiate(resGo, content, false);
            var panel   = go.GetComponent<UIPanel>();

            if (panel == null)
            {
                Debug.LogError($"[UIManager] {resGo.name} 上缺少 UIPanel 组件");
                Object.Destroy(canvasPanel);
                return null;
            }

            go.name = resGo.name;

            // 保持 prefab 中设置的 RectTransform 偏移
            if (go.transform is RectTransform rect && resGo.transform is RectTransform resRect)
            {
                rect.offsetMin = resRect.offsetMin;
                rect.offsetMax = resRect.offsetMax;
            }

            ApplySortOrder(canvasPanel.GetComponent<Canvas>());
            _panelStack.Push(panel);
            panel.FadeIn();
            return panel;
        }

        public T PushPanel<T>(string res) where T : UIPanel
            => PushPanel(res) as T;

        public void PopPanel()
        {
            if (_panelStack.Count == 0) return;
            var panel = _panelStack.Pop();
            panel.OnPop();
            panel.FadeOut();
        }

        public void PopPanel(UIPanel panel)
        {
            _panelStack.Pop(panel);
            panel.OnPop();
            panel.FadeOut();
        }

        public void PopAllPanels()
        {
            while (_panelStack.Count > 0)
            {
                var panel = _panelStack.Pop();
                panel.OnPop();
                panel.FadeOut();
            }
        }

        // ── 查询 ───────────────────────────────────────────────────────────────

        public UIPanel GetTopPanel() => _panelStack.Peek();

        public bool CheckHavePanel(string panelName)
        {
            for (int i = 0; i < _panelStack.Count; i++)
                if (_panelStack.CheckByIndex(i).name == panelName)
                    return true;
            return false;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[UIManager] stackCount={_panelStack.Count}");
            return sb.ToString();
        }

        // ── 内部工具 ──────────────────────────────────────────────────────────

        void ApplySortOrder(Canvas canvas)
        {
            if (canvas == null) return;
            _sortOrder += 10;
            canvas.sortingOrder = _sortOrder;
            if (_uiCamera != null) canvas.worldCamera = _uiCamera;
        }

        // 从 Resources 加载 CanvasPanel 模板，否则代码创建
        // 层级：CanvasPanel(Canvas) → Content(RectTransform，全拉伸)
        GameObject InstantiateCanvasPanel()
        {
            var template = ResManager.LoadGameObjectSync("UI/CanvasPanel");
            if (template != null) return Object.Instantiate(template);

            var root = new GameObject("CanvasPanel");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution  = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight   = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            var content = new GameObject("Content");
            content.transform.SetParent(root.transform, false);
            var rect = content.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return root;
        }

        GameObject CreateUIRoot()
        {
            return new GameObject("[UIRoot]");
        }

        Camera CreateUICamera(Transform parent)
        {
            var go  = new GameObject("UICamera");
            go.transform.SetParent(parent, false);
            var cam = go.AddComponent<Camera>();
            cam.clearFlags   = CameraClearFlags.Depth;
            cam.cullingMask  = LayerMask.GetMask("UI");
            cam.orthographic = false;
            cam.depth        = 100f;
            try
            {
                var data = cam.GetUniversalAdditionalCameraData();
                data.renderType = CameraRenderType.Overlay;
            }
            catch { /* 非 URP 项目跳过 */ }
            return cam;
        }
    }
}
