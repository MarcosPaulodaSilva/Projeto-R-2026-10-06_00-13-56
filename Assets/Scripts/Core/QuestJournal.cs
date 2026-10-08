using System;
using System.Collections.Generic;

namespace Vadronia
{
    // Pure rules, no Unity types: the .NET regression project compiles this file too.

    /// <summary>
    /// Stable IDs of story facts saved with the adventure (Master §12-13). Never rename or reuse an ID:
    /// old saves refer to them. Unknown IDs are dropped on load, so add new ones here first.
    /// </summary>
    public static class StoryFlags
    {
        public const int MaxFlags = 64;
        public const string VozesAceita = "vozes.aceita";
        public const string VozesMural = "vozes.mural";
        public const string VozesBanca = "vozes.banca";
        public const string VozesGuilda = "vozes.guilda";
        public const string VozesConcluida = "vozes.concluida";

        public static readonly string[] All =
        {
            VozesAceita, VozesMural, VozesBanca, VozesGuilda, VozesConcluida
        };

        /// <summary>Distinct, known IDs only; never null. Used when a save is loaded.</summary>
        public static List<string> Clean(List<string> raw)
        {
            var clean = new List<string>();
            if (raw == null) return clean;
            foreach (var id in raw)
                if (id != null && Array.IndexOf(All, id) >= 0 && !clean.Contains(id) && clean.Count < MaxFlags) clean.Add(id);
            return clean;
        }
    }

    /// <summary>
    /// Quest "Vozes da vila" (id grunwald.vozes): after Konrad's herb favour, listen to what the village
    /// says about the broken ancient stone. Three accounts that disagree; nothing is confirmed.
    /// </summary>
    public static class Vozes
    {
        public const int Needed = 3;
        public const int Reward = 15;

        public static int Heard(AdventureState state)
        {
            return (state.HasFlag(StoryFlags.VozesMural) ? 1 : 0)
                 + (state.HasFlag(StoryFlags.VozesBanca) ? 1 : 0)
                 + (state.HasFlag(StoryFlags.VozesGuilda) ? 1 : 0);
        }

        public static bool Active(AdventureState state)
        {
            return state.HasFlag(StoryFlags.VozesAceita) && !state.HasFlag(StoryFlags.VozesConcluida);
        }

        /// <summary>Konrad offers the quest. Only after the herb favour is done; false if already accepted.</summary>
        public static bool TryAccept(AdventureState state)
        {
            if (state.Progress.quest != 2 || state.HasFlag(StoryFlags.VozesAceita)) return false;
            return state.AddFlag(StoryFlags.VozesAceita);
        }

        /// <summary>Pays the reward exactly once, and only when all three accounts were heard.</summary>
        public static bool TryComplete(AdventureState state)
        {
            if (state.Progress.quest != 2 || !Active(state) || Heard(state) < Needed) return false;
            state.AddFlag(StoryFlags.VozesConcluida);
            state.Progress.coins = Math.Min(999999, state.Progress.coins + Reward);
            return true;
        }
    }

    /// <summary>Text of the travel journal (HUD) for the current point of the story.</summary>
    public static class QuestJournal
    {
        public static void Describe(AdventureState state, out string title, out string text)
        {
            int quest = state.Progress.quest;
            if (quest == 0)
            {
                title = "Conheça a vila";
                text = "Converse com Konrad na praça.";
            }
            else if (quest == 1)
            {
                title = "Uma pequena ajuda";
                text = state.HerbCount == 3
                    ? "Volte a Konrad para entregar as ervas."
                    : "Colha ervas nos jardins: " + state.HerbCount + " / 3";
            }
            else if (state.HasFlag(StoryFlags.VozesConcluida))
            {
                title = "Relatos anotados";
                text = "Konrad anotou o que você ouviu. Explore Grünwald.";
            }
            else if (state.HasFlag(StoryFlags.VozesAceita))
            {
                title = "Vozes da vila";
                text = Vozes.Heard(state) >= Vozes.Needed
                    ? "Volte a Konrad e conte o que ouviu."
                    : "Ouça o que se diz da pedra partida: mural, banca e placa da guilda  " + Vozes.Heard(state) + " / " + Vozes.Needed;
            }
            else
            {
                title = "Um favor retribuído";
                text = "Konrad agradeceu sua ajuda. Fale com ele de novo quando quiser.";
            }
        }
    }
}
