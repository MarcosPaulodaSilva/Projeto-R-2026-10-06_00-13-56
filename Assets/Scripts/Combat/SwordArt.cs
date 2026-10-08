using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vadronia
{
    /// <summary>Sprites gerados em código (sem arquivos de arte): espada comum, arco de corte e anel.</summary>
    public sealed class SwordArt : IDisposable
    {
        readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        public readonly Sprite Blade, Arc, Ring;

        public SwordArt()
        {
            Blade = MakeBlade();
            Arc = MakeArc(false);
            Ring = MakeArc(true);
        }

        Sprite Finish(Texture2D texture, Vector2 pivot, float ppu)
        {
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.Apply(false, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot, ppu);
            owned.Add(texture); owned.Add(sprite);
            return sprite;
        }

        // Espada 16x64 px, pivô no cabo. Lâmina aponta para cima (+Y).
        Sprite MakeBlade()
        {
            const int W = 16, H = 64;
            var px = new Color32[W * H];
            var steelL = new Color32(214, 224, 238, 255); var steelC = new Color32(246, 250, 255, 255);
            var steelD = new Color32(150, 163, 184, 255); var steelE = new Color32(104, 116, 138, 255);
            var gold = new Color32(232, 190, 84, 255); var goldD = new Color32(168, 126, 44, 255);
            var grip = new Color32(104, 66, 40, 255); var gripD = new Color32(70, 42, 26, 255);

            for (int y = 0; y < 3; y++) for (int x = 6; x <= 9; x++) px[y * W + x] = (x == 6 || y == 0) ? goldD : gold;           // pomo
            for (int y = 3; y < 14; y++) for (int x = 7; x <= 8; x++) px[y * W + x] = (y % 3 == 0) ? gripD : grip;                // cabo
            for (int y = 14; y < 17; y++) for (int x = 2; x <= 13; x++) px[y * W + x] = (y == 14 || x <= 3 || x >= 12) ? goldD : gold; // guarda
            for (int y = 17; y < 62; y++)                                                                                          // lâmina
            {
                if (y <= 52)
                {
                    px[y * W + 6] = steelL; px[y * W + 7] = steelC; px[y * W + 8] = steelD; px[y * W + 9] = steelE;
                }
                else if (y <= 60)
                {
                    px[y * W + 7] = steelC; px[y * W + 8] = steelD;
                }
                else px[y * W + 7] = steelC;
            }

            // contorno escuro de 1 px para leitura sobre o cenário
            var outline = new Color32(26, 28, 38, 255);
            var result = (Color32[])px.Clone();
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    if (px[y * W + x].a != 0) continue;
                    bool near = (x > 0 && px[y * W + x - 1].a != 0) || (x < W - 1 && px[y * W + x + 1].a != 0)
                        || (y > 0 && px[(y - 1) * W + x].a != 0) || (y < H - 1 && px[(y + 1) * W + x].a != 0);
                    if (near) result[y * W + x] = outline;
                }

            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixels32(result);
            return Finish(tex, new Vector2(.5f, 8f / H), 56f);
        }

        // Arco (crescente apontando para +X, ±80°) ou anel completo. 128 px = 1 unidade de diâmetro.
        Sprite MakeArc(bool ring)
        {
            const int N = 128;
            var px = new Color32[N * N];
            float maxAngle = 80f * Mathf.Deg2Rad;
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    float dx = (x + .5f - N / 2f) / (N / 2f), dy = (y + .5f - N / 2f) / (N / 2f);
                    float r = Mathf.Sqrt(dx * dx + dy * dy), a = 0;
                    if (r <= 1f)
                    {
                        float ang = Mathf.Atan2(dy, dx);
                        if (ring)
                        {
                            if (r >= .82f)
                            {
                                a = Mathf.Sin(Mathf.Clamp01((r - .82f) / .18f) * Mathf.PI * .5f);
                                a *= .35f + .65f * (.5f + .5f * Mathf.Cos(ang * 3f));
                            }
                        }
                        else
                        {
                            float t = Mathf.Abs(ang) / maxAngle;
                            if (t <= 1f)
                            {
                                float inner = .5f + .45f * Mathf.Pow(t, 1.6f);
                                if (r >= inner) a = Mathf.Pow(Mathf.Clamp01((r - inner) / (1f - inner)), .65f);
                            }
                        }
                        a *= Mathf.Clamp01((1f - r) * 24f);
                    }
                    px[y * N + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(a) * 255f));
                }
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            tex.SetPixels32(px);
            return Finish(tex, new Vector2(.5f, .5f), N);
        }

        public void Dispose() { foreach (var item in owned) if (item != null) UnityEngine.Object.Destroy(item); }
    }
}