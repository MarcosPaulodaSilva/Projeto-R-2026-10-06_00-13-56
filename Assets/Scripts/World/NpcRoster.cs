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
        public readonly float Speed, Scale;
        public readonly NpcLook Look;
        public readonly string[] Lines;
        public NpcDefinition(string id, string name, string title, NpcFunction function, FootPoint[] route, float[] pause,
            float speed, float scale, NpcLook look, string[] lines)
        {
            Id = id; Name = name; Title = title; Function = function; Route = route; Pause = pause;
            Speed = speed; Scale = scale; Look = look; Lines = lines;
        }
    }

    public static class NpcRoster
    {
        // Rotas conferidas offline contra TownLayout.Blocks() (ver CoreChecks).
        public static readonly NpcDefinition[] All =
        {
            new NpcDefinition("helga", "HELGA", "Estalajadeira", NpcFunction.Rest,
                new[] { new FootPoint(11.2f, 5.3f), new FootPoint(12.4f, 5.3f), new FootPoint(11.8f, 5.1f) },
                new[] { 4f, 3f, 5f }, .5f, 1f,
                new NpcLook
                {
                    Skin = 0xE6B38C, Hair = 0x6A4127, Top = 0x8A4A3A, Trim = 0xEBD8B7,
                    Bottom = 0x6C4634, Boots = 0x55331F, Belt = 0x4A2D1B, Apron = 0xD9C7A4,
                    Dress = true, HairStyle = NpcHairStyle.Bun, Headwear = NpcHeadwear.Kerchief, HeadColor = 0xB95B57
                },
                new[] { "Entre, viajante. A estalagem de Grünwald sempre tem uma cadeira livre. Sente um pouco e recupere o fôlego." }),
            new NpcDefinition("bruno", "BRUNO", "Ferreiro", NpcFunction.Tips,
                new[] { new FootPoint(8.3f, 1.2f), new FootPoint(10.2f, 1.2f), new FootPoint(9.2f, 1.5f) },
                new[] { 3f, 3f, 4f }, .6f, 1.04f,
                new NpcLook
                {
                    Skin = 0xC98F68, Hair = 0x3A271E, Top = 0x59636C, Trim = 0xC9B28F,
                    Bottom = 0x3F4347, Boots = 0x4E3020, Belt = 0x33231B,
                    Beard = true, Rolled = true, Wide = 2, HairStyle = NpcHairStyle.Short
                },
                new[]
                {
                    "Espada boa se maneja em sequência. Clique com o botão esquerdo e encadeie até três golpes: o terceiro avança mais.",
                    "Mire com o mouse. O golpe vai para onde você aponta, não para onde você anda.",
                    "Corra com Z, mas cuidado com o fôlego. Quem fica sem ar na estrada não luta nem foge.",
                    "Ainda não há lobos nem bandidos por aqui, mas o ferro não espera a ameaça chegar."
                }),
            new NpcDefinition("maren", "MAREN", "Herbalista", NpcFunction.HerbHints,
                new[] { new FootPoint(7.8f, -3.1f), new FootPoint(9f, -3.3f), new FootPoint(8.4f, -2.6f) },
                new[] { 3f, 3f, 4f }, .55f, .97f,
                new NpcLook
                {
                    Skin = 0xD9A47D, Hair = 0x6A4A2C, Top = 0x5F7B4D, Trim = 0xD9D5A7,
                    Bottom = 0x5A4A35, Boots = 0x4D3423, Belt = 0x4A3523, Bag = true,
                    BagColor = 0x7D5A35, HairStyle = NpcHairStyle.Braid
                },
                new[]
                {
                    "Conheço cada canteiro da vila. Se alguém precisar de ervas, é só vir falar comigo.",
                    "Konrad anda atrás de ajuda para juntar ervas. Fale com ele na praça.",
                    "Obrigada por ajudar Konrad. Quem cuida da vila merece um bom chá."
                }),
            new NpcDefinition("lucia", "LÚCIA", "Aldeã", NpcFunction.Chat,
                new[] { new FootPoint(-11.05f, 5.1f), new FootPoint(-11.05f, 6.4f), new FootPoint(-10f, 6.8f), new FootPoint(-10f, 8.2f) },
                new[] { 2f, 3f, 4f, 3f }, .45f, .93f,
                new NpcLook
                {
                    Skin = 0xE7B48C, Hair = 0x7B5334, Top = 0x8F6B57, Trim = 0xE6D5B8,
                    Bottom = 0x6D5A48, Boots = 0x5C402A, Belt = 0x4D3324, Dress = true,
                    Skirt = 0xA0785E, HairStyle = NpcHairStyle.Long, Headwear = NpcHeadwear.StrawHat, HeadColor = 0xC9A85B
                },
                new[]
                {
                    "O jardim do noroeste é meu orgulho. As ervas crescem melhor com a luz da manhã.",
                    "Ouvi dizer que a guilda tem notícias da estrada. Dizem que o caminho ficou mais movimentado.",
                    "Se passar pela praça, beba água do poço. É a melhor de toda a região."
                }),
            new NpcDefinition("tomas", "TOMÁS", "Aldeão", NpcFunction.Chat,
                new[] { new FootPoint(-10.5f, -8.4f), new FootPoint(-8f, -8.4f), new FootPoint(-5.5f, -8.4f), new FootPoint(-8f, -8.2f) },
                new[] { 2f, 3f, 2f, 1f }, .5f, .96f,
                new NpcLook
                {
                    Skin = 0xC98F6B, Hair = 0x4A3324, Top = 0x4E6675, Trim = 0xC8D1C5,
                    Bottom = 0x4D4B45, Boots = 0x49301F, Belt = 0x3F2A1C, Bag = true,
                    BagColor = 0x755131, HairStyle = NpcHairStyle.Short
                },
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
