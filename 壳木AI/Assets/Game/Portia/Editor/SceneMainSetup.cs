using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Game.Portia
{
    public static class SceneMainSetup
    {
        const string WoodPrefabPath  = "Assets/Model/itemmall/twohand/ItemTwoHand_material_007_a.prefab";
        const string StonePrefabPath = "Assets/Model/interactive/stone/stone_LargeCrystal.prefab";

        [MenuItem("壳木AI/SceneMain/配置拾取资源生成器")]
        static void SetupPickupSpawner()
        {
            var woodPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>(WoodPrefabPath);
            var stonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StonePrefabPath);

            if (woodPrefab  == null) Debug.LogWarning($"[SceneMainSetup] 找不到: {WoodPrefabPath}");
            if (stonePrefab == null) Debug.LogWarning($"[SceneMainSetup] 找不到: {StonePrefabPath}");

            var existing = Object.FindObjectOfType<PickupItemSpawner>();
            var go       = existing != null ? existing.gameObject : new GameObject("PickupItemSpawner");
            var spawner  = go.GetComponent<PickupItemSpawner>() ?? go.AddComponent<PickupItemSpawner>();

            var so      = new SerializedObject(spawner);
            var entries = so.FindProperty("_entries");
            entries.arraySize = 2;

            SetEntry(entries.GetArrayElementAtIndex(0), woodPrefab,  (int)ItemGid.Wood,  1, 15, 0.5f);
            SetEntry(entries.GetArrayElementAtIndex(1), stonePrefab, (int)ItemGid.Stone, 1, 10, 0.4f);

            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("[SceneMainSetup] PickupItemSpawner 已配置，运行时将在玩家附近生成拾取资源。");
            EditorUtility.DisplayDialog("完成",
                "PickupItemSpawner 已添加到场景！\n\n游戏运行时自动在玩家附近随机生成：\n  木头 ×15\n  石头 ×10\n\n靠近后按 E 键拾取。",
                "确定");
        }

        static void SetEntry(SerializedProperty elem, GameObject prefab,
                             int gid, int itemCount, int spawnCount, float scale)
        {
            elem.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            elem.FindPropertyRelative("gid").intValue                = gid;
            elem.FindPropertyRelative("itemCount").intValue          = itemCount;
            elem.FindPropertyRelative("spawnCount").intValue         = spawnCount;
            elem.FindPropertyRelative("scale").floatValue            = scale;
        }
    }
}
