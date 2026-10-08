using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Vadronia
{
    /// <summary>Entrada da espada: clique esquerdo = combo, E e H = habilidades, Shift = correr (Z continua valendo).</summary>
    public static class SwordInput
    {
        public static bool AttackPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var m = Mouse.current;
            var pad = Gamepad.current;
            return (m != null && m.leftButton.wasPressedThisFrame) || (pad != null && pad.buttonSouth.wasPressedThisFrame);
#else
            return Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1");
#endif
        }

        public static bool SkillEPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current; return k != null && k.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }

        public static bool SkillHPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current; return k != null && k.hKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.H);
#endif
        }

        public static bool ShiftHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current; return k != null && (k.leftShiftKey.isPressed || k.rightShiftKey.isPressed);
#else
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
        }

        public static bool TryGamepadAim(out Vector2 direction)
        {
            direction = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var pad = Gamepad.current;
            if (pad == null) return false;
            direction = pad.rightStick.ReadValue();
            return direction.sqrMagnitude > .04f;
#else
            return false;
#endif
        }

        public static bool TryMouseWorld(Camera cam, out Vector2 world)
        {
            world = default(Vector2);
            if (cam == null) return false;
#if ENABLE_INPUT_SYSTEM
            var m = Mouse.current; if (m == null) return false;
            Vector2 screen = m.position.ReadValue();
#else
            Vector2 screen = Input.mousePosition;
#endif
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, cam.nearClipPlane));
            world = new Vector2(w.x, w.y);
            return true;
        }
    }
}