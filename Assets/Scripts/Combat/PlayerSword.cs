using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Vadronia
{
    /// <summary>
    /// Espada inicial do jogador (provisória, até existir inventário). Integra o SwordCombo puro ao input,
    /// à mira e à apresentação. Mantém a API anterior (Combo, Aim, Tick, Interrupt, Dispose).
    ///   Clique esquerdo / botão Sul ... combo de 3 golpes, cada um com animação própria
    ///   E / botão Oeste ............... Redemoinho de Aço (360°, 3 pulsos, recarga 6 s)
    ///   H / botão Norte ............... Investida Cortante (avanço + corte final, recarga 8 s)
    ///   Z ............................. correr (ExplorerMotor), agora com pose de corrida
    /// Tick() deve rodar DEPOIS de motor.Tick (como já acontece no VadroniaDemo).
    /// </summary>
    public sealed class PlayerSword : IDisposable
    {
        enum Mode { None, Whirl, Dash }

        /// <summary>Boneco de treino ao lado do ponto inicial, para testar os golpes. Ponha false para remover.</summary>
        public static bool SpawnTrainingDummy = true;

        // ---- ajustes rápidos (unidades de mundo) ----
        const float HandHeight = .78f, HandSide = .30f, RestAngle = 62f;
        const float PullBack = .12f, LungeSmooth = .14f, JumpHeight = .26f;
        const float WhirlRadius = 2.1f, WhirlDamage = 11f, WhirlKnock = 3.6f;
        const float WhirlLead = .08f, WhirlPulse = .17f, WhirlTail = .15f;
        const int WhirlPulses = 3;
        const float DashSpeed = 9.5f, DashTime = .22f, DashHitRadius = 1.0f, DashDamage = 14f;
        const float FinishRange = 2.0f, FinishHalfAngle = 100f, FinishDamage = 16f, FinishKnock = 4.5f;

        public readonly SwordCombo Combo = new SwordCombo();
        public readonly List<ISwordTarget> Targets = new List<ISwordTarget>();
        /// <summary>Opcional: mensagem curta (ex.: recarga). Ligue com sword.Notify = hud.Notify.</summary>
        public Action<string> Notify;

        readonly ExplorerMotor motor;
        readonly Camera view;
        readonly SwordArt art = new SwordArt();
        readonly SwordFx fx = new SwordFx();
        readonly PlayerPose pose;
        readonly GameObject swordRoot;
        readonly Transform swordT;
        readonly SpriteRenderer swordR;
        readonly HashSet<ISwordTarget> hits = new HashSet<ISwordTarget>();
        readonly List<TrainingDummy> dummies = new List<TrainingDummy>();

        Vector2 aim = Vector2.down, strikeAim = Vector2.down, dashAim = Vector2.down;
        Vector2 lungeMoved, smoothBody;
        Mode mode;
        float swordAbs, windupFrom, whirlT, dashT, trailT, ghostT, strikeClock = 9f, notifyCool;
        int pulses, lastStage;
        SwordPhase lastPhase;

        public Vector2 Aim => aim;

        public PlayerSword(ExplorerMotor motor, Camera view)
        {
            this.motor = motor;
            this.view = view;
            pose = new PlayerPose(motor.Actor);

            swordRoot = new GameObject("Espada do jogador");
            swordT = swordRoot.transform;
            swordT.SetParent(motor.Actor.Transform, false);
            swordR = swordRoot.AddComponent<SpriteRenderer>();
            swordR.sprite = art.Blade;
            swordAbs = -90f + RestAngle;

            if (SpawnTrainingDummy)
                SpawnDummy(new Vector2(motor.Actor.Position.X + 2.4f, motor.Actor.Position.Y));
        }

        public TrainingDummy SpawnDummy(Vector2 where)
        {
            var dummy = new TrainingDummy(where);
            dummies.Add(dummy); Targets.Add(dummy);
            return dummy;
        }

        // =====================================================================================================
        public void Tick(float dt)
        {
            dt = Mathf.Min(dt, .05f);
            Combo.Tick(dt);
            ReadAim();
            notifyCool = Mathf.Max(0, notifyCool - dt);

            bool dodging = motor.State.Dodging;
            if (dodging) { if (Combo.Busy) Combo.Interrupt(); mode = Mode.None; }

            if (mode == Mode.None && !dodging)
            {
                if (SwordInput.SkillEPressed()) StartWhirl();
                else if (SwordInput.SkillHPressed()) StartDash();
                else if (AttackPressed()) Combo.Press();
            }

            if (Combo.Swung) { strikeAim = aim; hits.Clear(); windupFrom = swordAbs; trailT = 0; }

            if (Combo.Busy)
            {
                FaceTowards(strikeAim);
                if (Combo.Struck)
                {
                    Vector2 before = Feet();
                    motor.Lunge(strikeAim, Combo.Current.Lunge);
                    lungeMoved = Feet() - before;   // distância real (a colisão pode ter encurtado)
                    strikeClock = 0f;
                    SpawnSlash(Feet(), strikeAim, Combo.Current, Combo.Stage);
                }
                if (Combo.Struck || Combo.Phase == SwordPhase.Active)
                {
                    var s = Combo.Current;
                    Strike(Feet(), strikeAim, s.Range, s.HalfAngle, s.Damage, s.Knockback, "Golpe " + Combo.Stage);
                }
            }

            if (mode == Mode.Whirl) TickWhirl(dt);
            else if (mode == Mode.Dash) TickDash(dt);

            Vector2 pos = Feet();
            strikeClock += dt;
            PoseBody(dt);
            pose.Apply(dt, motor.IsSprinting && mode == Mode.None && !Combo.Busy);
            UpdateSword(dt, pos);
            SpawnTrail(dt);
            DetectLanding(pos);
            fx.Tick(dt);
            for (int i = 0; i < dummies.Count; i++) dummies[i].Tick(dt);
            lastPhase = Combo.Phase; lastStage = Combo.Stage;
        }

        public void Interrupt()
        {
            Combo.Interrupt();
            mode = Mode.None;
            hits.Clear();
            lungeMoved = smoothBody = Vector2.zero;
            trailT = ghostT = 0f;
            pose.Reset();
        }

        // =====================================================================================================
        // E — Redemoinho de Aço
        void StartWhirl()
        {
            if (!Combo.WhirlReady) { Say("Redemoinho de Aço recarregando: " + Combo.WhirlRemaining.ToString("0.0") + " s"); return; }
            if (!Combo.CanAct) return;
            Combo.Interrupt(); Combo.StartWhirlCooldown();
            mode = Mode.Whirl; whirlT = 0; pulses = 0; hits.Clear(); trailT = 0;
            FaceTowards(aim);
        }

        void TickWhirl(float dt)
        {
            whirlT += dt;
            Vector2 pos = Feet();
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

        // H — Investida Cortante (avanço por motor.Lunge, que respeita as colisões da vila)
        void StartDash()
        {
            if (!Combo.DashReady) { Say("Investida Cortante recarregando: " + Combo.DashRemaining.ToString("0.0") + " s"); return; }
            if (!Combo.CanAct) return;
            Combo.Interrupt(); Combo.StartDashCooldown();
            mode = Mode.Dash; dashAim = aim; dashT = 0; ghostT = 0; trailT = 0; hits.Clear();
            FaceTowards(dashAim);
        }

        void TickDash(float dt)
        {
            dashT += dt;
            float step = DashSpeed * dt;
            Vector2 before = Feet();
            motor.Lunge(dashAim, step);
            Vector2 pos = Feet();
            bool blocked = (pos - before).magnitude < step * .25f;

            Strike(pos, dashAim, DashHitRadius, 180f, DashDamage, 3.2f, "Investida Cortante");
            ghostT -= dt;
            if (ghostT <= 0) { ghostT = .04f; fx.Ghost(pose.Body, new Color(.6f, .85f, 1f, .5f), .26f); }

            if (dashT >= DashTime || blocked)
            {
                hits.Clear();
                fx.Spawn(art.Arc, pos + Vector2.up * .5f + dashAim * .2f, Deg(dashAim), FinishRange * 1.4f, FinishRange * 2.1f,
                    new Color(1f, .9f, .55f, 1f), .28f, 0, OrderOf(pos) + 3);
                Strike(pos, dashAim, FinishRange, FinishHalfAngle, FinishDamage, FinishKnock, "Investida Cortante (corte final)");
                mode = Mode.None;
            }
        }

        // =====================================================================================================
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
            if (stage == 3)
                fx.Spawn(art.Arc, pos + Vector2.up * .5f + dir * .1f, Deg(dir), s.Range * 1.1f, s.Range * 1.7f,
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
        //   Golpe 1: recua o corpo, avança com torção do tronco (corte lateral)
        //   Golpe 2: mesma ideia espelhada, com pequeno salto (corte de volta)
        //   Golpe 3: agacha-sobe (pulo), desce com o golpe pesado, aterrissa esmagando e levantando poeira
        // O avanço real é o motor.Lunge (instantâneo); o recuo visual compensa o salto para ficar fluido.
        void PoseBody(float dt)
        {
            Vector2 slide = Vector2.zero, body = Vector2.zero, sc = Vector2.one;
            float roll = 0f;

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
                float prog, t;
                switch (Combo.Phase)
                {
                    case SwordPhase.Windup: prog = Mathf.Clamp01(Combo.Time / s.Windup); t = Ease(prog); break;
                    case SwordPhase.Active: prog = Mathf.Clamp01(Combo.Time / s.Active); t = 1f - (1f - prog) * (1f - prog); break;
                    default: prog = Mathf.Clamp01(Combo.Time / s.Recovery); t = Ease(prog); break;
                }

                if (Combo.Phase == SwordPhase.Windup) slide = -strikeAim * (PullBack * t);
                else
                {
                    float tau = Mathf.Clamp01(strikeClock / LungeSmooth);
                    float e = 1f - (1f - tau) * (1f - tau);
                    slide = -(strikeAim * PullBack + lungeMoved) * (1f - e);
                }

                float load = Combo.Phase == SwordPhase.Active ? 1f : (Combo.Phase == SwordPhase.Windup ? t : 1f - t);
                switch (Combo.Stage)
                {
                    case 1:
                        roll = Twist(Combo.Phase, t, +6f, -8f);
                        sc = new Vector2(1f + .04f * load, 1f - .04f * load);
                        break;
                    case 2:
                        roll = Twist(Combo.Phase, t, -6f, +8f);
                        if (Combo.Phase == SwordPhase.Active) body.y = Mathf.Sin(prog * Mathf.PI) * .05f;
                        sc = new Vector2(1f + .04f * load, 1f - .04f * load);
                        break;
                    default:
                        if (Combo.Phase == SwordPhase.Windup)
                        {
                            body.y = JumpHeight * t;
                            sc = new Vector2(.96f, 1f + .06f * t);
                            roll = Mathf.Clamp(-strikeAim.x, -1f, 1f) * 3f * t;
                        }
                        else if (Combo.Phase == SwordPhase.Active)
                        {
                            float fall = prog * prog;
                            body.y = JumpHeight * (1f - fall);
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

            smoothBody = Vector2.Lerp(smoothBody, body, 1f - Mathf.Exp(-35f * dt));
            pose.Offset = slide + smoothBody;
            pose.Roll = roll;
            pose.Scale = sc;
        }

        static float Twist(SwordPhase phase, float t, float windup, float strike)
        {
            if (phase == SwordPhase.Windup) return windup * t;
            if (phase == SwordPhase.Active) return Mathf.Lerp(windup, strike, t);
            return strike * (1f - t);
        }

        // Animação da espada: preparo → corte → recuperação, ângulos de SwordCombo.Strikes
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

        // Vira o sprite para a mira e, se a direção mudou, já redesenha o quadro (o motor.Tick deste frame já rodou).
        void FaceTowards(Vector2 dir)
        {
            var actor = motor.Actor;
            int before = actor.Facing;
            actor.Face(dir);
            if (actor.Facing != before) actor.Animate(0f, motor.State.Dodging, motor.IsSprinting);
        }

        Vector2 Feet() { return new Vector2(motor.Actor.Position.X, motor.Actor.Position.Y); }

        void Say(string message)
        {
            if (Notify == null || notifyCool > 0) return;
            Notify(message); notifyCool = 1f;
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