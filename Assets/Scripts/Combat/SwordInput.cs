using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Vadronia
{
    /// <summary>Teclas das habilidades da espada: E = Redemoinho de Aço, H = Investida Cortante.</summary>
    public static class SwordInput
    {
        public static bool SkillEPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            bool key = k != null && k.eKey.wasPressedThisFrame;
            bool pad = Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;
            return key || pad;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }

        public static bool SkillHPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current;
            bool key = k != null && k.hKey.wasPressedThisFrame;
            bool pad = Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame;
            return key || pad;
#else
            return Input.GetKeyDown(KeyCode.H);
#endif
        }
    }
}