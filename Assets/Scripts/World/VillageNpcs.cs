using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vadronia
{
    /// <summary>NPCs de Grünwald além de Konrad: rotas próprias, funções (descanso, dicas, ervas) e aldeões.</summary>
    public sealed class VillageNpcs : IDisposable
    {
        sealed class Npc
        {
            public NpcDefinition Def;
            public CharacterView View;
            public SpriteRenderer Marker;
            public string Hint;
            public int Waypoint = 1, Talks;
            public float Wait;
        }

        const float TalkRange = .8f;
        const float TalkRangeSq = TalkRange * TalkRange;
        const float FreezeRangeSq = TalkRangeSq * .64f;
        readonly List<Npc> npcs = new List<Npc>(NpcRoster.All.Length);
        readonly List<FootBlock> blocks;
        readonly GameObject root = new GameObject("NPCs de Grünwald");
        readonly VisualLibrary art = new VisualLibrary();
        float clock;
        public int Count => npcs.Count;
        public FootPoint Position(int index) => npcs[index].View.Position;
        public string Hint(int index) => npcs[index].Hint;

        public VillageNpcs(List<FootBlock> blocks)
        {
            this.blocks = blocks;
            foreach (var def in NpcRoster.All)
            {
                var view = new CharacterView(def.Name, def.Route[0], def.Look);
                view.Tint(Color.white, def.Scale);
                var marker = art.Add(root.transform, def.Name + " — conversa", art.Square, Vector2.zero,
                    new Vector2(.1f, .1f), new Color(.75f, .9f, 1f), 15000);
                marker.transform.localRotation = Quaternion.Euler(0, 0, 45);
                npcs.Add(new Npc
                {
                    Def = def,
                    View = view,
                    Marker = marker,
                    Hint = "T  ·  " + Verb(def) + " " + Capitalize(def.Name)
                });
            }
        }

        public void Tick(float dt, bool frozen, Vector2 player)
        {
            clock += dt;
            for (int i = 0; i < npcs.Count; i++)
            {
                var n = npcs[i];
                var old = n.View.Position;
                float pdx = old.X - player.x, pdy = old.Y - player.y;
                bool playerNear = pdx * pdx + pdy * pdy < FreezeRangeSq;

                if (frozen || n.Wait > 0 || playerNear)
                {
                    if (!frozen && n.Wait > 0) n.Wait = Mathf.Max(0, n.Wait - dt);
                    n.View.Place(old);
                    n.View.Animate(dt);
                }
                else
                {
                    var target = n.Def.Route[n.Waypoint];
                    var move = Vector2.ClampMagnitude(new Vector2(target.X - old.X, target.Y - old.Y), n.Def.Speed * dt);
                    var next = MovementCore.Move(old, move.x, move.y, blocks);
                    n.View.Place(next);
                    n.View.Animate(dt);
                    float dx = next.X - target.X, dy = next.Y - target.Y;
                    if (dx * dx + dy * dy < .0004f)
                    {
                        int reached = n.Waypoint;
                        n.Waypoint = (n.Waypoint + 1) % n.Def.Route.Length;
                        n.Wait = n.Def.Pause[reached];
                    }
                }
                n.Marker.transform.position = new Vector3(n.View.Position.X,
                    n.View.Position.Y + 1.85f + Mathf.Sin(clock * 2 + n.Def.Name.Length) * .05f, 0);
            }
        }

        /// <summary>Índice do NPC ao alcance do jogador (ou -1) e a dica de interação.</summary>
        public int Nearest(Vector2 player, out string hint)
        {
            int best = -1;
            float bestDistanceSq = TalkRangeSq;
            for (int i = 0; i < npcs.Count; i++)
            {
                var p = npcs[i].View.Position;
                float dx = player.x - p.X, dy = player.y - p.Y;
                float distanceSq = dx * dx + dy * dy;
                if (distanceSq < bestDistanceSq)
                {
                    best = i;
                    bestDistanceSq = distanceSq;
                }
            }
            hint = best < 0 ? "" : npcs[best].Hint;
            return best;
        }

        static string Verb(NpcDefinition def) =>
            def.Function == NpcFunction.Rest ? "Descansar com" :
            def.Function == NpcFunction.Tips ? "Ouvir dicas de" : "Conversar com";
        static string Capitalize(string s) => s.Length == 0 ? s : s.Substring(0, 1) + s.Substring(1).ToLowerInvariant();

        /// <summary>Executa a função do NPC e abre o diálogo. Retorna true se o estado do jogo mudou (para salvar).</summary>
        public bool Talk(int index, AdventureState state, AdventureHud hud)
        {
            if (index < 0 || index >= npcs.Count) return false;
            var n = npcs[index];
            string line = NpcRoster.Line(n.Def, state.Progress.quest, state.Progress.herbs, n.Talks++);
            hud.ShowDialog(n.Def.Name + "  ·  " + n.Def.Title.ToUpperInvariant(), line);
            if (n.Def.Function == NpcFunction.Rest)
            {
                state.Rest();
                hud.Notify("Um breve descanso. Fôlego recuperado.");
                return true;
            }
            return false;
        }

        public void FacePlayer(int index, Vector2 player)
        {
            if (index < 0 || index >= npcs.Count) return;
            var p = npcs[index].View.Position;
            npcs[index].View.Face(new Vector2(player.x - p.X, player.y - p.Y));
        }

        public void Dispose()
        {
            for (int i = 0; i < npcs.Count; i++) npcs[i].View.Dispose();
            UnityEngine.Object.Destroy(root);
            art.Dispose();
        }
    }
}
