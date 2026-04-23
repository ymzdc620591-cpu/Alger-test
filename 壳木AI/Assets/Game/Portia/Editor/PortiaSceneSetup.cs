using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using Game.Player;
using Game.System;

namespace Game.Portia
{
    public static class PortiaSceneSetup
    {
        const string TreePrefabPath = "Assets/Model/GameObject/ltree001_1.prefab";
        const string RockPrefabPath = "Assets/Model/itemmall/ItemMall_volcanicRock.prefab";
        const int    TreeCount      = 45;
        const int    RockCount      = 30;
        const int    IronOreCount   = 20;
        const float  SpawnRadius    = 65f;
        const float  ModelScale     = 3f;

        [MenuItem("壳木AI/配置 SampleScene (Portia P0)")]
        static void SetupScene()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!EditorUtility.DisplayDialog("配置 Portia P0",
                    $"将在场景 [{scene.name}] 中配置玩家、相机、交互系统和资源节点。\n已有地形保持不变，继续？",
                    "配置", "取消"))
                return;

            var envGroup      = GetOrCreateGroup("--Environment--");
            var gameplayGroup = GetOrCreateGroup("--Gameplay--");
            var managersGroup = GetOrCreateGroup("--Managers--");

            EnsureGameManager(managersGroup.transform);
            EnsureBootstrap(managersGroup.transform);
            EnsureInventoryManager(managersGroup.transform);
            EnsureEventSystem();
            EnsureHUD(managersGroup.transform);
            EnsureInventoryHUD(managersGroup.transform);

            var player = EnsurePlayer(gameplayGroup.transform);
            var cam    = EnsureCamera(player.transform);

            var pc = player.GetComponent<PlayerController>();
            if (pc != null) SetField(pc, "_cameraTransform", cam.transform);

            ScaleGround();
            ScatterResources(envGroup.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[壳木AI] Portia P0 场景配置完成。按 Play 即可运行。");
        }

        // ── Groups ────────────────────────────────────────────────────────────

        static GameObject GetOrCreateGroup(string name)
        {
            var ex = GameObject.Find(name);
            return ex != null ? ex : new GameObject(name);
        }

        // ── Managers ──────────────────────────────────────────────────────────

        static void EnsureGameManager(Transform parent)
        {
            if (Object.FindObjectOfType<GameManager>() != null) return;
            var go = new GameObject("GameManager");
            go.transform.SetParent(parent, false);
            go.AddComponent<GameManager>();
        }

        static void EnsureBootstrap(Transform parent)
        {
            if (Object.FindObjectOfType<PortiaBootstrap>() != null) return;
            var go = new GameObject("PortiaBootstrap");
            go.transform.SetParent(parent, false);
            go.AddComponent<PortiaBootstrap>();
        }

        static void EnsureInventoryManager(Transform parent)
        {
            if (Object.FindObjectOfType<InventoryManager>() != null) return;
            var go = new GameObject("InventoryManager");
            go.transform.SetParent(parent, false);
            go.AddComponent<InventoryManager>();
        }

        static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        static void EnsureHUD(Transform parent)
        {
            if (Object.FindObjectOfType<InteractPromptUI>() != null) return;
            var go = new GameObject("HUD");
            go.transform.SetParent(parent, false);
            go.AddComponent<InteractPromptUI>();
        }

        static void EnsureInventoryHUD(Transform parent)
        {
            if (Object.FindObjectOfType<InventoryHUD>() != null) return;
            var go = new GameObject("InventoryHUD");
            go.transform.SetParent(parent, false);
            go.AddComponent<InventoryHUD>();
        }

        // ── Player ────────────────────────────────────────────────────────────

        static GameObject EnsurePlayer(Transform parent)
        {
            var existing = GameObject.FindWithTag("Player");
            if (existing != null)
            {
                if (existing.GetComponent<InteractionDetector>() == null)
                    existing.AddComponent<InteractionDetector>();
                return existing;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Player";
            go.tag  = "Player";
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, 1f, 0f);
            Object.DestroyImmediate(go.GetComponent<CapsuleCollider>());
            go.AddComponent<CharacterController>();
            go.AddComponent<PlayerController>();
            go.AddComponent<InteractionDetector>();
            return go;
        }

        // ── Camera ────────────────────────────────────────────────────────────

        static GameObject EnsureCamera(Transform playerTransform)
        {
            var mainCam = Camera.main;
            var go      = mainCam != null ? mainCam.gameObject : CreateCamera();

            if (go.GetComponent<PortiaCamera>() == null)
            {
                var pc = go.AddComponent<PortiaCamera>();
                SetField(pc, "_target", playerTransform);
            }

            go.transform.position = playerTransform.position + new Vector3(0f, 4f, -5f);
            go.transform.rotation = Quaternion.Euler(25f, 0f, 0f);
            return go;
        }

