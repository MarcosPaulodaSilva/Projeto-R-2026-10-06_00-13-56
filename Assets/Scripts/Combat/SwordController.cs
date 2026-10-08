using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vadronia
{
    /// <summary>
    /// Espada inicial do player (provisória, até existir inventário).
    ///   Clique esquerdo ... combo de 3 golpes, cada um com animação própria
    ///   E .................. Redemoinho de Aço (giro 360°, 3 pulsos de dano, 6 s)
    ///   H .................. Investida Cortante (avanço + corte final em arco, 8 s)
    ///   Z ou Shift ......... correr (já existia no ExplorerMotor)
    /// A espada só conhece ISwordTarget; quando houver inventário, basta trocar a fonte dos números de SwordCombo.Strikes.
    /// </summary>
    public sealed class SwordController : IDisposable
    {
        enum Mode { None, Whirl, Dash }

        // ---- ajustes rápidos (unidades de mundo) ----
        const float HandHeight = .78f, HandSide = .30f, RestAngle = 62f;
        const float WhirlRadius = 2.1f, WhirlDamage = 11f, WhirlKnock = 3.6f;
        const float WhirlLead = .08f, WhirlPulse = .17f, WhirlTail = .15f;
        const int WhirlPulses = 3;
        const float DashHitRadius = 1.0f, DashDamage = 14f;
        const float FinishRange = 2.0f, FinishHalfAngle = 100f, FinishDamage = 16f, FinishKnock = 4.5f;
        const float JumpHeight = .26f;          // altura do pulo do golpe 3

        public readonly SwordCombo Combo = new SwordCombo();
        public readonly List<ISwordTarget> Targets = new List<ISwordTarget>();

        readonly ExplorerMotor motor;
        readonly Camera cam;
        readonly Action<string> notify;
        readonly SwordArt art = new SwordArt();
        readonly SwordFx fx = new SwordFx();
        readonly PlayerPose pose;
        readonly GameObject swordRoot;
        readonly Transform swordT;
        readonly SpriteRenderer swordR;
        readonly HashSet<ISwordTarget> hits = new HashSet<ISwordTarget>();
        readonly List<TrainingDummy> dummies = new List<TrainingDummy>();

        Mode mode;
        Vector2 strikeAim = Vector2.down, dashAim = Vector2.down, smoothOff, aim = Vector2.down;
        float whirlT, swordAbs, windupFrom, notifyCool, trailT, ghostT;
        int pulses, dashPhase, lastStage;
        SwordPhase lastPhase;

        public bool Running => pose.Running;
        public Vector2 Aim => aim;

        public SwordController(ExplorerMotor motor, Camera cam, Action<string> notify = null)
        {
            this.motor = motor; this.cam = cam; this.notify = notify;
            pose = new PlayerPose(motor.Actor);
            swordRoot = new GameObject("Espada");
            swordT = swordRoot.transform;
            swordT.SetParent(motor.Actor.Transform, false);
            swordR = swordRoot.AddComponent<SpriteRenderer>();
            swordR.sprite = art.Blade;
            swordAbs = -90f + RestAngle;
        }

        public TrainingDummy SpawnDummy(Vector2 where)
        {
            var d = new TrainingDummy(where);
            dummies.Add(d); Targets.Add(d);
            return d;
        }

        public void Interrupt()
        {
            Combo.Interrupt();
            mode = Mode.None;
            dashPhase = 0;
            hits.Clear();
            pose.Reset();
        }

        // =====================================================================================================
        public void Tick(float dt)
        {
            dt = Mathf.Min(dt, .05f);
            var actor = motor.Actor;
            Vector2 pos = Feet();
            aim = ReadAim(pos);
            notifyCool = Mathf.Max(0, notifyCool - dt);
            Combo.Tick(dt);

            // ---- começar ações ----
            if (mode == Mode.None)
            {
                if (SwordInput.SkillEPressed()) StartWhirl(aim);
                else if (SwordInput.SkillHPressed()) StartDash(aim);
                else if (SwordInput.AttackPressed()) Combo.Press();
            }
            if (Combo.Swung) { strikeAim = aim; actor.Face(aim); hits.Clear(); windupFrom = swordAbs; trailT = 0; }

            // ---- movimento ----
            Vector2 move = ExplorerMotor.ReadMovement();
            bool run = ExplorerMotor.Held(KeyCode.Z) || SwordInput.ShiftHeld();
            bool dodge = ExplorerMotor.Pressed(KeyCode.Space);
            Vector2 input = move;

            if (mode == Mode.Whirl)
            {
                TickWhirl(dt, pos, aim);
                input = move * .5f; run = false; dodge = false;
            }
            else if (mode == Mode.Dash)
            {
                input = dashAim; run = false; dodge = dashPhase == 1;
            }
            else if (Combo.Busy)
            {
                bool recovering = Combo.Phase == SwordPhase.Recovery;
                bool free = recovering && Combo.Time >= Combo.Current.Cancel;
                input = free ? move * .45f : Vector2.zero; run = false;
                if (!recovering) dodge = false;
                else if (dodge && motor.State.DodgeCooldown <= 0 && motor.State.Stamina >= 24f) Combo.Interrupt();
            }
            motor.Tick(dt, input, run, dodge);
            pos = Feet();

            // Golpe único com deslocamento real: usa a colisão do ExplorerMotor.
            if (mode == Mode.None && Combo.Busy && Combo.Struck)
            {
                motor.Lunge(strikeAim, Combo.Current.Lunge);
                pos = Feet();
            }
            if (mode == Mode.None && Combo.Busy && (Combo.Struck || Combo.Phase == SwordPhase.Active))
            {
                var s = Combo.Current;
                if (Combo.Struck) SpawnSlash(pos, strikeAim, s, Combo.Stage);
                Strike(pos, strikeAim, s.Range, s.HalfAngle, s.Damage, s.Knockback, "Golpe " + Combo.Stage);
            }

            if (mode == Mode.Dash) AfterDash(dt, pos);

            // ---- visual ----
            PoseBody(dt, pos);
            pose.Apply(dt, run && mode == Mode.None && !Combo.Busy);
            UpdateSword(dt, pos);
            SpawnTrail(dt);
            DetectLanding(pos);
            fx.Tick(dt);
            for (int i = 0; i < dummies.Count; i++) dummies[i].Tick(dt);
            lastPhase = Combo.Phase; lastStage = Combo.Stage;
        }

        // =====================================================================================================
        // Habilidade E — Redemoinho de Aço
        void StartWhirl(Vector2 aim)
        {
            if (!Combo.WhirlReady) { Say("Redemoinho de Aço recarregando: " + Combo.WhirlRemaining.ToString("0.0") + " s"); return; }
            if (!Combo.CanAct) return;
            Combo.Interrupt(); Combo.StartWhirlCooldown();
            mode = Mode.Whirl; whirlT = 0; pulses = 0; hits.Clear(); trailT = 0;
            motor.Actor.Face(aim);
        }

        void TickWhirl(float dt, Vector2 pos, Vector2 aim)
        {
            whirlT += dt;
            while (pulses < WhirlPulses && whirlT >= WhirlLead + pulses * WhirlPulse)
            {
                hits.Clear();
                fx.Spawn(art.Ring, pos + Vector2.up * .5f, 0, WhirlRadius * 1.5f, WhirlRadius * 2.1f,
                    new Color(.7f, .9f, 1f, .9f), .26f, pulses % 2 == 0 ? 420f : -420f, OrderOf(pos) + 3);
                Strike(pos, aim, WhirlRadius, 180f, WhirlDamage, WhirlKnock, "Redemoinho de Aço");
                pulses++;
            }
            if (whirlT >= WhirlLead + WhirlPulses * WhirlPulse + WhirlTail) mode = Mode.None;
        }

        // Habilidade H — Investida Cortante (usa a esquiva do ExplorerMotor como avanço)
        void StartDash(Vector2 aim)
        {
            if (!Combo.DashReady) { Say("Investida Cortante recarregando: " + Combo.DashRemaining.ToString("0.0") + " s"); return; }
            if (!Combo.CanAct) return;
            var st = motor.State;
            if (st.DodgeCooldown > 0 || st.Stamina < 24f) { Say("Sem fôlego para a Investida Cortante."); return; }
            Combo.Interrupt(); Combo.StartDashCooldown();
            mode = Mode.Dash; dashAim = aim; dashPhase = 1; hits.Clear(); ghostT = 0; trailT = 0;
            motor.Actor.Face(aim);
        }

        void AfterDash(float dt, Vector2 pos)
        {
            if (dashPhase == 1)
            {
                if (!motor.State.Dodging) { mode = Mode.None; dashPhase = 0; return; }
                dashPhase = 2;
            }
            if (motor.State.Dodging)
            {
                Strike(pos, dashAim, DashHitRadius, 180f, DashDamage, 3.2f, "Investida Cortante");
                ghostT -= dt;
                if (ghostT <= 0) { ghostT = .04f; fx.Ghost(pose.Body, new Color(.6f, .85f, 1f, .5f), .26f); }
            }
            else
            {
                hits.Clear();
                fx.Spawn(art.Arc, pos + Vector2.up * .5f + dashAim * .2f, Deg(dashAim), FinishRange * 1.4f, FinishRange * 2.1f,
                    new Color(1f, .9f, .55f, 1f), .28f, 0, OrderOf(pos) + 3);
                Strike(pos, dashAim, FinishRange, FinishHalfAngle, FinishDamage, FinishKnock, "Investida Cortante (corte final)");
                mode = Mode.None; dashPhase = 0;
            }
        }

        // =====================================================================================================
        // Acertos
        int Strike(Vector2 origin, Vector2 dir, float range, float halfAngle, float damage, float knock, string label)
        {
            int n = 0;
            for (int i = 0; i < Targets.Count; i++)
            {
                var t = Targets[i];
                if (t == null || !t.Alive || hits.Contains(t)) continue;
                Vector2 to = t.Position - origin;
                float dist = to.magnitude;
                if (dist > range + t.Radius) continue;
                if (halfAngle < 179f && dist > t.Radius && Vector2.Angle(dir, to) > halfAngle) continue;
                hits.Add(t); n++;
                t.ReceiveHit(new SwordHit(damage, dist > .001f ? to / dist : dir, knock, label));
            }
            return n;
        }

        void SpawnSlash(Vector2 pos, Vector2 dir, SwordStrike s, int stage)
        {
            Color c = stage == 1 ? new Color(.85f, .95f, 1f, .9f)
                    : stage == 2 ? new Color(.7f, .85f, 1f, .9f)
                    : new Color(1f, .86f, .45f, 1f);
            float life = stage == 3 ? .32f : .2f;
            fx.Spawn(art.Arc, pos + Vector2.up * .5f + dir * .1f, Deg(dir), s.Range * 1.5f, s.Range * 2.05f, c, life, 0, OrderOf(pos) + 3);
            if (stage == 3) fx.Spawn(art.Arc, pos + Vector2.up * .5f + dir * .1f, Deg(dir), s.Range * 1.1f, s.Range * 1.7f,
                new Color(1f, 1f, 1f, .7f), .22f, 0, OrderOf(pos) + 4);
        }

        void DetectLanding(Vector2 pos)
        {
            if (lastPhase == SwordPhase.Active && Combo.Phase == SwordPhase.Recovery && lastStage == 3)
            {
                pose.Burst(pos, 8, 1.4f);
                fx.Spawn(art.Ring, pos + Vector2.up * .04f, 0, .6f, 3.4f, new Color(1f, .9f, .6f, .6f), .34f, 0, OrderOf(pos) - 2);
            }
        }

        // =====================================================================================================
        // Animação do corpo — cada golpe tem a sua coreografia
        //   Golpe 1: passo à frente + torção do tronco (corte lateral)
        //   Golpe 2: contra-passo, torção para o lado oposto e pequeno salto (corte de volta)
        //   Golpe 3: agacha, PULA, desce com o golpe pesado, aterrissa com esmagamento e poeira
        void PoseBody(float dt, Vector2 pos)
        {
            Vector2 off = Vector2.zero; float roll = 0; Vector2 sc = Vector2.one;

            if (mode == Mode.Dash)
            {
                roll = -Mathf.Clamp(dashAim.x, -1f, 1f) * 14f;
                sc = new Vector2(1.08f, .92f);
            }
            else if (mode == Mode.Whirl)
            {
                roll = Mathf.Sin(whirlT * 40f) * 3f;
                sc = new Vector2(1.05f, .95f);
            }
            else if (Combo.Busy)
            {
                var s = Combo.Current;
                float t, k, prog;           // k = avanço (-.3 recuo … 1 alcance), prog = 0..1 dentro da fase
                switch (Combo.Phase)
                {
                    case SwordPhase.Windup:
                        prog = Mathf.Clamp01(Combo.Time / s.Windup); t = Ease(prog); k = -.3f * t; break;
                    case SwordPhase.Active:
                        prog = Mathf.Clamp01(Combo.Time / s.Active); t = 1f - (1f - prog) * (1f - prog); k = Mathf.Lerp(-.3f, 1f, t); break;
                    default:
                        prog = Mathf.Clamp01(Combo.Time / s.Recovery); t = Ease(prog); k = 1f - t; break;
                }
                off = strikeAim * (k * s.Lunge);

                switch (Combo.Stage)
                {
                    case 1: // torção anti-horária no preparo, horária no corte
                        roll = Twist(Combo.Phase, t, +6f, -8f);
                        sc = new Vector2(1f + .04f * Mathf.Clamp01(k), 1f - .04f * Mathf.Clamp01(k));
                        break;
                    case 2: // espelhado + pulinho
                        roll = Twist(Combo.Phase, t, -6f, +8f);
                        off.y += Mathf.Sin(Mathf.Clamp01(k) * Mathf.PI) * .05f;
                        sc = new Vector2(1f + .04f * Mathf.Clamp01(k), 1f - .04f * Mathf.Clamp01(k));
                        break;
                    default: // golpe pesado: sobe no preparo, desce no golpe, esmaga na recuperação
                        if (Combo.Phase == SwordPhase.Windup)
                        {
                            off.y += JumpHeight * t;
                            sc = new Vector2(.96f, 1f + .06f * t);
                            roll = Mathf.Clamp(-strikeAim.x, -1f, 1f) * 3f * t;
                        }
                        else if (Combo.Phase == SwordPhase.Active)
                        {
                            float fall = prog * prog;
                            off.y += JumpHeight * (1f - fall);
                            sc = new Vector2(.98f + .1f * fall, 1.06f - .22f * fall);
                            roll = Mathf.Clamp(strikeAim.x, -1f, 1f) * -4f;
                        }
                        else
                        {
                            float settle = 1f - t;
                            sc = new Vector2(1f + .12f * settle, 1f - .16f * settle);
                        }
                        break;
                }
            }

            smoothOff = Vector2.Lerp(smoothOff, off, 1f - Mathf.Exp(-35f * dt));
            pose.Offset = smoothOff; pose.Roll = roll; pose.Scale = sc;
        }

        static float Twist(SwordPhase phase, float t, float windup, float strike)
        {
            if (phase == SwordPhase.Windup) return windup * t;
            if (phase == SwordPhase.Active) return Mathf.Lerp(windup, strike, t);
            return strike * (1f - t);
        }

        // Animação da espada: preparo → corte → recuperação, ângulos vindos de SwordCombo.Strikes
        void UpdateSword(float dt, Vector2 pos)
        {
            Vector2 restAim = motor.Heading.sqrMagnitude > .001f ? motor.Heading.normalized : Vector2.down;
            float restAbs = Deg(restAim) + RestAngle;
            Vector2 refAim = restAim; float reach = 0f, length = 1f;
            bool centered = false;

            if (mode == Mode.Whirl)
            {
                swordAbs = Deg(restAim) + 90f + whirlT * 2300f; centered = true;
            }
            else if (mode == Mode.Dash)
            {
                swordAbs = Mathf.LerpAngle(swordAbs, Deg(dashAim), 1f - Mathf.Exp(-30f * dt));
                refAim = dashAim; reach = .28f;
            }
            else if (Combo.Busy)
            {
                var s = Combo.Current; float aimDeg = Deg(strikeAim); refAim = strikeAim;
                if (Combo.Stage == 3) length = 1.15f;
                switch (Combo.Phase)
                {
                    case SwordPhase.Windup:
                        {
                            float t = Ease(Combo.Time / s.Windup);
                            swordAbs = Mathf.LerpAngle(windupFrom, aimDeg + s.StartAngle, t); reach = -.05f * t; break;
                        }
                    case SwordPhase.Active:
                        {
                            float p = Mathf.Clamp01(Combo.Time / s.Active);
                            swordAbs = aimDeg + Mathf.Lerp(s.StartAngle, s.EndAngle, 1f - (1f - p) * (1f - p)); reach = .28f; break;
                        }
                    default:
                        {
                            float t = Ease(Combo.Time / s.Recovery);
                            swordAbs = Mathf.LerpAngle(aimDeg + s.EndAngle, restAbs, t); reach = .28f * (1f - t); break;
                        }
                }
            }
            else swordAbs = Mathf.LerpAngle(swordAbs, restAbs, 1f - Mathf.Exp(-14f * dt));

            Vector2 perp = new Vector2(refAim.y, -refAim.x);
            Vector2 hand = new Vector2(0, HandHeight);
            if (!centered) hand += perp * HandSide + refAim * reach;
            swordT.localPosition = new Vector3(hand.x, hand.y, 0);
            swordT.rotation = Quaternion.Euler(0, 0, swordAbs - 90f);
            swordT.localScale = new Vector3(1f, length, 1f);
            swordR.sortingOrder = OrderOf(pos) + (refAim.y > .5f ? -1 : 2);
        }

        // Rastro (imagens residuais da lâmina) durante o corte, o redemoinho e a investida
        void SpawnTrail(float dt)
        {
            bool swinging = mode != Mode.None || (Combo.Busy && Combo.Phase == SwordPhase.Active);
            if (!swinging) return;
            trailT -= dt;
            if (trailT > 0) return;
            trailT = .018f;
            Color c = mode == Mode.None ? (Combo.Stage == 3 ? new Color(1f, .9f, .55f, .7f) : new Color(.85f, .95f, 1f, .6f))
                                        : new Color(.65f, .88f, 1f, .6f);
            fx.Spawn(art.Blade, swordT.position, swordAbs - 90f, swordT.localScale.y, swordT.localScale.y, c, .16f, 0, swordR.sortingOrder - 1);
        }

        // =====================================================================================================
        Vector2 Feet() { return new Vector2(motor.Actor.Position.X, motor.Actor.Position.Y); }

        Vector2 ReadAim(Vector2 feet)
        {
            Vector2 stick;
            if (SwordInput.TryGamepadAim(out stick)) return stick.normalized;
            Vector2 center = feet + Vector2.up * .55f, mouse;
            if (SwordInput.TryMouseWorld(cam, out mouse))
            {
                Vector2 d = mouse - center;
                if (d.sqrMagnitude > .04f) return d.normalized;
            }
            return motor.Heading.sqrMagnitude > .001f ? motor.Heading.normalized : Vector2.down;
        }

        void Say(string message)
        {
            if (notify == null || notifyCool > 0) return;
            notify(message); notifyCool = 1f;
        }

        static float Deg(Vector2 v) { return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg; }
        static float Ease(float t) { t = Mathf.Clamp01(t); return t * t * (3f - 2f * t); }
        static int OrderOf(Vector2 feet) { return -Mathf.RoundToInt(feet.y * 100) * 10 + 4; }

        public void Dispose()
        {
            for (int i = 0; i < dummies.Count; i++) dummies[i].Dispose();
            pose.Dispose(); fx.Dispose(); art.Dispose();
            if (swordRoot != null) UnityEngine.Object.Destroy(swordRoot);
        }
    }
}