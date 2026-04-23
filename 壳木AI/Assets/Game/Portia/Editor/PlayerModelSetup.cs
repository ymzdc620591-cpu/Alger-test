using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Game.Player;

namespace Game.Portia
{
    public static class PlayerModelSetup
    {
        const string NpcAaditPath = "Assets/Model/actor/Npc_Aadit.prefab";

        [MenuItem("壳木AI/Setup/绑定玩家模型 (Npc_Aadit)")]
        static void BindPlayerModel()
        {
            var pc = Object.FindObjectOfType<PlayerController>();
            if (pc == null)
            {
                EditorUtility.DisplayDialog("错误",
                    "场景中找不到 PlayerController。\n请先执行「配置 SampleScene (Portia P0)」创建 Player。",
                    "确定");
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NpcAaditPath);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("错误",
                    $"找不到预制体：\n{NpcAaditPath}\n请确认资产已导入。",
                    "确定");
                return;
            }

            var playerRoot = pc.gameObject;

            // 替换已有模型子对象
            var existingAnimator = playerRoot.GetComponentInChildren<Animator>();
            if (existingAnimator != null && existingAnimator.gameObject != playerRoot)
            {
                if (!EditorUtility.DisplayDialog("替换模型",
                        $"检测到已有模型子对象 [{existingAnimator.gameObject.name}]，是否替换为 Npc_Aadit？",
                        "替换", "取消"))
                    return;
                Undo.DestroyObjectImmediate(existingAnimator.gameObject);
            }

            // 隐藏 Player 根节点上的胶囊体网格（由 CreatePrimitive 生成）
            var mr = playerRoot.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Undo.RecordObject(mr, "Hide Capsule Mesh");
                mr.enabled = false;
            }

            // 实例化 Npc_Aadit 作为 Player 的子对象
            var modelGo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, playerRoot.transform);
            Undo.RegisterCreatedObjectUndo(modelGo, "Add Npc_Aadit");
            modelGo.transform.localPosition = Vector3.zero;
            modelGo.transform.localRotation = Quaternion.identity;
            modelGo.transform.localScale    = Vector3.one;

            // 获取 Animator（优先 root，否则 children）
            var animator = modelGo.GetComponent<Animator>();
            if (animator == null)
                animator = modelGo.GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogWarning("[PlayerModelSetup] Npc_Aadit 上未找到 Animator，动画将不会播放。");
            }

            // 将 Animator 赋值给 PlayerController._animator
            var so       = new SerializedObject(pc);
            var animProp = so.FindProperty("_animator");
            if (animProp != null)
            {
                animProp.objectReferenceValue = animator;
                so.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("[PlayerModelSetup] PlayerController 上找不到 _animator 字段。");
            }

            // 调整 CharacterController 尺寸以匹配人形模型
            var cc = playerRoot.GetComponent<CharacterController>();
            if (cc != null)
            {
                Undo.RecordObject(cc, "Adjust CharacterController");
                cc.height = 1.8f;
                cc.center = new Vector3(0f, 0.9f, 0f);
                cc.radius = 0.3f;
            }

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log($"[PlayerModelSetup] Npc_Aadit 绑定完成。" +
                      $"Animator={animator?.gameObject.name ?? "null"}, " +
                      $"CC: height=1.8 radius=0.3");

            EditorUtility.DisplayDialog("绑定完成",
                "Npc_Aadit 已成功绑定为玩家模型！\n\n" +
                "PlayerController → Animator 参数映射：\n" +
                "  Speed   (float)    移动 / 奔跑混合\n" +
                "  OnGround (bool)    是否落地\n" +
                "  VY      (float)    垂直速度（跳跃/下落）\n" +
                "  Jump    (trigger)  跳跃触发\n\n" +
                "CharacterController 已调整：height=1.8, radius=0.3",
                "确定");
        }
    }
}
