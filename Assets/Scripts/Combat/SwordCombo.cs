using System;

namespace Vadronia
{
    public enum SwordPhase { Idle, Windup, Active, Recovery }

    /// <summary>Dados de um golpe do combo. Ângulos em graus, relativos à mira (positivo = anti-horário).</summary>
    public struct SwordStrike
    {
        public readonly float Windup, Active, Recovery, Cancel, Range, HalfAngle, Damage, Knockback, Lunge, StartAngle, EndAngle;
        public SwordStrike(float windup, float active, float recovery, float cancel, float range, float halfAngle,
            float damage, float knockback, float lunge, float startAngle, float endAngle)
        {
            Windup = windup; Active = active; Recovery = recovery; Cancel = cancel; Range = range; HalfAngle = halfAngle;
            Damage = damage; Knockback = knockback; Lunge = lunge; StartAngle = startAngle; EndAngle = endAngle;
        }
    }

    /// <summary>
    /// Regras puras da espada: combo de 3 golpes + recargas das habilidades E e H.
    /// Sem Unity: pode ser coberto pelo projeto de regressão .NET.
    /// </summary>
    public sealed class SwordCombo
    {
        public const float ComboGrace = .35f;
        public const float WhirlCooldown = 6f;
        public const float DashCooldown = 8f;

        public static readonly SwordStrike[] Strikes =
        {
            new SwordStrike(.07f, .10f, .17f, .06f, 1.55f, 70f, 10f, 2.2f, .30f,  100f, -80f),
            new SwordStrike(.06f, .10f, .18f, .07f, 1.60f, 75f, 12f, 2.4f, .32f,  -95f,  85f),
            new SwordStrike(.16f, .14f, .36f, .22f, 1.95f, 85f, 22f, 5.0f, .55f,  165f, -15f),
        };

        public int Stage { get; private set; }
        public SwordPhase Phase { get; private set; }
        public float Time { get; private set; }
        public bool Swung { get; private set; }
        public bool Struck { get; private set; }
        public float WhirlRemaining { get; private set; }
        public float DashRemaining { get; private set; }
        bool buffered;

        public bool Busy => Phase != SwordPhase.Idle;
        public SwordStrike Current => Stage > 0 ? Strikes[Stage - 1] : default(SwordStrike);
        public bool WhirlReady => WhirlRemaining <= 0;
        public bool DashReady => DashRemaining <= 0;
        public bool CanAct => Phase == SwordPhase.Idle || (Phase == SwordPhase.Recovery && Time >= Current.Cancel);

        public bool Press()
        {
            if (Phase == SwordPhase.Idle) { Begin(1); return true; }
            buffered = true;
            return true;
        }

        public void StartWhirlCooldown() { WhirlRemaining = WhirlCooldown; }
        public void StartDashCooldown() { DashRemaining = DashCooldown; }

        public void Interrupt() { Stage = 0; Phase = SwordPhase.Idle; Time = 0; buffered = false; }

        public void Tick(float dt)
        {
            Swung = Struck = false;
            if (dt <= 0 || float.IsNaN(dt) || float.IsInfinity(dt)) return;
            dt = Math.Min(dt, .1f);
            WhirlRemaining = Math.Max(0, WhirlRemaining - dt);
            DashRemaining = Math.Max(0, DashRemaining - dt);
            if (Phase == SwordPhase.Idle) return;

            Time += dt;
            var s = Current;
            if (Phase == SwordPhase.Windup && Time >= s.Windup) { Time -= s.Windup; Phase = SwordPhase.Active; Struck = true; }
            if (Phase == SwordPhase.Active && Time >= s.Active) { Time -= s.Active; Phase = SwordPhase.Recovery; }
            if (Phase != SwordPhase.Recovery) return;

            bool last = Stage >= Strikes.Length;
            if (buffered && !last && Time >= s.Cancel) { Begin(Stage + 1); return; }
            float end = s.Recovery + (last ? 0f : ComboGrace);
            if (Time >= end)
            {
                bool again = buffered && last;
                Interrupt();
                if (again) Begin(1);
            }
        }

        void Begin(int stage)
        {
            Stage = stage; Phase = SwordPhase.Windup; Time = 0; buffered = false; Swung = true;
        }
    }
}
