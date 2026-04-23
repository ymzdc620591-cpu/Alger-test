using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Game.Player;
using Game.System;

namespace Game.Portia
{
    public static class PortiaSceneSetup
    {
        const string PlayerModelPath    = "Assets/Model/actor/Npc_Aadit.prefab";
        const string TreePrefabPath     = "Assets/Model/GameObject/ltree001_1.prefab";
        const string RockPrefabPath     = "Assets/Model/itemmall/ItemMall_volcanicRock.prefab";
        const string SawingPrefabPath   = "Assets/Model/itemmall/comparegame/ItemCom_machine_sawing.prefab";
        const string FurnacePrefabPath  = "Assets/Model/itemmall/comparegame/ItemCom_machine_furnace02.prefab";
        const string AssemblyPrefabPath = "Assets/Model/itemmall/comparegame/ItemCom_machine_power01.prefab";
        const string RecipeDataFolder   = "Assets/Game/Portia/Data/Recipes";
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

            EnsureBuildController(player);

            ScaleGround();
            ScatterResources(envGroup.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[壳木AI] Portia P0 场景配置完成。按 Play 即可运行。");
        }

        // ── Groups ────────────────────────────────────────────────────────────

        // ── Build System ──────────────────────────────────────────────────────

        [MenuItem("壳木AI/配置建造系统 (BuildController)")]
        static void SetupBuildSystem()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                EditorUtility.DisplayDialog("提示",
                    "场景中未找到 Tag='Player' 的对象。\n请先运行「配置 SampleScene (Portia P0)」。",
                    "OK");
                return;
            }

