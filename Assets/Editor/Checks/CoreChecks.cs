using System;
using System.Collections.Generic;

namespace Vadronia
{
    public static class CoreChecks
    {
        public static int Run(Action<string> report)
        {
            int count = 0;
            Check("Ciclo avança com a distância, não com o tempo", () =>
            {
                var a = new WalkCycle(); var b = new WalkCycle();
                for (int i = 0; i < 60; i++) a.Advance(.01f, 0);
                for (int i = 0; i < 30; i++) b.Advance(.02f, 0);
                Require(Math.Abs(a.Phase - b.Phase) < .0001f, "Fases diferentes para a mesma distância");
            }, report, ref count);
            Check("Quatro poses e ciclo completo", () =>
            {
                var cycle = new WalkCycle(); var frames = new HashSet<int>();
                for (int i = 0; i < 120; i++) { cycle.Advance(WalkCycle.Stride / 120, 0); frames.Add(cycle.Frame); }
                Require(frames.Count == 4, "Quadro nunca utilizado");
                Require(cycle.Phase < .001f || cycle.Phase > .999f, "Comprimento do ciclo incorreto");
            }, report, ref count);
            Check("Parar conserva orientação e interrompe passos", () =>
            {
                var cycle = new WalkCycle(); cycle.Advance(0, .4f); float phase = cycle.Phase;
                cycle.Advance(0, 0);
                Require(!cycle.Moving && cycle.Facing == 2 && cycle.Phase == phase, "Idle alterou orientação/fase");
            }, report, ref count);
            Check("Quatro direções corretas", () =>
            {
                var c = new WalkCycle();
                c.Advance(0, -.1f); Require(c.Facing == 0, "Frente");
                c.Advance(.1f, 0); Require(c.Facing == 1, "Direita");
                c.Advance(0, .1f); Require(c.Facing == 2, "Costas");
                c.Advance(-.1f, 0); Require(c.Facing == 3, "Esquerda");
            }, report, ref count);
            var wall = new List<FootBlock> { new FootBlock(0, -2, 1, 4) };
            Check("Não atravessa parede mesmo com deslocamento grande", () =>
            {
                var p = MovementCore.Move(new FootPoint(-3, 0), 10, 0, wall);
                Require(p.X <= -MovementCore.Radius && p.X > -.31f, "Atravessou parede ou parou longe demais");
            }, report, ref count);
            Check("Desliza ao longo da parede sem penetrar", () =>
            {
                var p = MovementCore.Move(new FootPoint(-.3f, 0), 1, 1, wall);
                Require(p.X < 0 && p.Y > .99f && MovementCore.Clear(p.X, p.Y, wall), "Colisão lateral incorreta");
            }, report, ref count);
            Check("Passos param quando a parede impede o deslocamento", () =>
            {
                var p = new FootPoint(-.22f, 0); var c = new WalkCycle();
                var next = MovementCore.Move(p, .1f, 0, wall); c.Advance(next.X - p.X, next.Y - p.Y);
                Require(!c.Moving, "Caminhando sem sair do lugar");
            }, report, ref count);
            Check("Limites do mapa e entradas inválidas", () =>
            {
                var empty = new List<FootBlock>();
                var p = MovementCore.Move(new FootPoint(0, 0), 100, -100, empty);
                Require(p.X <= MovementCore.MaxX - MovementCore.Radius && p.Y >= MovementCore.MinY + MovementCore.Radius, "Fora do mapa");
                var invalid = MovementCore.Move(p, float.NaN, 0, empty);
                Require(invalid.X == p.X && invalid.Y == p.Y, "NaN contaminou a posição");
            }, report, ref count);
            Check("Spawns livres e percurso inteiro de Konrad", () =>
            {
                var blocks = TownLayout.Blocks(); var p = TownLayout.Patrol[0]; int target = 1, laps = 0;
                var visited = new bool[TownLayout.Patrol.Length]; visited[0] = true;
                Require(TownLayout.PatrolPause.Length == TownLayout.Patrol.Length, "Cada ponto da patrulha precisa de uma pausa");
                Require(MovementCore.Clear(0, -5, blocks), "Player nasce dentro de obstáculo");
                for (int i = 0; i < 24000; i++)
                {
                    var t = TownLayout.Patrol[target]; float dx = t.X - p.X, dy = t.Y - p.Y;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    float step = Math.Min(distance, 1.65f / 60);
                    if (distance > 0) p = MovementCore.Move(p, dx / distance * step, dy / distance * step, blocks);
                    Require(MovementCore.Clear(p.X, p.Y, blocks), "Konrad dentro de obstáculo");
                    if (Math.Abs(p.X - t.X) + Math.Abs(p.Y - t.Y) < .02f)
                    { visited[target] = true; target = (target + 1) % TownLayout.Patrol.Length; if (target == 1) laps++; }
                }
                // A ronda cobre praça, ruas e quintais (~42 unidades por volta): cerca de 15 voltas em 400 s sem pausas.
                Require(laps >= 10, "Patrulha presa: " + laps + " voltas");
                Require(Array.TrueForAll(visited, v => v), "Patrulha não alcançou todos os pontos");
            }, report, ref count);
            Check("Roster dos NPCs é consistente", () =>
            {
                Require(NpcRoster.All.Length == 5, "Grünwald precisa dos cinco NPCs extras");
                var ids = new HashSet<string>();
                foreach (var npc in NpcRoster.All)
                {
                    Require(!string.IsNullOrWhiteSpace(npc.Id) && ids.Add(npc.Id), "ID de NPC ausente ou duplicado");
                    Require(npc.Route != null && npc.Route.Length >= 2, npc.Name + ": rota curta");
                    Require(npc.Look != null, npc.Name + ": aparência ausente");
                    Require(npc.Lines != null && npc.Lines.Length > 0, npc.Name + ": sem falas");
                }
                var maren = NpcRoster.All[2];
                Require(NpcRoster.Line(maren, 1, 0, 0).Contains("3 porção"), "Maren não informa as três ervas faltantes");
                Require(NpcRoster.Line(maren, 1, 7, 0).Contains("já colheu"), "Maren não reconhece coleta completa");
            }, report, ref count);
            Check("Pixel art procedural dos NPCs é válida", () =>
            {
                foreach (var npc in NpcRoster.All)
                {
                    var frame = NpcPixelArt.Frame(npc.Look, 0, 1);
                    Require(frame.Length == NpcPixelArt.FrameWidth * NpcPixelArt.FrameHeight, npc.Name + ": quadro inválido");
                    Require(Array.Exists(frame, p => p != NpcPixelArt.Clear), npc.Name + ": quadro vazio");
                }

                var look = NpcRoster.All[0].Look;
                var right = NpcPixelArt.Frame(look, 1, 0);
                var left = NpcPixelArt.Frame(look, 3, 0);
                for (int y = 0; y < NpcPixelArt.FrameHeight; y++)
                    for (int x = 0; x < NpcPixelArt.FrameWidth; x++)
                        Require(left[y * NpcPixelArt.FrameWidth + x] ==
                            right[y * NpcPixelArt.FrameWidth + (NpcPixelArt.FrameWidth - 1 - x)],
                            "Perfil esquerdo não é espelho do direito");

                var sheet = NpcPixelArt.Sheet(look);
                Require(sheet.Length == NpcPixelArt.SheetWidth * NpcPixelArt.SheetHeight * 4, "Folha RGBA32 com tamanho incorreto");
            }, report, ref count);
            Check("Rotas dos NPCs livres e completas", () =>
            {
                var blocks = TownLayout.Blocks();
                foreach (var npc in NpcRoster.All)
                {
                    Require(npc.Pause.Length == npc.Route.Length, npc.Name + ": uma pausa por ponto");
                    var p = npc.Route[0]; int target = 1, laps = 0;
                    var seen = new bool[npc.Route.Length]; seen[0] = true;
                    Require(MovementCore.Clear(p.X, p.Y, blocks), npc.Name + " nasce dentro de obstáculo");
                    for (int i = 0; i < 12000; i++)
                    {
                        var t = npc.Route[target]; float dx = t.X - p.X, dy = t.Y - p.Y;
                        float d = (float)Math.Sqrt(dx * dx + dy * dy); float step = Math.Min(d, npc.Speed / 60);
                        if (d > 0) p = MovementCore.Move(p, dx / d * step, dy / d * step, blocks);
                        Require(MovementCore.Clear(p.X, p.Y, blocks), npc.Name + " dentro de obstáculo");
                        if (Math.Abs(p.X - t.X) + Math.Abs(p.Y - t.Y) < .02f)
                        {
                            seen[target] = true; target = (target + 1) % npc.Route.Length;
                            if (target == 1) laps++;
                        }
                    }
                    Require(laps >= 5 && Array.TrueForAll(seen, v => v), npc.Name + ": rota presa");
                }
            }, report, ref count);
            Check("10.000 movimentos aleatórios preservam colisões e limites", () =>
            {
                var p = new FootPoint(0, -5); var blocks = TownLayout.Blocks(); var random = new Random(32);
                for (int i = 0; i < 10000; i++)
                {
                    p = MovementCore.Move(p, (float)(random.NextDouble() - .5) * 2, (float)(random.NextDouble() - .5) * 2, blocks);
                    Require(MovementCore.Clear(p.X, p.Y, blocks), "Penetração no passo " + i);
                    Require(p.X >= MovementCore.MinX && p.X <= MovementCore.MaxX && p.Y >= MovementCore.MinY && p.Y <= MovementCore.MaxY, "Fora do mapa");
                }
            }, report, ref count);
            Check("Sword combo percorre windup, active e recovery", () =>
            {
                var combo = new SwordCombo();
                Require(combo.Press() && combo.Stage == 1 && combo.Phase == SwordPhase.Windup && combo.Swung, "Golpe 1 não iniciou");
                combo.Tick(.07f);
                Require(combo.Phase == SwordPhase.Active && combo.Struck, "Golpe 1 não entrou em active");
                combo.Tick(.10f);
                Require(combo.Phase == SwordPhase.Recovery && !combo.CanAct, "Golpe 1 não entrou em recovery");
                combo.Tick(.06f);
                Require(combo.CanAct, "Janela de cancelamento do golpe 1 não abriu");
            }, report, ref count);
            Check("Buffer do sword combo encadeia os três golpes", () =>
            {
                var combo = new SwordCombo();
                combo.Press(); combo.Tick(.07f); combo.Press(); combo.Tick(.10f); combo.Tick(.06f);
                Require(combo.Stage == 2 && combo.Phase == SwordPhase.Windup, "Golpe 2 não foi encadeado");
                combo.Tick(.06f); combo.Press(); combo.Tick(.10f); combo.Tick(.07f);
                Require(combo.Stage == 3 && combo.Phase == SwordPhase.Windup, "Golpe 3 não foi encadeado");
            }, report, ref count);
            Check("Cooldowns da espada contam e terminam em zero", () =>
            {
                var combo = new SwordCombo();
                combo.StartWhirlCooldown(); combo.StartDashCooldown();
                combo.Tick(.10f);
                Require(Math.Abs(combo.WhirlRemaining - 5.9f) < .001f, "Cooldown do redemoinho incorreto");
                Require(Math.Abs(combo.DashRemaining - 7.9f) < .001f, "Cooldown da investida incorreto");
                for (int i = 0; i < 80; i++) combo.Tick(.10f);
                Require(combo.WhirlReady && combo.DashReady, "Cooldown não voltou a pronto");
            }, report, ref count);
            count += StoryChecks.Run(report);
            return count;
        }
        static void Check(string name, Action test, Action<string> report, ref int count)
        { test(); count++; report("PASS: " + name); }
        static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); }
    }
}
