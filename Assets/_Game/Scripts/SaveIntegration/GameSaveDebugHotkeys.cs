#if UNITY_EDITOR || DEVELOPMENT_BUILD
using HexaRealm.Save;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HexaRealm.SaveIntegration
{
    /// <summary>Development-only explicit save/load shortcuts. This type is absent from release builds.</summary>
    internal sealed class GameSaveDebugHotkeys : MonoBehaviour
    {
        private GameSaveService service;

        private void Awake()
        {
            service = GetComponent<GameSaveService>();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || service == null) return;
            if (keyboard.f5Key.wasPressedThisFrame) service.Save();
            if (keyboard.f9Key.wasPressedThisFrame) service.Load();
        }
    }
}
#endif
