using UnityEngine;

namespace Starter.Runtime
{
    // 用法：ResManager.LoadGameObjectSync("UI/HUDPanel")        → 返回 prefab 不实例化
    //       ResManager.InstantiateGameObjectSync("UI/HUDPanel") → 加载并实例化
    //       ResManager.LoadAssetSync<AudioClip>("Audio/BGM")    → 加载任意类型资源
    public static class ResManager
    {
        public static GameObject LoadGameObjectSync(string location)
        {
            var prefab = Resources.Load<GameObject>(location);
            if (prefab == null)
                Debug.LogError($"[ResManager] 资源加载失败: {location}");
            return prefab;
        }

        // 找不到时静默返回 null，不打错误日志（适用于有代码回退的场景）
        public static GameObject TryLoadGameObject(string location)
            => Resources.Load<GameObject>(location);

        public static GameObject InstantiateGameObjectSync(string location)
        {
            var prefab = Resources.Load<GameObject>(location);
            if (prefab == null)
            {
                Debug.LogError($"[ResManager] 资源加载失败: {location}");
                return null;
            }
            return Object.Instantiate(prefab);
        }

        public static T LoadAssetSync<T>(string location) where T : Object
        {
            var asset = Resources.Load<T>(location);
            if (asset == null)
                Debug.LogError($"[ResManager] 资源加载失败: {location}");
            return asset;
        }
    }
}
