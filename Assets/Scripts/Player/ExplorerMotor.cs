using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace Vadronia
{
    public sealed class ExplorerMotor
    {
        public readonly AdventureState State = new AdventureState();
        public readonly CharacterView Actor;
        readonly TownWorld world;
        Vector2 velocity, dodgeDirection = Vector2.down;
        public Vector2 Heading { get; private set; } = Vector2.down;
        public bool IsSprinting { get; private set; }
        public ExplorerMotor(CharacterView actor, TownWorld world) { Actor = actor; this.world = world; }
        public void Tick(float dt)
        {
            Tick(dt, ReadMovement(), Held(KeyCode.LeftShift), Pressed(KeyCode.Space));
        }
        public void Tick(float dt, Vector2 input, bool sprint, bool dodge)
        {
            input = Vector2.ClampMagnitude(input, 1);
            if (input.sqrMagnitude > .01f) Heading = input.normalized;
            if (dodge && State.TryDodge()) dodgeDirection = Heading;
            IsSprinting = State.Tick(dt, sprint && input.sqrMagnitude > .01f);
            Vector2 desired = State.Dodging ? dodgeDirection * 7.5f : input * (IsSprinting ? 4.6f : 2.4f);
            velocity = State.Dodging ? desired : Vector2.MoveTowards(velocity, desired, dt * 32);
            var old = Actor.Position;
            var next = MovementCore.Move(old, velocity.x * dt, velocity.y * dt, world.Blocks);
            Actor.Place(next);
            Actor.Animate(dt, State.Dodging, IsSprinting);
        }
        public void Halt() { velocity = Vector2.zero; Actor.Place(Actor.Position); Actor.Animate(.1f,false,false); }
        public static Vector2 ReadMovement()
        {
            // RDFG replaces WASD because W/S are unavailable on Marcos's keyboard.
            // R = up, F = down, D = left, G = right. Arrow keys remain as fallback.
            return Vector2.ClampMagnitude(new Vector2(
                (Held(KeyCode.G)||Held(KeyCode.RightArrow)?1:0)-(Held(KeyCode.D)||Held(KeyCode.LeftArrow)?1:0),
                (Held(KeyCode.R)||Held(KeyCode.UpArrow)?1:0)-(Held(KeyCode.F)||Held(KeyCode.DownArrow)?1:0)),1);
        }
        public static bool Held(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var k = Keyboard.current; if(k==null) return false;
            switch(key) { case KeyCode.R:return k.rKey.isPressed; case KeyCode.D:return k.dKey.isPressed; case KeyCode.F:return k.fKey.isPressed; case KeyCode.G:return k.gKey.isPressed;
            case KeyCode.UpArrow:return k.upArrowKey.isPressed; case KeyCode.DownArrow:return k.downArrowKey.isPressed; case KeyCode.LeftArrow:return k.leftArrowKey.isPressed; case KeyCode.RightArrow:return k.rightArrowKey.isPressed; case KeyCode.LeftShift:return k.leftShiftKey.isPressed; } return false;
#else
            return Input.GetKey(key);
#endif
        }
        public static bool Pressed(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var k=Keyboard.current; if(k==null)return false;
            switch(key) {case KeyCode.Space:return k.spaceKey.wasPressedThisFrame;case KeyCode.E:return k.eKey.wasPressedThisFrame;case KeyCode.Escape:return k.escapeKey.wasPressedThisFrame;case KeyCode.F1:return k.f1Key.wasPressedThisFrame;case KeyCode.F5:return k.f5Key.wasPressedThisFrame;}return false;
#else
            return Input.GetKeyDown(key);
#endif
        }
    }
}
