using System;
using UnityEngine;

namespace Vadronia
{
    /// <summary>
    /// Praça de Grünwald: base de terra para ocultar o calçamento antigo,
    /// cobblestones irregulares e caminhos que entram organicamente na praça.
    /// Dois sprites produzidos apenas uma vez ao abrir a cena; sem Tick e
    /// sem GameObjects individuais para as pedras.
    /// </summary>
    public sealed class VillageSquare : IDisposable
    {
        // O terrain-v3 mede 896x704 em 28x22 unidades: 32 pixels por unidade.
        const int Ppu = 32, Width = 576, Height = 480;
        readonly GameObject root;
        readonly Texture2D soilTexture, stoneTexture;
        readonly Sprite soilSprite, stoneSprite;

        static readonly Color32[] Paving =
        {
            new Color32(180, 164, 133, 255),
            new Color32(173, 157, 126, 255),
            new Color32(190, 172, 139, 255),
            new Color32(161, 153, 130, 255),
            new Color32(178, 161, 131, 255),
            new Color32(166, 151, 121, 255),
            new Color32(193, 174, 141, 255),
            new Color32(168, 160, 137, 255)
        };

        public VillageSquare(Transform parent)
        {
            root = new GameObject("Praça de Grünwald — piso integrado");
            root.transform.SetParent(parent, false);
            soilTexture = MakeTexture("Terra sob a praça", PaintSoil());
            stoneTexture = MakeTexture("Pedras antigas da praça", PaintStones());
            soilSprite = AddLayer("Base de terra — remove retângulo antigo", soilTexture, -19800);
            stoneSprite = AddLayer("Calçamento irregular e caminhos", stoneTexture, -19500);
        }

        static Texture2D MakeTexture(string name, Color32[] pixels)
        {
            var result = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            result.SetPixels32(pixels);
            result.Apply(false, true);
            return result;
        }

