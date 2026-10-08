using System;
using UnityEditor;
using UnityEngine;

namespace Vadronia.Editor
{
    /// <summary>Exercises the actual sword, collisions and presentation without touching saved progress.</summary>
    public static class CombatPlayChecks
    {
        sealed class Target : ISwordTarget
        {
            public Vector2 Position { get; set; }
            public float Radius => .15f;
            public bool Alive => true;
            public int Hits;
            public float Damage;
            public void ReceiveHit(SwordHit hit) { Hits++; Damage += hit.Damage; }
        }

        [MenuItem("Vadronia/Verificar combate em Play")]
        public static void Run()
        {
            var game = UnityEngine.Object.FindAnyObjectByType<VadroniaDemo>();
            if (!Application.isPlaying || game == null || game.Motor == null)
                throw new Exception("Entre em Play antes de verificar o combate.");
            bool enabled = game.enabled, spawnDummy = PlayerSword.SpawnTrainingDummy;
            game.enabled = false;
            TownWorld world = null;
            CharacterView actor = null;
            PlayerSword sword = null;
            int count = 0;
            Action<bool, string> check = (ok, message) =>
            {
                if (!ok) throw new Exception(message);
                count++; Debug.Log("COMBAT PASS: " + message);
            };
            try
            {
                var existingRenderers = new System.Collections.Generic.HashSet<SpriteRenderer>(
                    UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include));
                world = new TownWorld(Resources.Load<Texture2D>("Vadronia/town"));
                actor = new CharacterView("Player — teste de combate", new FootPoint(0, -6), null, game.characterAtlas, false);
                var motor = new ExplorerMotor(actor, world);
                PlayerSword.SpawnTrainingDummy = false;
                sword = new PlayerSword(motor, null);
                Action reset = () =>
                {
                    sword.Interrupt(); sword.Targets.Clear();
                    for (int i = 0; i < 90; i++) sword.Combo.Tick(.1f);
                    motor.State.Rest(); actor.Place(new FootPoint(0, -6)); motor.Halt();
                };
                foreach (float dt in new[] { .05f, 1f / 30, 1f / 60, 1f / 120 })
                {
                    reset();
                    for (int i = 0; i < Mathf.CeilToInt(.3f / dt); i++)
                        sword.Tick(dt, Vector2.right, false, false, i == 0);
                    check(Mathf.Abs(actor.Position.X - 2.09f) < .0001f,
                        "Investida percorre 2,09 unidades com intervalo " + dt);
                }

                reset();
                var target = new Target { Position = new Vector2(1.3f, -6) };
                var behind = new Target { Position = new Vector2(-1f, -6) };
                sword.Targets.Add(target); sword.Targets.Add(behind);
                for (int i = 0; i < 25; i++)
                    sword.Tick(.05f, Vector2.right, i == 0 || (sword.Combo.Busy && sword.Combo.Stage < 3), false, false);
                check(target.Hits == 3 && target.Damage == 44, "Combo causa 10 + 12 + 22, uma vez por golpe");
                check(behind.Hits == 0, "Combo não acerta alvo atrás do jogador");

                reset(); target = new Target { Position = new Vector2(.8f, -6) }; sword.Targets.Add(target);
                for (int i = 0; i < 16; i++) sword.Tick(.05f, Vector2.right, false, i == 0, false);
                check(target.Hits == 3 && target.Damage == 33, "Redemoinho aplica exatamente três pulsos");
                for (int i = 0; i < 16; i++) sword.Tick(.05f, Vector2.right, false, true, false);
                check(target.Hits == 3 && !sword.Combo.WhirlReady, "Recarga impede repetir o redemoinho");

                reset(); target = new Target { Position = new Vector2(2.3f, -6) }; sword.Targets.Add(target);
                for (int i = 0; i < 7; i++) sword.Tick(.05f, Vector2.right, false, false, i == 0);
                check(target.Hits == 2 && target.Damage == 30, "Investida acerta uma vez no avanço e uma no corte final");
                float endX = actor.Position.X;
                for (int i = 0; i < 7; i++) sword.Tick(.05f, Vector2.right, false, false, true);
                check(Mathf.Abs(actor.Position.X - endX) < .0001f, "Recarga impede repetir o avanço");

                reset(); actor.Place(new FootPoint(0, -.3f)); motor.Halt();
                for (int i = 0; i < 8; i++) sword.Tick(.05f, Vector2.up, false, false, i == 0);
                check(actor.Position.Y <= -.219f && MovementCore.Clear(actor.Position.X, actor.Position.Y, world.Blocks),
                    "Habilidade H respeita o colisor do poço");

                reset(); target = new Target { Position = new Vector2(1f, -6) }; sword.Targets.Add(target);
                sword.Tick(.02f, Vector2.right, true, false, false);
                sword.Interrupt();
                for (int i = 0; i < 12; i++) sword.Tick(.05f, Vector2.right, false, false, false);
                check(target.Hits == 0, "Interromper o preparo cancela o dano pendente");

                reset(); target = new Target { Position = new Vector2(.8f, -6) }; sword.Targets.Add(target);
                sword.Tick(.05f, Vector2.right, false, true, false);
                sword.Tick(.05f, Vector2.right, false, false, false);
                check(target.Hits == 1, "Primeiro pulso ocorre antes da interrupção");
                sword.Interrupt();
                var renderers = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include);
                bool cleared = true;
                foreach (var renderer in renderers)
                {
                    var parent = renderer.transform.parent;
                    if (!existingRenderers.Contains(renderer) && parent != null &&
                        (parent.name == "Efeitos da espada" || parent.name == "Poeira da corrida"))
                        cleared &= !renderer.gameObject.activeInHierarchy;
                }
                check(cleared, "Interrupção remove rastros e poeira ativos");
                check(Quaternion.Angle(actor.Transform.rotation, Quaternion.identity) < .001f &&
                    Vector3.Distance(actor.Transform.localScale, Vector3.one) < .001f &&
                    Vector2.Distance(actor.Transform.position, new Vector2(actor.Position.X, actor.Position.Y)) < .001f,
                    "Interrupção restaura posição, escala e rotação do corpo");
                for (int i = 0; i < 16; i++) sword.Tick(.05f, Vector2.right, false, false, false);
                check(target.Hits == 1 && !sword.Combo.WhirlReady, "Retomar após interrupção não repete pulsos nem elimina a recarga");
                Debug.Log(count + " verificações de combate em Play passaram; nenhum save foi alterado.");
            }
            finally
            {
                sword?.Dispose(); actor?.Dispose(); world?.Dispose();
                PlayerSword.SpawnTrainingDummy = spawnDummy;
                game.enabled = enabled;
            }
        }
    }
}
