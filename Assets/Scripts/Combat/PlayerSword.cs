using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Vadronia
{
    /// <summary>Integra o SwordCombo puro ao input, mira e apresentação do jogador.</summary>
    public sealed class PlayerSword : IDisposable
    {
        public readonly SwordCombo Combo = new SwordCombo();

        readonly ExplorerMotor motor;
        readonly Camera view;
        readonly VisualLibrary visuals = new VisualLibrary();
        readonly GameObject pivot;
        readonly SpriteRenderer blade, guard;
        Vector2 aim = Vector2.down;

        public Vector2 Aim => aim;

        public PlayerSword(ExplorerMotor motor, Camera view)
        {
            this.motor = motor;
            this.view = view;

            pivot = new GameObject("Espada do jogador");
            pivot.transform.SetParent(motor.Actor.Transform, false);
            pivot.transform.localPosition = new Vector3(0, .78f, 0);

            blade = visuals.Add(pivot.transform, "Lâmina", visuals.Square,
                new Vector2(0, .48f), new Vector2(.10f, .88f),
                new Color(.82f, .86f, .88f), 15000);
            guard = visuals.Add(pivot.transform, "Guarda", visuals.Square,
                new Vector2(0, .05f), new Vector2(.36f, .07f),
                new Color(.55f, .39f, .19f), 15001);
            pivot.SetActive(false);
        }

        public void Tick(float dt)
        {
            Combo.Tick(dt);
            ReadAim();

            if (motor.State.Dodging && Combo.Busy)
                Combo.Interrupt();

            if (!motor.State.Dodging && AttackPressed())
                Combo.Press();

            if (Combo.Busy)
            {
                motor.Actor.Face(aim);
                if (Combo.Struck)
                    motor.Lunge(aim, Combo.Current.Lunge);
            }

            UpdateVisual();
        }

        public void Interrupt()
        {
            Combo.Interrupt();
            pivot.SetActive(false);
        }

        void ReadAim()
        {
            Vector2 candidate = motor.Heading;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && view != null)
            {
                Vector2 screen = Mouse.current.position.ReadValue();
                Vector3 world = view.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -view.transform.position.z));
                candidate = new Vector2(world.x - motor.Actor.Position.X, world.y - motor.Actor.Position.Y);
            }
            if (Gamepad.current != null)
            {
                Vector2 stick = Gamepad.current.rightStick.ReadValue();
                if (stick.sqrMagnitude > .04f) candidate = stick;
            }
#else
            if (view != null)
            {
                Vector3 world = view.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -view.transform.position.z));
                candidate = new Vector2(world.x - motor.Actor.Position.X, world.y - motor.Actor.Position.Y);
            }
#endif
            if (candidate.sqrMagnitude > .0001f) aim = candidate.normalized;
        }

        static bool AttackPressed()
        {
#if ENABLE_INPUT_SYSTEM
            bool mouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool pad = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
            return mouse || pad;
#else
            return Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1");
#endif
        }

        void UpdateVisual()
        {
            if (!Combo.Busy)
            {
                pivot.SetActive(false);
                return;
            }

            pivot.SetActive(true);
            SwordStrike strike = Combo.Current;
            float relative;
            if (Combo.Phase == SwordPhase.Windup)
            {
                float t = Mathf.Clamp01(Combo.Time / Mathf.Max(.001f, strike.Windup));
                relative = Mathf.Lerp(0, strike.StartAngle, t);
            }
            else if (Combo.Phase == SwordPhase.Active)
            {
                float t = Mathf.Clamp01(Combo.Time / Mathf.Max(.001f, strike.Active));
                relative = Mathf.Lerp(strike.StartAngle, strike.EndAngle, t);
            }
            else
            {
                float t = Mathf.Clamp01(Combo.Time / Mathf.Max(.001f, strike.Recovery));
                relative = Mathf.Lerp(strike.EndAngle, 0, t);
            }

            float baseAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg - 90f;
            pivot.transform.localRotation = Quaternion.Euler(0, 0, baseAngle + relative);

            int order = -Mathf.RoundToInt(motor.Actor.Position.Y * 100) * 10 + 8;
            blade.sortingOrder = order;
            guard.sortingOrder = order + 1;
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(pivot);
            visuals.Dispose();
        }
    }
}
