using System;
using System.Collections.Generic;

namespace Vadronia
{
    public enum NpcFunction { Rest, Tips, HerbHints, Chat }

    // Pure data, no Unity types: the .NET regression project compiles this file too.
    public sealed class NpcDefinition
    {
        public readonly string Id, Name, Title;
        public readonly NpcFunction Function;
        public readonly FootPoint[] Route;
        public readonly float[] Pause;
        public readonly float Speed, Scale, Red, Green, Blue;
        public readonly string[] Lines;
        public NpcDefinition(string id, string name, string title, NpcFunction function, FootPoint[] route, float[] pause,
            float speed, float scale, float r, float g, float b, string[] lines)
        {
            Id = id; Name = name; Title = title; Function = function; Route = route; Pause = pause;
            Speed = speed; Scale = scale; Red = r; Green = g; Blue = b; Lines = lines;
        }
    }

    public static class NpcRoster
    {
        // Rotas conferidas offline contra TownLayout.Blocks() (ver CoreChecks).
        public static readonly NpcDefinition[] All =
        {
            new NpcDefinition("helga", "HELGA", "Estalajadeira", NpcFunction.Rest,
                new[] { new FootPoint(-4f, 3.8f), new FootPoint(-2.4f, 3.8f), new FootPoint(-3.2f, 3.5f) },
                new[] { 4f, 3f, 5f }, .5f, 1f, 1f, .82f, .82f,
                new[] { "Entre, viajante. A estalagem de Grünwald sempre tem uma cadeira livre. Sente um pouco e recupere o fôlego." }),
            new NpcDefinition("bruno", "BRUNO", "Ferreiro", NpcFunction.Tips,
                new[] { new FootPoint(8.3f, 1.2f), new FootPoint(10.2f, 1.2f), new FootPoint(9.2f, 1.5f) },
                new[] { 3f, 3f, 4f }, .6f, 1.04f, .88f, .88f, 1f,
                new[]
                {
                    "Espada boa se maneja em sequência. Clique com o botão esquerdo e encadeie até três golpes: o terceiro avança mais.",
                    "Mire com o mouse. O golpe vai para onde você aponta, não para onde você anda.",
                    "Corra com Z, mas cuidado com o fôlego. Quem fica sem ar na estrada não luta nem foge.",
                    "Ainda não há lobos nem bandidos por aqui, mas o ferro não espera a ameaça chegar."
                }),
            new NpcDefinition("maren", "MAREN", "Herbalista", NpcFunction.HerbHints,
                new[] { new FootPoint(7.8f, -3.1f), new FootPoint(9f, -3.3f), new FootPoint(8.4f, -2.6f) },
                new[] { 3f, 3f, 4f }, .55f, .97f, .85f, 1f, .85f,
                new[]
                {
                    "Conheço cada canteiro da vila. Se alguém precisar de ervas, é só vir falar comigo.",
                    "Konrad anda atrás de ajuda para juntar ervas. Fale com ele na praça.",
                    "Obrigada por ajudar Konrad. Quem cuida da vila merece um bom chá."
                }),
            new NpcDefinition("lucia", "LÚCIA", "Aldeã", NpcFunction.Chat,
                new[] { new FootPoint(-11.05f, 5.1f), new FootPoint(-11.05f, 6.4f), new FootPoint(-10f, 6.8f), new FootPoint(-10f, 8.2f) },
                new[] { 2f, 3f, 4f, 3f }, .45f, .93f, 1f, .92f, .75f,
                new[]
                {
                    "O jardim do noroeste é meu orgulho. As ervas crescem melhor com a luz da manhã.",
                    "Ouvi dizer que a guilda tem notícias da estrada. Dizem que o caminho ficou mais movimentado.",
                    "Se passar pela praça, beba água do poço. É a melhor de toda a região."
                }),
            new NpcDefinition("tomas", "TOMÁS", "Aldeão", NpcFunction.Chat,
                new[] { new FootPoint(-10.5f, -8.4f), new FootPoint(-8f, -8.4f), new FootPoint(-5.5f, -8.4f), new FootPoint(-8f, -8.2f) },
                new[] { 2f, 3f, 2f, 1f }, .5f, .96f, .78f, .9f, .95f,
                new[]
                {
                    "A viela dos fundos é sossegada. Gosto de passar por aqui no fim do dia.",
                    "A carroça lá no fundo está parada desde a semana passada. Ninguém sabe de quem é.",
                    "Konrad faz a ronda sem parar. Aquele homem nunca descansa."
                })
        };

        static readonly string[] HerbPlaces =
        {
            "a oeste da praça",
            "a sudeste, perto da banca ao sul",
            "no jardim a noroeste"
        };

        // quest: 0 não descoberta · 1 coletando · 2 recompensa entregue. herbMask: bits 0-2 das ervas coletadas.
        public static string Line(NpcDefinition npc, int quest, int herbMask, int talks)
        {
            if (npc == null || npc.Lines == null || npc.Lines.Length == 0) return "";
            if (npc.Function != NpcFunction.HerbHints)
                return npc.Lines[Math.Abs(talks) % npc.Lines.Length];
            if (quest == 0) return npc.Lines[Math.Abs(talks) % 2];
            if (quest >= 2) return npc.Lines[2];
            var missing = new List<string>();
            for (int i = 0; i < HerbPlaces.Length; i++)
                if ((herbMask & (1 << i)) == 0) missing.Add(HerbPlaces[i]);
            if (missing.Count == 0) return "Você já colheu as três porções! Leve tudo para Konrad.";
            return "Ainda falta(m) " + missing.Count + " porção(ões): " + string.Join("; ", missing)
                + ". Chegue perto das ervas e pressione T.";
        }
    }
}
