using UnityEngine;
using Game.System;
using Starter.UI;

namespace Game.Portia
{
    public class PortiaBootstrap : MonoBehaviour
    {
        void Start()
        {
            UIManager.Inst.Init();
            UIManager.Inst.PushPanel("UI/MainMenuPresetPanel");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }
}