            EnsureBuildController(player);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("完成",
                "BuildController 已挂载到 Player，切割机与熔炉 Prefab 已自动填入。\n\n" +
                "如需调整 Ground Mask，在 Player → BuildController Inspector 中修改。",
                "OK");
        }

        static void EnsureBuildController(GameObject player)
        {
            var bc = player.GetComponent<BuildController>() ?? player.AddComponent<BuildController>();

            var sawingPrefab   = AssetDatabase.LoadAssetAtPath<GameObject>(SawingPrefabPath);
            var furnacePrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(FurnacePrefabPath);
            var assemblyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssemblyPrefabPath);

            if (sawingPrefab   == null) Debug.LogWarning($"[壳木AI] 找不到切割机预制体: {SawingPrefabPath}");
            if (furnacePrefab  == null) Debug.LogWarning($"[壳木AI] 找不到熔炉预制体: {FurnacePrefabPath}");
            if (assemblyPrefab == null) Debug.LogWarning($"[壳木AI] 找不到组装台预制体: {AssemblyPrefabPath}");

            // 切割机配方：木材×1 → 木板×1（5s）
            var sawRecipe = GetOrCreateRecipe("Recipe_Wood2Plank",
                MachineType.Sawmill, "木头 → 木板", (int)ItemGid.Plank, 1, 5f,
                new RecipeInput { gid = (int)ItemGid.Wood, count = 1 });

            // 熔炉配方：铁矿石×2 → 铁锭×1（8s）
            var furnaceRecipe = GetOrCreateRecipe("Recipe_IronOre2Ingot",
                MachineType.Furnace, "铁矿石 → 铁锭", (int)ItemGid.IronIngot, 1, 8f,
                new RecipeInput { gid = (int)ItemGid.IronOre, count = 2 });

            // 组装台配方：木板×4 + 铁锭×2 → 烹饪锅×1（12s）
            var cookingPotRecipe = GetOrCreateRecipe("Recipe_CookingPot",
                MachineType.Assembly, "木板 + 铁锭 → 烹饪锅", (int)ItemGid.CookingPot, 1, 12f,
                new RecipeInput { gid = (int)ItemGid.Plank,     count = 4 },
                new RecipeInput { gid = (int)ItemGid.IronIngot,  count = 2 });

            // 组装台配方：木板×2 + 铁锭×1 → 斧头×1（10s）
            var axeRecipe = GetOrCreateRecipe("Recipe_Axe",
                MachineType.Assembly, "木板 + 铁锭 → 斧头", (int)ItemGid.Axe, 1, 10f,
                new RecipeInput { gid = (int)ItemGid.Plank,    count = 2 },
                new RecipeInput { gid = (int)ItemGid.IronIngot, count = 1 });

            var so       = new SerializedObject(bc);
            var machines = so.FindProperty("_machines");
            machines.arraySize = 3;

            SetMachineEntry(machines.GetArrayElementAtIndex(0),
                "切割机", "木头 → 木板",     sawingPrefab,   "切割机", scale: 3f,
                new[] { sawRecipe });
            SetMachineEntry(machines.GetArrayElementAtIndex(1),
                "熔炉",   "矿石 → 金属锭",   furnacePrefab,  "熔炉",   scale: 3f,
                new[] { furnaceRecipe });
            SetMachineEntry(machines.GetArrayElementAtIndex(2),
                "组装台", "加工材料 → 成品", assemblyPrefab, "组装台", scale: 3f,
                new[] { cookingPotRecipe, axeRecipe });

            var maxDist = so.FindProperty("_maxPlaceDistance");
            if (maxDist != null) maxDist.floatValue = 20f;

            so.ApplyModifiedProperties();
            Debug.Log("[壳木AI] BuildController 配置完成 → 切割机 + 熔炉 + 组装台（配方已绑定）");
        }

        // inputs 可以是 1~N 个 RecipeInput，用 params 展开
        static RecipeData GetOrCreateRecipe(string assetName, MachineType type, string recipeName,
            int outputGid, int outputCount, float processTime, params RecipeInput[] inputs)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Game/Portia/Data"))
                AssetDatabase.CreateFolder("Assets/Game/Portia", "Data");
            if (!AssetDatabase.IsValidFolder(RecipeDataFolder))
                AssetDatabase.CreateFolder("Assets/Game/Portia/Data", "Recipes");

            string path   = $"{RecipeDataFolder}/{assetName}.asset";
            var    recipe = AssetDatabase.LoadAssetAtPath<RecipeData>(path);
            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<RecipeData>();
                AssetDatabase.CreateAsset(recipe, path);
            }

            // 每次都刷新，保证改动立即生效
            recipe.machineType = type;
            recipe.recipeName  = recipeName;
            recipe.inputs      = inputs;
            recipe.outputGid   = outputGid;
            recipe.outputCount = outputCount;
            recipe.processTime = processTime;
            EditorUtility.SetDirty(recipe);
            AssetDatabase.SaveAssets();
            return recipe;
        }

        static void SetMachineEntry(SerializedProperty elem, string display, string desc,
            GameObject prefab, string machineName, float scale = 3f, RecipeData[] recipes = null)
        {
            elem.FindPropertyRelative("displayName").stringValue      = display;
            elem.FindPropertyRelative("description").stringValue      = desc;
            elem.FindPropertyRelative("prefab").objectReferenceValue  = prefab;
            elem.FindPropertyRelative("machineName").stringValue      = machineName;
            var scaleProp = elem.FindPropertyRelative("scale");
            if (scaleProp != null) scaleProp.floatValue = scale;

            var recipesProp = elem.FindPropertyRelative("recipes");
            if (recipesProp != null && recipes != null)
            {
                recipesProp.arraySize = recipes.Length;
                for (int i = 0; i < recipes.Length; i++)
                    recipesProp.GetArrayElementAtIndex(i).objectReferenceValue = recipes[i];
            }
        }

        [MenuItem("壳木AI/从 Scene_demo 导入环境地形")]
        static void ImportFromSceneDemo()
        {
            const string DemoScenePath = "Assets/Scene_demo.unity";
            var activeScene = SceneManager.GetActiveScene();

            if (!EditorUtility.DisplayDialog("导入 Scene_demo",
                    $"将把 Scene_demo 中的地形/环境导入到当前场景 [{activeScene.name}]。\n继续？",
                    "导入", "取消"))
                return;

            var demoScene = EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Additive);

            var toMove = new List<GameObject>();
            foreach (var root in demoScene.GetRootGameObjects())
            {
                if (root.name == "Main Camera" || root.name == "Directional Light") continue;
                toMove.Add(root);
            }

            foreach (var go in toMove)
                SceneManager.MoveGameObjectToScene(go, activeScene);

            // 为所有导入的网格对象补全 MeshCollider（无碰撞体的地形/环境几何体）
            foreach (var go in toMove)
                EnsureMeshColliders(go.transform);

            EditorSceneManager.CloseScene(demoScene, true);
            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log($"[壳木AI] Scene_demo 环境已导入到场景 [{activeScene.name}]。");
        }

        // 递归为有 MeshFilter 但无任何 Collider 的对象添加 MeshCollider
        static void EnsureMeshColliders(Transform t)
        {
            var mf = t.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null && t.GetComponent<Collider>() == null)
                t.gameObject.AddComponent<MeshCollider>();

            foreach (Transform child in t)
                EnsureMeshColliders(child);
        }

        static GameObject GetOrCreateGroup(string name)
        {
            var ex = GameObject.Find(name);
            return ex != null ? ex : new GameObject(name);
        }

        // ── Scene_demo interaction ────────────────────────────────────────────

        [MenuItem("壳木AI/为 Scene_demo 地形补全碰撞体")]
        static void FixSceneDemoColliders()
        {
            var sceneRoot = GameObject.Find("Scene_demo");
            if (sceneRoot == null)
            {
                EditorUtility.DisplayDialog("提示",
                    "场景中未找到 'Scene_demo' 节点，请先执行「从 Scene_demo 导入环境地形」。", "OK");
                return;
            }

            int before = 0, after = 0;
            CountMissingColliders(sceneRoot.transform, ref before);
            EnsureMeshColliders(sceneRoot.transform);
            CountMissingColliders(sceneRoot.transform, ref after);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[壳木AI] 碰撞体补全完成，新增 {before - after} 个 MeshCollider。");
        }

        static void CountMissingColliders(Transform t, ref int count)
        {
            var mf = t.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null && t.GetComponent<Collider>() == null)
                count++;
            foreach (Transform child in t)
                CountMissingColliders(child, ref count);
        }

        [MenuItem("壳木AI/为 Scene_demo 节点添加采集逻辑")]
        static void AddInteractionToSceneDemoNodes()
        {
            var sceneRoot = GameObject.Find("Scene_demo");
            if (sceneRoot == null)
            {
                EditorUtility.DisplayDialog("提示",
                    "场景中未找到 'Scene_demo' 节点，请先执行「从 Scene_demo 导入环境地形」。", "OK");
                return;
            }

            int count = AddInteractionRecursive(sceneRoot.transform);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"[壳木AI] 已为 Scene_demo 中的 {count} 个资源节点添加采集逻辑。");
        }

        static int AddInteractionRecursive(Transform root, bool forceIronOre = false)
        {
            int count = 0;
            foreach (Transform child in root)
            {
                // 已有 GatherNode → 不重复添加，继续递归子节点
                if (child.GetComponent<GatherNode>() != null)
                {
                    count += AddInteractionRecursive(child);
                    continue;
                }

                string n = child.name.ToLower();

                // 用前缀精确匹配模型实例名，避免匹配到容器名
                bool isTree    = n.StartsWith("ltree") || n.StartsWith("palm") || n.StartsWith("bush");
                bool isRock    = n.StartsWith("itemmall_volcanic") || n.StartsWith("itemcom_rock")
                              || n.StartsWith("rock_")             || n.StartsWith("stone_");
                bool isIronOre = forceIronOre || n.StartsWith("tiekuang");

                // tiekuangshi 是铁矿石容器节点，进入后强制标记子节点为铁矿石
                bool enterIronOreGroup = n == "tiekuangshi" || n.StartsWith("tiekuangshi");

                bool hasMesh = child.GetComponentInChildren<MeshFilter>() != null;

                if (isTree)
                {
                    var oldTrigger = child.Find("InteractionTrigger");
                    if (oldTrigger != null) Object.DestroyImmediate(oldTrigger.gameObject);
                    AddInteractionTrigger(child.gameObject, new Vector3(3f, 6f, 3f), new Vector3(0f, 3f, 0f));
                    var node = child.gameObject.AddComponent<GatherNode>();
                    SetGatherNode(node, (int)ItemGid.Wood, 2, "按 F 砍树（木材）", true);
                    count++;
                }
                else if (isRock)
                {
                    var oldTrigger = child.Find("InteractionTrigger");
                    if (oldTrigger != null) Object.DestroyImmediate(oldTrigger.gameObject);
                    AddInteractionTrigger(child.gameObject, new Vector3(2f, 1.5f, 2f), new Vector3(0f, 0.75f, 0f));
                    var node = child.gameObject.AddComponent<GatherNode>();
                    SetGatherNode(node, (int)ItemGid.Stone, 3, "按 F 挖矿（石块）", false);
                    count++;
                }
                else if (isIronOre && hasMesh)
                {
                    var oldTrigger = child.Find("InteractionTrigger");
                    if (oldTrigger != null) Object.DestroyImmediate(oldTrigger.gameObject);
                    AddInteractionTrigger(child.gameObject, new Vector3(2f, 1.5f, 2f), new Vector3(0f, 0.75f, 0f));
                    var node = child.gameObject.AddComponent<GatherNode>();
                    SetGatherNode(node, (int)ItemGid.IronOre, 2, "按 F 挖矿（铁矿石）", false);
                    count++;
                }
                else
                {
                    // 非资源节点（容器/装饰），继续递归；进入铁矿石容器时传 forceIronOre
                    count += AddInteractionRecursive(child, forceIronOre || enterIronOreGroup);
                }
            }
            return count;
        }



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

            var pc = go.GetComponent<PortiaCamera>() ?? go.AddComponent<PortiaCamera>();
            SetField(pc, "_target", playerTransform);

            var so = new SerializedObject(pc);
            var dist = so.FindProperty("_distance");
            if (dist != null) dist.floatValue = 12f;
            var minP = so.FindProperty("_minPitch");
            if (minP != null) minP.floatValue = 20f;
            so.ApplyModifiedProperties();

            go.transform.position = playerTransform.position + new Vector3(0f, 10f, -9f);
            go.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
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
                    triggerSize: new Vector3(1f, 2f, 1f), triggerCenter: new Vector3(0f, 1f, 0f),
                    fallOnGather: true);

            // 石矿 GID=2（石块）
            if (rockPrefab != null)
                SpawnNodes(rockPrefab, group.transform, RockCount,
                    gid: (int)ItemGid.Stone, itemCount: 3, prompt: "按 F 挖矿（石块）",
                    triggerSize: new Vector3(0.8f, 0.6f, 0.8f), triggerCenter: new Vector3(0f, 0.3f, 0f),
                    fallOnGather: false);

            // 铁矿石 GID=3（复用石头模型）
            if (rockPrefab != null)
                SpawnNodes(rockPrefab, group.transform, IronOreCount,
                    gid: (int)ItemGid.IronOre, itemCount: 2, prompt: "按 F 挖矿（铁矿石）",
                    triggerSize: new Vector3(0.8f, 0.6f, 0.8f), triggerCenter: new Vector3(0f, 0.3f, 0f),
                    fallOnGather: false);
        }

        static void SpawnNodes(GameObject prefab, Transform parent,
            int count, int gid, int itemCount, string prompt,
            Vector3 triggerSize, Vector3 triggerCenter, bool fallOnGather = false)
        {
            for (int i = 0; i < count; i++)
            {
                float x = Random.Range(-SpawnRadius, SpawnRadius);
                float z = Random.Range(-SpawnRadius, SpawnRadius);
                if (Mathf.Abs(x) < 4f && Mathf.Abs(z) < 4f) x += 5f;

                // Instantiate 断开预制体连接，保证每棵树数据完全独立
                var go = Object.Instantiate(prefab, parent);
                go.name = prefab.name;
                go.transform.position   = new Vector3(x, 0f, z);
                go.transform.rotation   = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                go.transform.localScale = Vector3.one * ModelScale;

                // 清理预制体可能残留的旧组件（多次执行 SetupScene 的幂等保护）
                foreach (var old in go.GetComponents<GatherNode>())
                    Object.DestroyImmediate(old);
                var oldTrigger = go.transform.Find("InteractionTrigger");
                if (oldTrigger != null) Object.DestroyImmediate(oldTrigger.gameObject);

                AddInteractionTrigger(go, triggerSize, triggerCenter);

                var node = go.AddComponent<GatherNode>();
                SetGatherNode(node, gid, itemCount, prompt, fallOnGather);
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

        static void SetGatherNode(GatherNode node, int gid, int count, string prompt, bool fallOnGather = false)
        {
            var so = new SerializedObject(node);
            var pt = so.FindProperty("_promptText");
            if (pt != null) pt.stringValue = prompt;

            var fall = so.FindProperty("_fallOnGather");
            if (fall != null) fall.boolValue = fallOnGather;

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
