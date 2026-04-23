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
            GameManager.Instance?.StartGame();
        }
    }
}
