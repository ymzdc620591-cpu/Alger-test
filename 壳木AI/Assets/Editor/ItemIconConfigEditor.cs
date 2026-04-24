using Game.Portia;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class ItemIconConfigEditor
    {
        // GID → 资源路径（UI/sprites/package/ 均已导入为 Sprite 类型）
        static readonly (ItemGid gid, string path)[] IconMap =
        {
            (ItemGid.Wood,       "Assets/UI/sprites/package/Item_wood.png"),
            (ItemGid.Stone,      "Assets/UI/sprites/package/Item_stone.png"),
            (ItemGid.IronOre,    "Assets/UI/sprites/package/Item_iron.png"),
            (ItemGid.Mushroom,   "Assets/UI/sprites/package/Item_mushroom01.png"),
            (ItemGid.Plank,      "Assets/UI/sprites/package/Item_IronWoodenPlank.png"),
            (ItemGid.IronIngot,  "Assets/UI/sprites/package/Item_iron.png"),
            (ItemGid.CookingPot, "Assets/UI/sprites/package/Item_wooden_bowl.png"),
            (ItemGid.Axe,        "Assets/UI/sprites/package/Item_weapon_ironAxe.png"),
            (ItemGid.Pickaxe,    "Assets/UI/sprites/package/Item_weapon_ironPickaxe.png"),
            (ItemGid.WheatSeed,  "Assets/UI/sprites/package/Item_wheat_seeds.png"),
            (ItemGid.Food,       "Assets/UI/sprites/package/Item_food_02.png"),
            (ItemGid.Fish,       "Assets/UI/sprites/package/Item_fish.png"),
            (ItemGid.Coin,       "Assets/UI/sprites/package/Item_CoinChallenger.png"),
        };

        [MenuItem("Tools/壳木AI/生成 ItemIconConfig")]
        static void Generate()
        {
            EnsureFolder("Assets/Game/Resources");
            EnsureFolder("Assets/Game/Resources/Game");

            const string assetPath = "Assets/Game/Resources/Game/ItemIconConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<ItemIconConfig>(assetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<ItemIconConfig>();
                AssetDatabase.CreateAsset(config, assetPath);
            }

            var so = new SerializedObject(config);
            var list = so.FindProperty("entries");
            list.ClearArray();

            int idx = 0;
            foreach (var (gid, path) in IconMap)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                {
                    Debug.LogWarning($"[ItemIconConfig] 未找到 Sprite: {path}，跳过 {gid}");
                    continue;
                }

                list.InsertArrayElementAtIndex(idx);
                var elem = list.GetArrayElementAtIndex(idx);
                elem.FindPropertyRelative("gid").intValue              = (int)gid;
                elem.FindPropertyRelative("icon").objectReferenceValue = sprite;
                idx++;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = config;
            Debug.Log($"[ItemIconConfig] 已生成 {idx} 条图标配置 → {assetPath}");
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path[..slash], path[(slash + 1)..]);
        }
    }
}
