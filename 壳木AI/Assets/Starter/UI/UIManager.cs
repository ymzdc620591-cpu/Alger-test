using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Starter.Core;
using Starter.Runtime;
using Object = UnityEngine.Object;

namespace Starter.UI
{
    // 用法：UIManager.Inst.Init()（在 GameBootstrap 中调用一次）
    //       UIManager.Inst.PushPanel("UI/HUDPanel")           —— 从 Resources 加载的 UIPanel
    //       UIManager.Inst.PushExternal(CloseCallback)        —— 外部自建 Canvas 面板（返回应使用的 sortingOrder）
    //       UIManager.Inst.PopPanel()                         —— 关闭栈顶面板（UIPanel 或外部）
    //       UIManager.Inst.HasAnyPanel()                      —— 是否有任意面板在栈中
    public class UIManager
    {
        UIManager() { }
        public static UIManager Inst { get; } = new UIManager();

        sealed class PanelEntry
        {
            public UIPanel Panel;       // UIPanel 面板；null 表示外部面板
            public Action  CloseAction; // 外部面板的关闭回调
            public bool IsExternal => Panel == null;
        }

        readonly ExtStack<PanelEntry> _stack = new();
        Transform _rootTrans;
        Camera _mainCamera;
        Camera _uiCamera;
        int _sortOrder;
        bool _initialized;

        public Camera MainCamera => _mainCamera;
        public Camera UICamera   => _uiCamera;

        // ── 初始化 ─────────────────────────────────────────────────────────────

        public void Init()
        {
            if (_initialized) return;
            _initialized = true;
            var rootGo = ResManager.TryLoadGameObject("UI/UIRoot") is {} r ? Object.Instantiate(r) : CreateUIRoot();
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

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) => _mainCamera = Camera.main;

        void SetupURPStack()
        {
            _mainCamera = Camera.main;
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

            if (go.transform is RectTransform rect && resGo.transform is RectTransform resRect)
            {
                rect.offsetMin = resRect.offsetMin;
                rect.offsetMax = resRect.offsetMax;
            }

            _sortOrder += 10;
            var canvas = canvasPanel.GetComponent<Canvas>();
            if (canvas != null) canvas.sortingOrder = _sortOrder;

            _stack.Push(new PanelEntry { Panel = panel });
            panel.FadeIn();
            return panel;
        }

        public T PushPanel<T>(string res) where T : UIPanel
            => PushPanel(res) as T;

        // 注册外部自建 Canvas 面板，返回该面板应使用的 Canvas.sortingOrder
        public int PushExternal(Action onClose)
        {
            _sortOrder += 10;
            _stack.Push(new PanelEntry { CloseAction = onClose });
            return _sortOrder;
        }

        public void PopPanel()
        {
            if (_stack.Count == 0) return;
            var entry = _stack.Pop();
            if (entry.IsExternal)
                entry.CloseAction?.Invoke();
            else
            {
                entry.Panel.OnPop();
                entry.Panel.FadeOut();
            }
        }

        public void PopPanel(UIPanel panel)
        {
            for (int i = 0; i < _stack.Count; i++)
            {
                var entry = _stack.CheckByIndex(i);
                if (entry.Panel == panel)
                {
                    _stack.Pop(entry);
                    panel.OnPop();
                    panel.FadeOut();
                    return;
                }
            }
        }

        public void PopAllPanels()
        {
            while (_stack.Count > 0)
            {
                var entry = _stack.Pop();
                if (entry.IsExternal)
                    entry.CloseAction?.Invoke();
                else
                {
                    entry.Panel.OnPop();
                    entry.Panel.FadeOut();
                }
            }
        }

        // ── 查询 ───────────────────────────────────────────────────────────────

        // 仅返回栈顶的 UIPanel（若栈顶是外部面板则返回 null）
        public UIPanel GetTopPanel() => _stack.Peek()?.Panel;

        // 栈中是否有任何面板（UIPanel 或外部）
        public bool HasAnyPanel() => _stack.Count > 0;

        public bool CheckHavePanel(string panelName)
        {
            for (int i = 0; i < _stack.Count; i++)
            {
                var entry = _stack.CheckByIndex(i);
                if (entry.Panel != null && entry.Panel.name == panelName)
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"[UIManager] stackCount={_stack.Count}");
            return sb.ToString();
        }

        // ── 内部工具 ──────────────────────────────────────────────────────────

        GameObject InstantiateCanvasPanel()
        {
            var template = ResManager.TryLoadGameObject("UI/CanvasPanel");
            if (template != null) return Object.Instantiate(template);

            var root = new GameObject("CanvasPanel");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

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
            return cam;
        }
    }
}
