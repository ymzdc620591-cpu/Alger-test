using UnityEngine;
using Game.System;
using Starter.UI;

namespace Game.Portia
{
    public class PortiaBootstrap : MonoBehaviour
    {
        void Start()
        {
            EnsureRuntimeHUD<ItemPickupToastUI>("ItemPickupToastUI");
            EnsureRuntimeHUD<QuickBarHUD>("QuickBarHUD");
            UIManager.Inst.Init();
            UIManager.Inst.PushPanel("UI/MainMenuPresetPanel");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        void EnsureRuntimeHUD<T>(string objectName) where T : Component
        {
            if (FindObjectOfType<T>() != null) return;

            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            go.AddComponent<T>();
        }
    }
}
