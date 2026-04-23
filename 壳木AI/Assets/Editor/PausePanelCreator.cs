using Game.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor
{
    public static class PausePanelCreator
    {
        [MenuItem("Tools/壳木AI/生成 TestPausePanel Prefab")]
        static void Create()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/UI");

            var root = new GameObject("TestPausePanel");
            SetupStretchRect(root.AddComponent<RectTransform>());
            var panel = root.AddComponent<TestPausePanel>();

            // 全屏半透明遮罩
            var bg = MakeImage("Background", root.transform, new Color(0f, 0f, 0f, 0.75f));
            SetupStretchRect(bg.GetComponent<RectTransform>());

            // 居中卡片容器
            var card = MakeImage("Container", bg.transform, new Color(0.1f, 0.1f, 0.1f, 0.95f));
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot     = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(420f, 400f);

            var vlg = card.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment       = TextAnchor.MiddleCenter;
            vlg.spacing              = 24f;
            vlg.padding              = new RectOffset(40, 40, 48, 48);
            vlg.childControlWidth    = true;
            vlg.childControlHeight   = false;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;

            // 标题
            MakeLabel("Title", card.transform, "游戏暂停", 38, FontStyles.Bold, Color.white, 60f);

            // 分隔线
            var line = MakeImage("Divider", card.transform, new Color(1f, 1f, 1f, 0.15f));
            var lineLE = line.AddComponent<LayoutElement>();
            lineLE.minHeight = 2f;
            lineLE.preferredHeight = 2f;

            // 继续按钮
            var resumeBtn = MakeButton("ResumeBtn", card.transform, "继续游戏",
                new Color(0.22f, 0.60f, 0.33f), new Color(0.28f, 0.72f, 0.40f));

            // 结束按钮
            var endBtn = MakeButton("EndBtn", card.transform, "结束游戏",
                new Color(0.55f, 0.35f, 0.10f), new Color(0.68f, 0.45f, 0.14f));

            // 退出按钮
            var quitBtn = MakeButton("QuitBtn", card.transform, "退出游戏",
                new Color(0.65f, 0.18f, 0.18f), new Color(0.78f, 0.24f, 0.24f));

            // 绑定序列化字段
            var so = new SerializedObject(panel);
            so.FindProperty("_resumeButton").objectReferenceValue = resumeBtn;
            so.FindProperty("_endButton").objectReferenceValue    = endBtn;
            so.FindProperty("_quitButton").objectReferenceValue   = quitBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            // 保存 Prefab
            const string savePath = "Assets/Resources/UI/TestPausePanel.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, savePath);
            Object.DestroyImmediate(root);

            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(savePath);
            Debug.Log("[PausePanelCreator] Prefab saved → " + savePath);
        }

        // ── 工具方法 ──────────────────────────────────────────────────────────

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path[..slash], path[(slash + 1)..]);
        }

        static void SetupStretchRect(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }

        static GameObject MakeImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<Image>().color = color;
            return go;
        }

        static void MakeLabel(string name, Transform parent, string text,
            float fontSize, FontStyles style, Color color, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, height);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = color;

            var le = go.AddComponent<LayoutElement>();
            le.minHeight       = height;
            le.preferredHeight = height;
        }

        static Button MakeButton(string name, Transform parent,
            string label, Color normalColor, Color highlightColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 64f);

            var img = go.AddComponent<Image>();
            img.color = normalColor;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor      = normalColor;
            colors.highlightedColor = highlightColor;
            colors.pressedColor     = new Color(normalColor.r * 0.8f, normalColor.g * 0.8f, normalColor.b * 0.8f);
            btn.colors = colors;

            var le = go.AddComponent<LayoutElement>();
            le.minHeight       = 64f;
            le.preferredHeight = 64f;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            SetupStretchRect(textGo.AddComponent<RectTransform>());
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 26f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            return btn;
        }
    }
}
