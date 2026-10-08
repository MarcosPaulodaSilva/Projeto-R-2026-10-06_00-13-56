using System;
using System.Collections.Generic;

namespace Vadronia
{
    // Pure logic checks: also compiled by Tests/Regression.csproj (no Unity needed).
    public static class StoryChecks
    {
        public static int Run(Action<string> report)
        {
            int count = 0;

            Check("Flags de história: únicas, conhecidas e limpas ao carregar", () =>
            {
                var s = new AdventureState();
                Require(s.AddFlag(StoryFlags.VozesMural), "Flag nova deveria ser aceita");
                Require(!s.AddFlag(StoryFlags.VozesMural), "Flag repetida duplicou");
                Require(!s.AddFlag("vozes.inventada") && !s.AddFlag(null) && !s.AddFlag(""), "ID desconhecido entrou no save");
                var dirty = new AdventureProgress();
                dirty.flags = new List<string> { StoryFlags.VozesBanca, null, "lixo", StoryFlags.VozesBanca, StoryFlags.VozesGuilda };
                s.Restore(dirty);
                Require(s.Progress.flags.Count == 2 && s.HasFlag(StoryFlags.VozesBanca) && s.HasFlag(StoryFlags.VozesGuilda), "Limpeza do save incorreta");
                var old = new AdventureProgress();
                old.flags = null; // save antigo, sem o campo
                s.Restore(old);
                Require(s.Progress.flags != null && s.Progress.flags.Count == 0 && !s.HasFlag(StoryFlags.VozesMural), "Save antigo não carregou como vazio");
            }, report, ref count);

            Check("Vozes da vila só começa depois do favor das ervas", () =>
            {
                var s = new AdventureState();
                Require(!Vozes.TryAccept(s), "Aceitou antes de conhecer Konrad");
                s.AcceptQuest();
                Require(!Vozes.TryAccept(s), "Aceitou com as ervas ainda pendentes");
                s.Gather(0); s.Gather(1); s.Gather(2); s.ClaimReward();
                Require(Vozes.TryAccept(s) && !Vozes.TryAccept(s), "Aceite deveria acontecer exatamente uma vez");
            }, report, ref count);

            Check("Vozes da vila paga 15 moedas exatamente uma vez e só com três relatos", () =>
            {
                var s = Ready();
                int coins = s.Progress.coins;
                Require(!Vozes.TryComplete(s), "Concluiu sem nenhum relato");
                s.AddFlag(StoryFlags.VozesMural); s.AddFlag(StoryFlags.VozesBanca);
                Require(!Vozes.TryComplete(s) && Vozes.Heard(s) == 2, "Concluiu com dois relatos");
                s.AddFlag(StoryFlags.VozesGuilda);
                Require(Vozes.TryComplete(s) && s.Progress.coins == coins + Vozes.Reward, "Recompensa incorreta");
                Require(!Vozes.TryComplete(s) && s.Progress.coins == coins + Vozes.Reward, "Recompensa duplicada");
                // Salvar e recarregar não reabre a recompensa.
                var again = new AdventureState();
                again.Restore(new AdventureProgress { quest = s.Progress.quest, herbs = s.Progress.herbs, coins = s.Progress.coins, flags = new List<string>(s.Progress.flags) });
                Require(!Vozes.TryComplete(again) && again.Progress.coins == s.Progress.coins, "Recompensa reaberta após recarregar");
            }, report, ref count);

            Check("Relatos ouvidos antes da conversa contam depois", () =>
            {
                var s = new AdventureState();
                s.AcceptQuest(); s.Gather(0); s.Gather(1); s.Gather(2); s.ClaimReward();
                s.AddFlag(StoryFlags.VozesMural); s.AddFlag(StoryFlags.VozesBanca); s.AddFlag(StoryFlags.VozesGuilda);
                Require(!Vozes.TryComplete(s), "Pagou sem Konrad ter pedido");
                Require(Vozes.TryAccept(s) && Vozes.TryComplete(s), "Relatos antigos não contaram");
            }, report, ref count);

            Check("Diário descreve cada ponto da história", () =>
            {
                string t, x;
                var s = new AdventureState();
                QuestJournal.Describe(s, out t, out x);
                Require(t == "Conheça a vila" && x == "Converse com Konrad na praça.", "Texto inicial mudou");
                s.AcceptQuest();
                QuestJournal.Describe(s, out t, out x);
                Require(t == "Uma pequena ajuda" && x == "Colha ervas nos jardins: 0 / 3", "Texto das ervas mudou");
                s.Gather(0); s.Gather(1); s.Gather(2);
                QuestJournal.Describe(s, out t, out x);
                Require(x == "Volte a Konrad para entregar as ervas.", "Texto de entrega mudou");
                s.ClaimReward();
                QuestJournal.Describe(s, out t, out x);
                Require(t == "Um favor retribuído" && x.Length > 0, "Texto após o favor");
                Vozes.TryAccept(s);
                QuestJournal.Describe(s, out t, out x);
                Require(t == "Vozes da vila" && x.EndsWith("0 / 3"), "Texto da quest ativa");
                s.AddFlag(StoryFlags.VozesMural); s.AddFlag(StoryFlags.VozesBanca); s.AddFlag(StoryFlags.VozesGuilda);
                QuestJournal.Describe(s, out t, out x);
                Require(x == "Volte a Konrad e conte o que ouviu.", "Texto de retorno");
                Vozes.TryComplete(s);
                QuestJournal.Describe(s, out t, out x);
                Require(t == "Relatos anotados", "Texto de conclusão");
            }, report, ref count);

            Check("Falas de Konrad listam só o que falta ouvir", () =>
            {
                Require(GrunwaldStory.KonradAndamento(false, false, false).Contains("o mural de avisos, a banca da praça e a placa da guilda"), "Lista de três");
                Require(GrunwaldStory.KonradAndamento(true, false, true).Contains("a banca da praça") && !GrunwaldStory.KonradAndamento(true, false, true).Contains("mural"), "Lista de um");
                Require(GrunwaldStory.KonradAndamento(false, true, true).Contains("o mural de avisos"), "Lista do mural");
                Require(GrunwaldStory.KonradOferta(0).Length > 0 && GrunwaldStory.KonradOferta(2).Contains("2 relatos") && GrunwaldStory.KonradOferta(1).Contains("1 relato"), "Oferta com relatos já ouvidos");
                Require(GrunwaldStory.KonradConclusao().Contains(Vozes.Reward + " moedas"), "Fala promete outra recompensa");
            }, report, ref count);

            Check("Pontos de leitura: ids únicos, flags válidas, textos presentes e curtos", () =>
            {
                var ids = new HashSet<int>();
                foreach (var p in GrunwaldStory.Points)
                {
                    Require(p.Choice > 6 && ids.Add(p.Choice), "Id de interação repetido ou colide com os existentes: " + p.Choice);
                    Require(Array.IndexOf(StoryFlags.All, p.Flag) >= 0, "Flag desconhecida: " + p.Flag);
                    Require(!string.IsNullOrEmpty(p.Hint) && !string.IsNullOrEmpty(p.Speaker), "Dica ou falante vazio");
                    Require(!string.IsNullOrEmpty(p.Text) && p.Text.Length <= 480, "Texto vazio ou longo demais: " + p.Speaker);
                }
                Require(GrunwaldStory.GuildaText.Length <= 480, "Texto da guilda longo demais");
                Require(Array.IndexOf(StoryFlags.All, StoryFlags.VozesGuilda) >= 0, "Flag da guilda ausente");
            }, report, ref count);

            Check("Pontos de leitura ficam em chão livre e alcançável a pé", () =>
            {
                var blocks = TownLayout.Blocks();
                var spots = new List<float[]>();
                foreach (var p in GrunwaldStory.Points) spots.Add(new[] { p.X, p.Y, p.Radius });
                spots.Add(new[] { 4.3f, 4.25f, 1.3f }); // placa da guilda (VillageInteraction)
                foreach (var spot in spots)
                    Require(MovementCore.Clear(spot[0], spot[1], blocks), "Âncora dentro de obstáculo em " + spot[0] + ", " + spot[1]);
                // Varredura em grade a partir do ponto de partida do jogador (0, -3.9).
                const float step = .25f;
                int nx = (int)((MovementCore.MaxX - MovementCore.MinX) / step) + 1, ny = (int)((MovementCore.MaxY - MovementCore.MinY) / step) + 1;
                var seen = new bool[nx, ny];
                var queue = new Queue<int[]>();
                int si = (int)Math.Round((0 - MovementCore.MinX) / step), sj = (int)Math.Round((-3.9f - MovementCore.MinY) / step);
                Require(MovementCore.Clear(MovementCore.MinX + si * step, MovementCore.MinY + sj * step, blocks), "Partida bloqueada");
                seen[si, sj] = true; queue.Enqueue(new[] { si, sj });
                int[] di = { 1, -1, 0, 0 }, dj = { 0, 0, 1, -1 };
                while (queue.Count > 0)
                {
                    var c = queue.Dequeue();
                    for (int k = 0; k < 4; k++)
                    {
                        int a = c[0] + di[k], b = c[1] + dj[k];
                        if (a < 0 || b < 0 || a >= nx || b >= ny || seen[a, b]) continue;
                        if (!MovementCore.Clear(MovementCore.MinX + a * step, MovementCore.MinY + b * step, blocks)) continue;
                        seen[a, b] = true; queue.Enqueue(new[] { a, b });
                    }
                }
                foreach (var spot in spots)
                {
                    bool reachable = false;
                    for (int a = 0; a < nx && !reachable; a++)
                        for (int b = 0; b < ny; b++)
                        {
                            if (!seen[a, b]) continue;
                            float dx = MovementCore.MinX + a * step - spot[0], dy = MovementCore.MinY + b * step - spot[1];
                            if (dx * dx + dy * dy < spot[2] * spot[2]) { reachable = true; break; }
                        }
                    Require(reachable, "Jogador não alcança a interação em " + spot[0] + ", " + spot[1]);
                }
            }, report, ref count);

            return count;
        }

        // Konrad's herb favour done: the player may now be offered "Vozes da vila".
        static AdventureState Ready()
        {
            var s = new AdventureState();
            s.AcceptQuest(); s.Gather(0); s.Gather(1); s.Gather(2); s.ClaimReward();
            Vozes.TryAccept(s);
            return s;
        }
        static void Check(string name, Action test, Action<string> report, ref int count)
        { test(); count++; report("PASS: " + name); }
        static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); }
    }
}
