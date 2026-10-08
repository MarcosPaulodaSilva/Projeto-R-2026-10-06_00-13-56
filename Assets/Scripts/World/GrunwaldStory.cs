using System.Collections.Generic;

namespace Vadronia
{
    // Pure data, no Unity types: the .NET regression project compiles this file too.

    /// <summary>A place in the village where the player can press T to read or listen.</summary>
    public struct StoryPoint
    {
        public readonly int Choice;
        public readonly string Flag, Hint, Speaker, Text;
        public readonly float X, Y, Radius;
        public StoryPoint(int choice, string flag, float x, float y, float radius, string hint, string speaker, string text)
        { Choice = choice; Flag = flag; X = x; Y = y; Radius = radius; Hint = hint; Speaker = speaker; Text = text; }
    }

    /// <summary>
    /// Text and places of the "Vozes da vila" story beat. Canon rules kept (Master §4, LORE_BIBLE §6):
    /// the ancient civilisation stays unnamed, its inscriptions are never translated, and local accounts
    /// contradict each other without the game choosing one.
    /// </summary>
    public static class GrunwaldStory
    {
        // Interaction ids: 0-6 already belong to VillageInteraction (Konrad, well, herbs, inn, guild plaque).
        public const int GuildaChoice = 6, MuralChoice = 7, BancaChoice = 8;

        public const string GuildaSpeaker = "GUILDA DE GRÜNWALD";
        public const string GuildaText =
            "Registro de viajantes e notícias da estrada. A Guilda recolhe relatos sobre as pedras e ruínas antigas da região, " +
            "sem afirmar o que significam: ninguém decifrou as inscrições. Deixe o que ouvir com a ronda da praça. " +
            "Konrad faz a ronda e orienta quem acabou de chegar.";

        // Anchors must stand on free ground (StoryChecks verifies it against the map's collision blocks).
        public static readonly StoryPoint[] Points =
        {
            new StoryPoint(MuralChoice, StoryFlags.VozesMural, 3.4f, 2.9f, 1f,
                "T  ·  Ler o mural de avisos", "MURAL DE AVISOS",
                "Viajantes: não deixem a estrada depois do pôr do sol e acendam uma luz ao acampar. " +
                "Quem vir uma pedra antiga partida a leste da vila, avise a ronda. " +
                "Dizem uns que ela sempre esteve assim; outros, que rachou há pouco.  — A ronda de Grünwald"),
            new StoryPoint(BancaChoice, StoryFlags.VozesBanca, 3.3f, -2.85f, 1.1f,
                "T  ·  Ouvir a conversa na banca", "NA BANCA",
                "Dois fregueses discutem enquanto escolhem frutas.\n" +
                "— A pedra rachou no inverno passado. Eu vi.\n" +
                "— Inverno passado nada: meu avô já falava dela partida.\n" +
                "— E as marcas que ela tem?\n" +
                "— Ninguém sabe ler aquilo. Quem diz que sabe está inventando."),
        };

        public static string KonradOferta(int alreadyHeard)
        {
            string text =
                "Obrigado de novo pelas ervas. Outra coisa: a Guilda quer relatos sobre uma pedra antiga, partida, a leste da vila. " +
                "Cada um aqui conta uma história diferente, e eu gostaria de saber o que se diz antes que algum viajante vá até lá. " +
                "Passe no mural, na banca da praça e na placa da guilda, e depois me conte.";
            if (alreadyHeard > 0)
                text += " Pelo visto você já ouviu " + alreadyHeard + (alreadyHeard == 1 ? " relato" : " relatos") + " por aí; isso conta.";
            return text;
        }

        public static string KonradAndamento(bool mural, bool banca, bool guilda)
        {
            var missing = new List<string>();
            if (!mural) missing.Add("o mural de avisos");
            if (!banca) missing.Add("a banca da praça");
            if (!guilda) missing.Add("a placa da guilda");
            if (missing.Count == 0) return "Você já ouviu os três relatos. Fale comigo de novo.";
            string list = missing.Count == 1
                ? missing[0]
                : string.Join(", ", missing.GetRange(0, missing.Count - 1)) + " e " + missing[missing.Count - 1];
            return "Ainda falta ouvir " + list + ". Volte quando tiver os três relatos.";
        }

        public static string KonradConclusao()
        {
            return "Então ninguém concorda. Faz sentido: ninguém lê as marcas, e cada geração conta do seu jeito. " +
                   "Vou deixar o que você ouviu com a Guilda, sem dizer qual versão é a certa. " +
                   "Aqui, " + Vozes.Reward + " moedas pelo trabalho. Se for até lá um dia, leve luz e volte antes de escurecer.";
        }

        public const string KonradDepois =
            "Ninguém sabe o que a pedra diz, e está tudo bem. Se um viajante perguntar, agora você sabe o que se conta por aqui.";
    }
}
