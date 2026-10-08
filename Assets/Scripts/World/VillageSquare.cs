using System;
using UnityEngine;

namespace Vadronia
{
    /// <summary>
    /// Calçamento unificado da praça: um único sprite gerado na inicialização,
    /// sem GameObjects por pedra e sem cálculos por frame.
    /// O contorno irregular mistura a praça com o terreno existente.
    /// </summary>
    public sealed class VillageSquare : IDisposable
    {
        const int PixelsPerUnit = 64;
        const int Width = 704, Height = 448;
        readonly GameObject root;
        readonly Texture2D texture;
        readonly Sprite sprite;

        static readonly Color32[] Stone =
        {
            new Color32(145, 142, 120, 255),
            new Color32(154, 151, 128, 255),
            new Color32(130, 137, 119, 255),
            new Color32(166, 158, 129, 255),
            new Color32(140, 145, 130, 255),
            new Color32(154, 145, 121, 255),
        };

        public VillageSquare(Transform parent)
        {
            texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                name = "Calçamento de Grünwald",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(Paint());
            texture.Apply(false, true);
            sprite = Sprite.Create(texture, new Rect(0, 0, Width, Height), new Vector2(.5f, .5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);
            root = new GameObject("Praça de pedra — calçamento");
            root.transform.SetParent(parent, false);
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -19500; // Sobre terreno, sob canteiros, sombras e construções.
        }

        static int Hash(int x, int y, int salt)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + salt * 1442695041);
                h = (h ^ (h >> 13)) * 1274126177;
                return (int)((h ^ (h >> 16)) & 0x7fffffff);
            }
        }

        static Color32[] Paint()
        {
            var pixels = new Color32[Width * Height];
            for (int py = 0; py < Height; py++)
            {
                float y = (py - Height * .5f) / PixelsPerUnit;
                int row = py / 16;
                int offset = (row & 1) * 13;
                for (int px = 0; px < Width; px++)
                {
                    float x = (px - Width * .5f) / PixelsPerUnit;

                    // Retângulo de cantos arredondados, não uma mancha quadrada de textura.
                    float dx = Mathf.Abs(x) - 4.15f;
                    float dy = Mathf.Abs(y) - 2.22f;
                    float sdf = Mathf.Sqrt(Mathf.Max(dx, 0) * Mathf.Max(dx, 0) +
                                           Mathf.Max(dy, 0) * Mathf.Max(dy, 0))
                                + Mathf.Min(Mathf.Max(dx, dy), 0) - .58f;
                    float ragged = (Hash(px / 18, py / 18, 8) % 100 - 50) * .0012f;
                    float coverage = Mathf.Clamp01((-sdf + ragged + .06f) / .22f);
                    if (coverage <= 0) continue;

                    int col = (px + offset) / 25;
                    int localX = (px + offset) % 25;
                    int localY = py % 16;
                    int h = Hash(col, row, 23);
                    int mortar = Mathf.Min(Mathf.Min(localX, 24 - localX), Mathf.Min(localY, 15 - localY));
                    var color = mortar < 2
                        ? new Color32(103, 105, 91, 255)
                        : Stone[h % Stone.Length];

                    // Luz na aresta superior e variações pequenas entre pedras.
                    int light = mortar >= 2 && localY >= 12 ? 8 : (mortar >= 2 && localY <= 3 ? -8 : 0);
                    light += (Hash(col, row, 47) % 9) - 4;
                    if (mortar < 2 && (h % 7 == 0))
                        color = new Color32(104, 119, 92, 255); // musgo nas juntas
                    else
                        color = new Color32(
                            (byte)Mathf.Clamp(color.r + light, 0, 255),
                            (byte)Mathf.Clamp(color.g + light, 0, 255),
                            (byte)Mathf.Clamp(color.b + light, 0, 255), 255);

                    // Transparência nos limites deixa a mesma terra original à mostra.
                    color.a = (byte)Mathf.RoundToInt(233f * coverage);
                    pixels[py * Width + px] = color;
                }
            }
            return pixels;
        }

        public void Dispose()
        {
            if (root != null) UnityEngine.Object.Destroy(root);
            if (sprite != null) UnityEngine.Object.Destroy(sprite);
            if (texture != null) UnityEngine.Object.Destroy(texture);
        }
    }
}