        Sprite AddLayer(string name, Texture2D texture, int order)
        {
            var sprite = Sprite.Create(texture, new Rect(0, 0, Width, Height),
                new Vector2(.5f, .5f), Ppu, 0, SpriteMeshType.FullRect);
            var layer = new GameObject(name);
            layer.transform.SetParent(root.transform, false);
            var renderer = layer.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order; // Solo -20000, decoração plana -18000.
            return sprite;
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


        static float Path(float x, float y, float ax, float ay, float bx, float by, float radius)
        {
            float dx = bx - ax, dy = by - ay;
            float t = Mathf.Clamp01(((x - ax) * dx + (y - ay) * dy) / (dx * dx + dy * dy));
            float rx = x - (ax + t * dx), ry = y - (ay + t * dy);
            return Mathf.Sqrt(rx * rx + ry * ry) - radius;
        }

        static float PlazaDistance(float x, float y)
        {
            // Contorno superelíptico: praça medieval larga no miolo, afunilando
            // nos quatro cantos. Não há lados retilíneos nem textura quadrada.
            // Feiras nas laterais, poço no centro e caminhos conectados.
            float xNorm = Mathf.Abs(x + .06f) / (4.85f + .10f * Mathf.Sin(y * 1.8f));
            float yNorm = Mathf.Abs(y - .02f) / 3.55f;
            const float Shape = 2.45f;
            float d = (Mathf.Pow(Mathf.Pow(xNorm, Shape) +
                                 Mathf.Pow(yNorm, Shape), 1f / Shape) - 1f) * 3.65f;
            d = Mathf.Min(d, Path(x, y, 0, 2.25f, .05f, 5.15f, 1.38f));
            d = Mathf.Min(d, Path(x, y, 0, -2.2f, -.13f, -5.18f, 1.42f));
            d = Mathf.Min(d, Path(x, y, -3.55f, -.78f, -6.55f, -1.16f, .88f));
            d = Mathf.Min(d, Path(x, y, 3.55f, -.78f, 6.55f, -1.16f, .88f));
            // Borda esburacada, como pedras colocadas em épocas diferentes.
            d += Mathf.Sin(x * 3.05f + y * 2.27f) * .14f;
            d += Mathf.Sin(x * 7.14f - y * 5.31f) * .066f;
            return d;
        }

        static Color32[] PaintSoil()
        {
            var pixels = new Color32[Width * Height];
            for (int py = 0; py < Height; py++)
            {
                float y = (py - Height * .5f) / Ppu;
                for (int px = 0; px < Width; px++)
                {
                    float x = (px - Width * .5f) / Ppu;
                    // O terreno anterior já tem uma praça retangular em x≈±4.3,
                    // y≈-3.6..4.3. Ocultamos esse retângulo POR INTEIRO antes
                    // de recortar a nova área de pedras; não se verá uma borda
                    // cinzenta antiga nas lacunas do pavimento.
                    float outsideX = Mathf.Max(0, Mathf.Abs(x) - 4.42f);
                    float outsideY = Mathf.Max(0, Mathf.Max(y - 4.45f, -3.78f - y));
                    float outside = Mathf.Max(outsideX, outsideY);
                    float coverage = Mathf.Clamp01((.65f - outside) / .48f);
                    if (coverage <= 0f) continue;
                    int grain = Hash(px, py, 101) % 1000;
                    if (coverage < 1f && grain >= coverage * 1000f) continue;

                    float d = PlazaDistance(x, y);
                    int h = Hash(px / 3, py / 3, 55);
                    int tiny = Hash(px, py, 56) % 13 - 6;
                    // Solo pisado em volta do poço; vegetação reaparece nas
                    // bordas, onde não há pedra. Nada de placa cinzenta.
                    int greenChance = Mathf.RoundToInt(Mathf.Clamp01((d + .10f) / 1.5f) * 47f);
                    bool green = h % 100 < greenChance;
                    Color32 color;
                    if (green)
                    {
                        int var = (h / 100) % 25;
                        color = new Color32((byte)(105 + var / 2), (byte)(122 + var), (byte)(57 + var / 4), 255);
                    }
                    else
                    {
                        int var = (h / 100) % 17;
                        color = new Color32((byte)(166 + var / 2), (byte)(137 + var / 2), (byte)(91 + var / 3), 255);
                    }
                    // Pequenos grãos e seixos da mesma escala do chão original.
                    if (Hash(px, py, 44) % 79 == 0)
                        color = new Color32(190, 177, 140, 255);
                    else
                        color = new Color32(
                            (byte)Mathf.Clamp(color.r + tiny, 0, 255),
                            (byte)Mathf.Clamp(color.g + tiny, 0, 255),
                            (byte)Mathf.Clamp(color.b + tiny, 0, 255), 255);
                    pixels[py * Width + px] = color;
                }
            }
            return pixels;
        }

        static Color32[] PaintStones()
        {
            var pixels = new Color32[Width * Height];
            // Pedras com comprimentos/larguras diferentes, não um grid regular
            // de ladrilhos. Os limites são calculados uma vez por fiada/pedra.
            int row = -1, rowStart = 0, rowHeight = 0;
            for (int py = 0; py < Height; py++)
            {
                if (py >= rowStart + rowHeight)
                {
                    rowStart += rowHeight;
                    row++;
                    rowHeight = 8 + Hash(0, row, 19) % 5;
                }
                float y = (py - Height * .5f) / Ppu;
                int rowLocal = py - rowStart;
                int col = 0;
                int stoneStart = -((row & 1) * 7 + Hash(0, row, 17) % 5);
                int stoneWidth = 11 + Hash(col, row, 41) % 6;
                for (int px = 0; px < Width; px++)
                {
                    while (px >= stoneStart + stoneWidth)
                    {
                        stoneStart += stoneWidth;
                        col++;
                        stoneWidth = 11 + Hash(col, row, 41) % 6;
                    }
                    float x = (px - Width * .5f) / Ppu;
                    float d = PlazaDistance(x, y);
                    d += (Hash(px / 6, py / 6, 88) % 101 - 50) * .003f;
                    float coverage = Mathf.Clamp01((.42f - d) / .80f);
                    if (coverage <= 0f) continue;
                    // Espaçar pedras em meio à terra torna o limite irregular
                    // de verdade, e não apenas uma linha com transparência.
                    if (coverage < 1f && Hash(px, py, 70) % 1000 > coverage * 1000f)
                        continue;

                    int localX = px - stoneStart;
                    int seed = Hash(col, row, 23);
                    bool cornerChip = (localX == 1 || localX == stoneWidth - 2) &&
                        (rowLocal == 1 || rowLocal == rowHeight - 2) && seed % 4 != 0;
                    bool mortar = localX == 0 || localX == stoneWidth - 1 ||
                                  rowLocal == 0 || rowLocal == rowHeight - 1 || cornerChip;
                    bool moss = seed % 9 == 0 &&
                        (localX <= 2 || localX >= stoneWidth - 3) && rowLocal <= 2;
                    int grit = Hash(px / 2, py / 2, 61) % 11 - 5;
                    int bevel = rowLocal >= rowHeight - 3 ? 6 : rowLocal <= 2 ? -7 : 0;
                    if (mortar)
                        pixels[py * Width + px] = moss ?
                            new Color32(106, 113, 83, 255) :
                            new Color32(119, 111, 89, 255);
                    else
                    {
                        var c = Paving[seed % Paving.Length];
                        int light = grit + bevel;
                        // Anel gasto de pedras um pouco mais claras em volta do poço.
                        float radius2 = x * x + y * y;
                        if (radius2 > 1.05f * 1.05f && radius2 < 1.65f * 1.65f)
                            light += 7;
                        pixels[py * Width + px] = new Color32(
                            (byte)Mathf.Clamp(c.r + light, 0, 255),
                            (byte)Mathf.Clamp(c.g + light, 0, 255),
                            (byte)Mathf.Clamp(c.b + light, 0, 255), 255);
                    }
                }
            }
            return pixels;
        }

        public void Dispose()
        {
            if (root != null) UnityEngine.Object.Destroy(root);
            if (soilSprite != null) UnityEngine.Object.Destroy(soilSprite);
            if (stoneSprite != null) UnityEngine.Object.Destroy(stoneSprite);
            if (soilTexture != null) UnityEngine.Object.Destroy(soilTexture);
            if (stoneTexture != null) UnityEngine.Object.Destroy(stoneTexture);
        }
    }
}