        static GameObject CreateCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            return go;
        }

        // ── Ground ────────────────────────────────────────────────────────────

        static void ScaleGround()
        {
            // 找场景中第一个 Plane Mesh，放大以覆盖 SpawnRadius
            foreach (var mf in Object.FindObjectsOfType<MeshFilter>())
            {
                if (mf.sharedMesh != null && mf.sharedMesh.name == "Plane")
                {
                    mf.transform.localScale = new Vector3(15f, 1f, 15f); // 150×150 单位
                    Debug.Log($"[Portia] 地面 '{mf.gameObject.name}' 已缩放至 150×150");
                    return;
                }
            }
            // 未找到则新建
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(15f, 1f, 15f);
            Debug.Log("[Portia] 新建地面 150×150");
        }

        // ── Resource scattering ───────────────────────────────────────────────

        static void ScatterResources(Transform parent)
        {
            var old = parent.Find("ResourceNodes");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TreePrefabPath);
            var rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RockPrefabPath);

            if (treePrefab == null) Debug.LogWarning($"[Portia] 找不到树木预制体: {TreePrefabPath}");
            if (rockPrefab == null) Debug.LogWarning($"[Portia] 找不到岩石预制体: {RockPrefabPath}");

            var group = new GameObject("ResourceNodes");
            group.transform.SetParent(parent, false);

            // 树木 GID=1（木材）
            if (treePrefab != null)
                SpawnNodes(treePrefab, group.transform, TreeCount,
                    gid: (int)ItemGid.Wood, itemCount: 2, prompt: "按 F 砍树（木材）",
                    triggerSize: new Vector3(1f, 2f, 1f), triggerCenter: new Vector3(0f, 1f, 0f));

            // 石矿 GID=2（石块）
            if (rockPrefab != null)
                SpawnNodes(rockPrefab, group.transform, RockCount,
                    gid: (int)ItemGid.Stone, itemCount: 3, prompt: "按 F 挖矿（石块）",
                    triggerSize: new Vector3(0.8f, 0.6f, 0.8f), triggerCenter: new Vector3(0f, 0.3f, 0f));

            // 铁矿石 GID=3（复用石头模型）
            if (rockPrefab != null)
                SpawnNodes(rockPrefab, group.transform, IronOreCount,
                    gid: (int)ItemGid.IronOre, itemCount: 2, prompt: "按 F 挖矿（铁矿石）",
                    triggerSize: new Vector3(0.8f, 0.6f, 0.8f), triggerCenter: new Vector3(0f, 0.3f, 0f));
        }

        static void SpawnNodes(GameObject prefab, Transform parent,
            int count, int gid, int itemCount, string prompt,
            Vector3 triggerSize, Vector3 triggerCenter)
        {
            for (int i = 0; i < count; i++)
            {
                float x = Random.Range(-SpawnRadius, SpawnRadius);
                float z = Random.Range(-SpawnRadius, SpawnRadius);
                if (Mathf.Abs(x) < 4f && Mathf.Abs(z) < 4f) x += 5f;

                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.SetParent(parent, false);
                go.transform.position   = new Vector3(x, 0f, z);
                go.transform.rotation   = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                go.transform.localScale = Vector3.one * ModelScale;

                // 交互用 Trigger 包围盒（子物体，大而易命中）
                AddInteractionTrigger(go, triggerSize, triggerCenter);

                var node = go.AddComponent<GatherNode>();
                SetGatherNode(node, gid, itemCount, prompt);
            }
        }

        static void AddInteractionTrigger(GameObject go, Vector3 size, Vector3 center)
        {
            var child = new GameObject("InteractionTrigger");
            child.transform.SetParent(go.transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale    = Vector3.one;
            var col       = child.AddComponent<BoxCollider>();
            col.size      = size;
            col.center    = center;
            col.isTrigger = true;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        static void SetGatherNode(GatherNode node, int gid, int count, string prompt)
        {
            var so = new SerializedObject(node);
            var pt = so.FindProperty("_promptText");
            if (pt != null) pt.stringValue = prompt;

            var drops = so.FindProperty("_drops");
            if (drops != null)
            {
                drops.arraySize = 1;
                var elem = drops.GetArrayElementAtIndex(0);
                elem.FindPropertyRelative("Gid").intValue   = gid;
                elem.FindPropertyRelative("Count").intValue = count;
            }
            so.ApplyModifiedProperties();
        }

        static void SetField(Object target, string field, Object value)
        {
            var so   = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null) { Debug.LogWarning($"[Portia] 找不到字段 '{field}'"); return; }
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
