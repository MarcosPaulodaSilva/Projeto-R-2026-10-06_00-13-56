using System;

namespace Vadronia
{
    [Serializable]
    public sealed class AdventureProgress
    {
        public int version = 1;
        public int quest; // 0: undiscovered; 1: gather; 2: reward claimed
        public int herbs;
        public int coins;
    }

    // Pure rules shared by gameplay and Editor checks.
    public sealed class AdventureState
    {
        public float Stamina { get; private set; } = 100;
        public float DodgeRemaining { get; private set; }
        public float DodgeCooldown { get; private set; }
        public AdventureProgress Progress { get; private set; } = new AdventureProgress();
        float recoveryDelay;
        public bool Dodging => DodgeRemaining > 0;
        public bool TryDodge()
        {
            if (DodgeCooldown > 0 || Stamina < 24) return false;
            Stamina -= 24; DodgeRemaining = .24f; DodgeCooldown = .7f; recoveryDelay = .8f;
            return true;
        }
        public bool Tick(float dt, bool wantsSprint)
        {
            if (dt <= 0 || float.IsNaN(dt) || float.IsInfinity(dt)) return false;
            dt = Math.Min(dt, .1f);
            DodgeRemaining = Math.Max(0, DodgeRemaining - dt);
            DodgeCooldown = Math.Max(0, DodgeCooldown - dt);
            bool sprint = wantsSprint && !Dodging && Stamina > 0;
            if (sprint) { Stamina = Math.Max(0, Stamina - 18 * dt); recoveryDelay = .6f; }
            else if (recoveryDelay > 0) recoveryDelay -= dt;
            else Stamina = Math.Min(100, Stamina + 25 * dt);
            return sprint;
        }
        public void Rest() { Stamina = 100; recoveryDelay = 0; }
        public void AcceptQuest() { if (Progress.quest == 0) Progress.quest = 1; }
        public bool Gather(int index)
        {
            if (Progress.quest != 1 || index < 0 || index > 2 || (Progress.herbs & (1 << index)) != 0) return false;
            Progress.herbs |= 1 << index; return true;
        }
        public int HerbCount => ((Progress.herbs & 1) != 0 ? 1 : 0) + ((Progress.herbs & 2) != 0 ? 1 : 0) + ((Progress.herbs & 4) != 0 ? 1 : 0);
        public bool ClaimReward()
        {
            if (Progress.quest != 1 || HerbCount != 3) return false;
            Progress.quest = 2; Progress.coins += 25; return true;
        }
        public void Restore(AdventureProgress progress)
        {
            if (progress == null || progress.version != 1) return;
            progress.quest = Math.Max(0, Math.Min(2, progress.quest));
            progress.herbs &= 7; progress.coins = Math.Max(0, Math.Min(999999, progress.coins));
            Progress = progress;
        }
    }

    public static class LowStepGait
    {
        public const float FootLift = .035f;
        public static float Swing(float phase) => (float)Math.Sin(phase * Math.PI * 2);
        public static float LeftLift(float phase) => Math.Max(0, Swing(phase)) * FootLift;
        public static float RightLift(float phase) => Math.Max(0, -Swing(phase)) * FootLift;
    }
}
