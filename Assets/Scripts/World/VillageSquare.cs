using System;
using UnityEngine;

namespace Vadronia
{
    /// <summary>
    /// A praça é um único sprite de pedras artesanais gerado uma vez no carregamento.
    /// A área central cobre completamente o piso retangular antigo; recortes com
    /// dithering unem o calçamento às estradas de terra já pintadas;
    /// A resolução por unidade corresponde exatamente à textura do terreno;
    /// sem blocos de pedra como GameObjects e sem atualizações por frame.
    /// </summary>
    public sealed class VillageSquare : IDisposable
    {
        // terrain-v3 mede 896x704 em 28x22 unidades: 32 pixels/unidade.
        // Um pixel da praça tem a mesma escala de um pixel do terreno.
        const int Ppu = 32;
        const int Width = 512, Height = 416;
        readonly GameObject root;
        readonly Texture2D texture;
        readonly Sprite sprite;

        // Tons quentes de pedra: as casas, o poço e a ferraria pertencem à mesma vila.
        static readonly Color32[] Stones =
        {
            new Color32(162, 148, 119, 255),
            new Color32(176, 160, 126, 255),
            new Color32(153, 144, 119, 255),
            new Color32(169, 154, 128, 255),
            new Color32(148, 139, 111, 255),
            new Color32(181, 166, 136, 255),
            new Color32(156, 149, 127, 255),
            new Color32(171, 153, 115, 255),
        };

        public VillageSquare(Transform parent)
        {
            texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                name = "Praça de Grünwald — pedra irregular",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(Paint());
            texture.Apply(false, true);
            sprite = Sprite.Create(texture, new Rect(0, 0, Width, Height),
                new Vector2(.5f, .5f), Ppu, 0, SpriteMeshType.FullRect);
            root = new GameObject("Praça — pedras e acessos");
            root.transform.SetParent(parent, false);
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -19500; // Sobre o terreno, atrás de canteiros, NPCs e objetos.
        }

        static int Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint v = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
                v = (v ^ (v >> 13)) * 1274126177;
                return (int)((v ^ (v >> 16)) & 0x7fffffff);
            }
        }

        static float RoundedBox(float x, float y, float hx, float hy, float radius)
        {
            float dx = Mathf.Abs(x) - hx;
            float dy = Mathf.Abs(y) - hy;
            return Mathf.Sqrt(Mathf.Max(0, dx) * Mathf.Max(0, dx) +
                              Mathf.Max(0, dy) * Mathf.Max(0, dy))
                   + Mathf.Min(Mathf.Max(dx, dy), 0) - radius;
        }

        static float Path(float x, float y, float ax, float ay, float bx, float by, float radius)
        {
            float vx = bx - ax, vy = by - ay;
            float t = Mathf.Clamp01(((x - ax) * vx + (y - ay) * vy) / (vx * vx + vy * vy));
            float dx = x - (ax + vx * t), dy = y - (ay + vy * t);
            return Mathf.Sqrt(dx * dx + dy * dy) - radius;
        }

        static float Surface(float x, float y)
        {
            // Corpo da praça arredondado. Acesso norte para a guilda, sul para
            // os caminhos da vila; acessos laterais encontram as ruas de terra.
            float stone = RoundedBox(x, y - .08f, 4.65f, 3.35f, 1.08f);
            stone = Mathf.Min(stone, Path(x, y, 0, 2.75f, 0, 4.70f, 1.28f));
            stone = Mathf.Min(stone, Path(x, y, 0, -2.90f, 0, -4.95f, 1.36f));
            stone = Mathf.Min(stone, Path(x, y, -4.65f, -.9f, -6.55f, -1.12f, .75f));
            stone = Mathf.Min(stone, Path(x, y, 4.65f, -.9f, 6.55f, -1.12f, .75f));
            return stone;
        }

        static Color32[] Paint()
        {
            var colors = new Color32[Width * Height];
            for (int py = 0; py < Height; py++)
            {
                float y = (py - Height * .5f) / Ppu;
                // Fiadas desencontradas, pedras de cerca de 15x9 pixels do terreno.
                int row = py / 9;
                int rowLocal = py - row * 9;
                int rowShift = (row & 1) * 8 + Hash(0, row, 2) % 4;
                for (int px = 0; px < Width; px++)
                {
                    float x = (px - Width * .5f) / Ppu;
                    float contour = Surface(x, y);
                    contour += (Hash(px / 7, py / 7, 31) % 101 - 50) * .0019f;
                    contour += Mathf.Sin(y * 9f + x * 3f) * .044f;

                    // Pedras vão se espaçando na borda: não há outra textura
                    // quadrada sobreposta ao chão, nem faixa lisa cinza.
                    float coverage = Mathf.Clamp01((.56f - contour) / .56f);
                    if (coverage <= 0) continue;
                    int grain = Hash(px, py, 47) % 1000;
                    if (coverage < 1 && grain >= coverage * 1000f) continue;

                    int shiftedX = px + rowShift;
                    int col = shiftedX / 15;
                    int localX = shiftedX - col * 15;
                    int seed = Hash(col, row, 18);
                    // Recuos nos quatro cantos quebram o aspecto de ladrilhos perfeitos.
                    bool chipped = (rowLocal == 1 || rowLocal == 7) &&
                                   (localX == 1 || localX == 13) && (seed % 5 != 0);
                    bool mortar = localX == 0 || localX == 14 ||
                                  rowLocal == 0 || rowLocal == 8 || chipped;
                    var stoneColor = Stones[seed % Stones.Length];
                    int grit = Hash(px / 2, py / 2, 72) % 11 - 5;
                    int bevel = rowLocal >= 7 ? 7 : rowLocal <= 2 ? -6 : 0;

                    if (mortar)
                    {
                        bool moss = Hash(col, row, 76) % 9 == 0;
                        colors[py * Width + px] = moss
                            ? new Color32(103, 114, 88, 255)
                            : new Color32(113, 109, 91, 255);
                    }
                    else
                    {
                        int light = grit + bevel;
                        // Círculo discreto de pedra clara ao redor do poço,
                        // sem bloquear o seu acesso nem alterar seu colisor.
                        float distanceSq = x * x + y * y;
                        if (distanceSq > .95f * .95f && distanceSq < 1.55f * 1.55f)
                            light += 7;
                        colors[py * Width + px] = new Color32(
                            (byte)Mathf.Clamp(stoneColor.r + light, 0, 255),
                            (byte)Mathf.Clamp(stoneColor.g + light, 0, 255),
                            (byte)Mathf.Clamp(stoneColor.b + light, 0, 255), 255);
                    }
                }
            }
            return colors;
        }

        public void Dispose()
        {
            if (root != null) UnityEngine.Object.Destroy(root);
            if (sprite != null) UnityEngine.Object.Destroy(sprite);
            if (texture != null) UnityEngine.Object.Destroy(texture);
        }
    }
}
