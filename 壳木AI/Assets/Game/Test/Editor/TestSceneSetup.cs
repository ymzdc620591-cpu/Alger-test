using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using Game.Player;
using Game.System;

namespace Game.Test
{
    public static class TestSceneSetup
    {
        const string ScenePath  = "Assets/Game/Test/Scenes/TestScene.unity";
        const string PrefabPath = "Assets/Game/Test/Resources/UI/TestStartPanel.prefab";

        [MenuItem("壳木AI/创建 TestScene")]
        static void CreateTestScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EnsureDirectory("Assets/Game/Test/Scenes");
            EnsureDirectory("Assets/Game/Test/Resources/UI");

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildLighting();

            var envGroup      = new GameObject("--Environment--");
            var gameplayGroup = new GameObject("--Gameplay--");
            var managersGroup = new GameObject("--Managers--");

            BuildGround(envGroup.transform);

            var player     = BuildPlayer(gameplayGroup.transform);
            var spawnPoint = BuildSpawnPoint(gameplayGroup.transform);

            BuildGameManager(managersGroup.transform);
            BuildBootstrap(managersGroup.transform, player, spawnPoint);
            BuildCamera(player.transform);
            BuildEventSystem();

            CreateStartPanelPrefab();

            AssetDatabase.SaveAssets();
            var saved = EditorSceneManager.SaveScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
            AssetDatabase.Refresh();

            if (saved)
                Debug.Log("[壳木AI] TestScene 创建完成 → " + ScenePath);
            else
                Debug.LogError("[壳木AI] 场景保存失败，请检查路径权限");
        }

        // ── Scene objects ────────────────────────────────────────────────────

        static void BuildLighting()
        {
            var go    = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type      = LightType.Directional;
            light.intensity = 1f;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        static void BuildGround(Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = "Ground";
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(10f, 1f, 10f);
        }

        static GameObject BuildPlayer(Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Player";
            go.transform.SetParent(parent, false);
            go.SetActive(false);
            Object.DestroyImmediate(go.GetComponent<CapsuleCollider>());
            go.AddComponent<CharacterController>();
            go.AddComponent<PlayerController>();
            return go;
        }

        static Transform BuildSpawnPoint(Transform parent)
        {
            var go = new GameObject("SpawnPoint");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, 5f, 0f);
            return go.transform;
        }

        static void BuildGameManager(Transform parent)
        {
            var go = new GameObject("GameManager");
            go.transform.SetParent(parent, false);
            go.AddComponent<GameManager>();
        }

        static void BuildBootstrap(Transform parent, GameObject player, Transform spawnPoint)
        {
            var go        = new GameObject("TestSceneBootstrap");
            go.transform.SetParent(parent, false);
            var bootstrap = go.AddComponent<TestSceneBootstrap>();
            SetField(bootstrap, "_playerGo",   player);
            SetField(bootstrap, "_spawnPoint", spawnPoint);
        }

        static void BuildCamera(Transform playerTransform)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.AddComponent<Camera>();
            go.transform.position = new Vector3(0f, 2f, -5f);
            var tpc = go.AddComponent<ThirdPersonCamera>();
            SetField(tpc, "_target", playerTransform);
        }

        static void BuildEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        // ── TestStartPanel prefab ────────────────────────────────────────────

        static void CreateStartPanelPrefab()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                    ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 根节点（全屏 RectTransform + 暗色背景）
            var root     = new GameObject("TestStartPanel");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.1f, 0.9f);

            var panel = root.AddComponent<TestStartPanel>();

            // Reset() 不会被 AddComponent 触发，手动关闭 showBlur / autoScale
            var panelSO = new SerializedObject(panel);
            SetBool(panelSO, "panelAttr.showBlur",  false);
            SetBool(panelSO, "panelAttr.autoScale", false);
            panelSO.ApplyModifiedProperties();

            // 标题
            AddText(root.transform, "Title",
                anchor: new Vector2(0.5f, 0.62f), size: new Vector2(500f, 80f),
                text: "第三人称控制测试", fontSize: 42, font: font);

            // 开始按钮
            var btn = AddButton(root.transform, "StartButton",
                anchor: new Vector2(0.5f, 0.42f), size: new Vector2(220f, 64f),
                label: "开始游戏", fontSize: 24, font: font);

            SetField(panel, "_startButton", btn);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            Debug.Log("[壳木AI] TestStartPanel 预制体创建完成 → " + PrefabPath);
        }

        static void AddText(Transform parent, string name,
            Vector2 anchor, Vector2 size, string text, int fontSize, Font font)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin        = anchor;
            rect.anchorMax        = anchor;
            rect.sizeDelta        = size;
            rect.anchoredPosition = Vector2.zero;
            var t       = go.AddComponent<Text>();
            t.text      = text;
            t.fontSize  = fontSize;
            t.color     = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            if (font != null) t.font = font;
        }

        static Button AddButton(Transform parent, string name,
            Vector2 anchor, Vector2 size, string label, int fontSize, Font font)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin        = anchor;
            rect.anchorMax        = anchor;
            rect.sizeDelta        = size;
            rect.anchoredPosition = Vector2.zero;
            var img          = go.AddComponent<Image>();
            img.color        = new Color(0.18f, 0.55f, 0.95f, 1f);
            var btn          = go.AddComponent<Button>();
            btn.targetGraphic = img;

            // 按钮文字
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var tRect = textGo.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            var t       = textGo.AddComponent<Text>();
            t.text      = label;
            t.fontSize  = fontSize;
            t.color     = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            if (font != null) t.font = font;

            return btn;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        static void EnsureDirectory(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            var parent     = Path.GetDirectoryName(assetPath)?.Replace('\\', '/') ?? "";
            var folderName = Path.GetFileName(assetPath);
            EnsureDirectory(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        static void SetField(Object target, string field, Object value)
        {
            var so   = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogWarning($"[TestSceneSetup] 找不到字段 '{field}' on {target.GetType().Name}");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }

        static void SetBool(SerializedObject so, string field, bool value)
        {
            var prop = so.FindProperty(field);
            if (prop != null) prop.boolValue = value;
        }
    }
}
