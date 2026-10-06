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
            return count;
        }
        static void Check(string name, Action test, Action<string> report, ref int count)
        { test(); count++; report("PASS: " + name); }
        static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); }
    }
}
