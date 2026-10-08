using System;
using UnityEngine;

namespace Vadronia
{
    /// <summary>
    /// Trilha ambiente de Grünwald. Carrega apenas a música da vila:
    /// faixas de cidade e castelo ficam para os mapas correspondentes.
    /// A preferência de mudo sobrevive ao reinício do jogo.
    /// </summary>
    public sealed class VillageMusic : IDisposable
    {
        const string MutedKey = "vadronia.music.muted";
        const float GameVolume = .38f;
        const float PausedVolume = .13f;

        readonly GameObject root;
        readonly AudioSource source;

        public bool Muted { get; private set; }
        public bool Available => source.clip != null;

        public VillageMusic()
        {
            Muted = PlayerPrefs.GetInt(MutedKey, 0) != 0;
            root = new GameObject("Música ambiente — Grünwald");
            source = root.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f; // 2D: não depende da distância até a câmera.
            source.dopplerLevel = 0f;
            source.volume = 0f;
            source.clip = Resources.Load<AudioClip>("Vadronia/Audio/village");
            if (source.clip == null)
                Debug.LogWarning("Trilha da vila ausente: Resources/Vadronia/Audio/village.ogg");
            else
                source.Play();
        }

        public void Tick(float unscaledDeltaTime, bool paused)
        {
            if (!Available) return;
            if (!source.isPlaying) source.Play();
            float target = Muted ? 0f : (paused ? PausedVolume : GameVolume);
            source.volume = Mathf.MoveTowards(source.volume, target, Mathf.Max(0f, unscaledDeltaTime) * .55f);
        }

        public void ToggleMuted()
        {
            Muted = !Muted;
            PlayerPrefs.SetInt(MutedKey, Muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void Dispose()
        {
            if (root != null) UnityEngine.Object.Destroy(root);
        }
    }
}
