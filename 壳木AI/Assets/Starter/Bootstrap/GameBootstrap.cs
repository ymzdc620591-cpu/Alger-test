using UnityEngine;

namespace Starter.Bootstrap
{
    // 挂在 Bootstrap 场景的根物体上，场景仅含此脚本与各 Manager 预制体
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField, Tooltip("启动后加载的第一个游戏场景名")] string _firstScene = "GameMain";

        void Start()
        {
            InitializeSystems();
            SceneLoader.Instance.Load(_firstScene);
        }

        // 按序初始化需要提前启动的系统（存档、数据加载等）
        void InitializeSystems()
        {
            Debug.Log("[Bootstrap] Systems initialized.");
        }
    }
}
