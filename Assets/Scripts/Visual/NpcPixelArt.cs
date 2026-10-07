using System;

namespace Vadronia
{
    public enum NpcHairStyle { Short, Long, Bun, Braid, Bald }
    public enum NpcHeadwear { None, Kerchief, StrawHat }

    /// <summary>
    /// Aparência de um NPC de Grünwald. Dados puros (cores 0xRRGGBB, -1 = ausente): o .NET de regressão compila este arquivo.
    /// </summary>
    public sealed class NpcLook
    {
        public int Skin = 0xE8B890, Hair = 0x5A3A22, Top = 0x7A4A2A, Trim = 0xE8DCC0, Bottom = 0x4A3A30,
            Boots = 0x6A4426, Belt = 0x4A2E1A, HeadColor = 0xE8DCC0, BagColor = 0x8A5A32;
        public int Apron = -1, Skirt = -1;
        public bool Dress, Beard, Rolled, Bag;
        /// <summary>Pixels extras de meia-largura do tronco (0 = normal, 2 = corpulento).</summary>
        public int Wide;
        public NpcHairStyle HairStyle;
        public NpcHeadwear Headwear;
    }

    /// <summary>
    /// Pintor procedural de pixel art 32x64 para NPCs: 4 direções (0 frente, 1 direita, 2 costas, 3 esquerda) x 4 quadros
    /// de caminhada (0 e 2 = passos, 1 e 3 = passagem/parado). Sem Unity: devolve pixels, o CharacterView cria a textura.
    /// Arte temporária de sistema (placeholder válido pelo Master Prompt §16); a arte final virá depois.
    /// </summary>
    public static class NpcPixelArt
    {
        public const int FrameWidth = 32, FrameHeight = 64, Directions = 4, FramesPerDirection = 4;
        public const int SheetWidth = FrameWidth * FramesPerDirection, SheetHeight = FrameHeight * Directions;
        public const float PixelsPerUnit = 37f;
        public const int Clear = -1;
        const int OutlineColor = 0x2A1C14, Center = 16, Ground = 63;

        /// <summary>Um quadro FrameWidth x FrameHeight, linhas de cima para baixo; Clear (-1) = transparente.</summary>
        public static int[] Frame(NpcLook look, int direction, int frame)
        {
            if (look == null) throw new ArgumentNullException("look");
            direction = ((direction % Directions) + Directions) % Directions;
            frame = ((frame % FramesPerDirection) + FramesPerDirection) % FramesPerDirection;
            var canvas = new PixelCanvas();
            Paint(canvas, look, direction == 3 ? 1 : direction, frame);
            canvas.Outline();
            if (direction == 3) canvas.Mirror();
            return canvas.Px;
        }

        /// <summary>
        /// Folha RGBA32 SheetWidth x SheetHeight, linhas de baixo para cima (ordem do Texture2D.LoadRawTextureData).
        /// Linha de quadros = direção; coluna = quadro.
        /// </summary>
        public static byte[] Sheet(NpcLook look)
        {
            var bytes = new byte[SheetWidth * SheetHeight * 4];
            for (int d = 0; d < Directions; d++)
                for (int f = 0; f < FramesPerDirection; f++)
                {
                    var px = Frame(look, d, f);
                    for (int y = 0; y < FrameHeight; y++)
                        for (int x = 0; x < FrameWidth; x++)
                        {
                            int c = px[y * FrameWidth + x];
                            if (c == Clear) continue;
                            int row = SheetHeight - 1 - (d * FrameHeight + y);
                            int o = (row * SheetWidth + f * FrameWidth + x) * 4;
                            bytes[o] = (byte)((c >> 16) & 255);
                            bytes[o + 1] = (byte)((c >> 8) & 255);
                            bytes[o + 2] = (byte)(c & 255);
                            bytes[o + 3] = 255;
                        }
                }
            return bytes;
        }

        static int Mix(int c, float k)
        {
            int r = (c >> 16) & 255, g = (c >> 8) & 255, b = c & 255;
            if (k >= 0f)
            {
                r += (int)((255 - r) * k); g += (int)((255 - g) * k); b += (int)((255 - b) * k);
            }
            else
            {
                r = (int)(r * (1f + k)); g = (int)(g * (1f + k)); b = (int)(b * (1f + k));
            }
            return (r << 16) | (g << 8) | b;
        }

        static void Paint(PixelCanvas cv, NpcLook look, int d, int f)
        {
            int by = f % 2 == 0 ? 1 : 0;            // tronco/cabeça descem 1 px nos passos
            int hw = 7 + look.Wide;
            int cx = Center, ground = Ground;
            bool dress = look.Dress;
            int tlen = dress ? 14 : 17;
            int apronEnd = (dress ? 55 : 49) + by;
            bool longHair = look.HairStyle == NpcHairStyle.Long || look.HairStyle == NpcHairStyle.Braid;
            int sk = look.Skirt != -1 ? look.Skirt : look.Bottom;

            if (d == 0 || d == 2)
            {
                int ll = f == 0 ? 0 : (f == 2 ? -2 : -1);
                int lr = f == 0 ? -2 : (f == 2 ? 0 : -1);
                if (!dress)
                {
                    cv.Rect(cx - 5, 43 + by, cx - 1, ground - 5 + ll, look.Bottom);
                    cv.Rect(cx + 1, 43 + by, cx + 5, ground - 5 + lr, look.Bottom);
                    cv.Rect(cx - 6, ground - 5 + ll, cx - 1, ground + ll, look.Boots);
                    cv.Rect(cx + 1, ground - 5 + lr, cx + 6, ground + lr, look.Boots);
                }
                else
                {
                    cv.Rect(cx - 5, ground - 4 + ll, cx - 1, ground + ll, look.Boots);
                    cv.Rect(cx + 1, ground - 4 + lr, cx + 5, ground + lr, look.Boots);
                    for (int y = 37 + by; y < 59 + by; y++)
                    {
                        int half = hw + 1 + (y - 37 - by) * 3 / 22;
                        cv.Rect(cx - half, y, cx + half, y + 1, sk, false);
                    }
                    for (int y = 37 + by; y < 59 + by; y++)
                    {
                        int half = hw + 1 + (y - 37 - by) * 3 / 22;
                        cv.Set(cx + half - 1, y, Mix(sk, -.18f));
                        cv.Set(cx - half, y, Mix(sk, .08f));
                    }
                    int hem = 58 + by;
                    int hemHalf = hw + 1 + (hem - 37 - by) * 3 / 22;
                    cv.Rect(cx - hemHalf, hem - 1, cx + hemHalf, hem + 1, look.Trim, false);
                }
                // braços
                int armTop = 24 + by;
                cv.Rect(cx - hw - 3, armTop, cx - hw, armTop + 13, look.Top);
                cv.Rect(cx + hw, armTop, cx + hw + 3, armTop + 13, look.Top);
                if (look.Rolled)
                {
                    cv.Rect(cx - hw - 3, armTop + 5, cx - hw, armTop + 13, look.Skin);
                    cv.Rect(cx + hw, armTop + 5, cx + hw + 3, armTop + 13, look.Skin);
                }
                cv.Rect(cx - hw - 3, armTop + 13, cx - hw, armTop + 16, look.Skin);
                cv.Rect(cx + hw, armTop + 13, cx + hw + 3, armTop + 16, look.Skin);
                // tronco
                cv.Rect(cx - hw, 23 + by, cx + hw, 23 + by + tlen, look.Top);
                cv.Rect(cx - hw - 1, 23 + by, cx + hw + 1, 25 + by, look.Top, false);
                cv.Rect(cx - hw, 23 + by + tlen - 3, cx + hw, 23 + by + tlen - 1, look.Belt);
                if (d == 0)
                {
                    cv.Rect(cx - 3, 22 + by, cx + 3, 24 + by, look.Trim);
                    cv.Rect(cx - 1, 24 + by, cx + 1, 26 + by, look.Skin);
                    if (dress) cv.Rect(cx - 3, 26 + by, cx + 3, 27 + by, look.Trim);
                }
                if (look.Apron != -1)
                {
                    int top = 31 + by;
                    cv.Rect(cx - hw + 2, top, cx + hw - 2, apronEnd, look.Apron);
                    if (!dress) cv.Rect(cx - hw + 2, top, cx + hw - 2, top + 1, Mix(look.Apron, -.25f), false);
                }
                if (look.Bag && d == 0)
                {
                    cv.Rect(cx + hw - 2, 33 + by, cx + hw + 4, 43 + by, look.BagColor);
                    cv.Rect(cx + hw - 1, 36 + by, cx + hw + 1, 38 + by, 0x58B060);
                    cv.Set(cx + hw + 2, 37 + by, 0xE0C040);
                    for (int y = 24 + by; y < 36 + by; y++)
                        cv.Set(cx - 4 + (y - 24 - by) * 10 / 12, y, Mix(look.BagColor, -.2f));
                }
                if (look.Bag && d == 2) cv.Rect(cx - hw + 1, 33 + by, cx + 5, 43 + by, look.BagColor);
            }
            else
            {
                // perfil voltado para a direita
                int swOff = f == 0 ? 3 : (f == 2 ? -3 : 0);
                int liftBack = (f == 1 || f == 3) ? 1 : 0;
                if (!dress)
                {
                    cv.Rect(cx - 2 - swOff, 43 + by, cx + 2 - swOff, ground - 5 - liftBack, Mix(look.Bottom, -.15f));
                    cv.Rect(cx - 3 - swOff, ground - 5 - liftBack, cx + 3 - swOff + 1, ground - liftBack, Mix(look.Boots, -.15f));
                    cv.Rect(cx - 2 + swOff, 43 + by, cx + 2 + swOff, ground - 5, look.Bottom);
                    cv.Rect(cx - 3 + swOff, ground - 5, cx + 3 + swOff + 1, ground, look.Boots);
                }
                else
                {
                    cv.Rect(cx - 3 - swOff, ground - 4 - liftBack, cx + 2 - swOff, ground - liftBack, Mix(look.Boots, -.15f));
                    cv.Rect(cx - 2 + swOff, ground - 4, cx + 4 + swOff, ground, look.Boots);
                    for (int y = 37 + by; y < 59 + by; y++)
                    {
                        int half = hw - 1 + (y - 37 - by) * 3 / 22;
                        cv.Rect(cx - half, y, cx + half + 1, y + 1, sk, false);
                        cv.Set(cx + half, y, Mix(sk, -.18f));
                        cv.Set(cx - half, y, Mix(sk, .08f));
                    }
                    int hemHalf = hw - 1 + 21 * 3 / 22;
                    cv.Rect(cx - hemHalf, 57 + by, cx + hemHalf + 1, 59 + by, look.Trim, false);
                }
                int swArm = f == 0 ? -2 : (f == 2 ? 2 : 0);
                cv.Rect(cx - 2 - swArm, 25 + by, cx + 1 - swArm, 38 + by, Mix(look.Top, -.2f));
                int tw = hw - 2;
                cv.Rect(cx - tw, 23 + by, cx + tw, 23 + by + tlen, look.Top);
                cv.Rect(cx - tw, 23 + by + tlen - 3, cx + tw, 23 + by + tlen - 1, look.Belt);
                if (look.Apron != -1) cv.Rect(cx + tw - 4, 31 + by, cx + tw + 1, apronEnd, look.Apron);
                if (look.Bag)
                {
                    cv.Rect(cx - 2, 33 + by, cx + 4, 43 + by, look.BagColor);
                    cv.Rect(cx - 1, 36 + by, cx + 1, 38 + by, 0x58B060);
                }
                int armX = cx - 1 + swArm;
                cv.Rect(armX - 1, 25 + by, armX + 2, 38 + by, look.Top);
                if (look.Rolled) cv.Rect(armX - 1, 30 + by, armX + 2, 38 + by, look.Skin);
                cv.Rect(armX - 1, 38 + by, armX + 2, 41 + by, look.Skin);
            }

            // cabeça
            int hy = 14 + by;
            int hx = cx + (d == 1 ? 1 : 0);
            int hair = look.Hair;
            cv.Ellipse(hx, hy, 7, 8, look.Skin);
            if (d == 1)
            {
                cv.Set(hx + 8, hy + 1, Mix(look.Skin, -.05f));
                cv.Set(hx + 8, hy + 2, Mix(look.Skin, -.05f));
            }
            if (look.HairStyle != NpcHairStyle.Bald || d == 2)
            {
                for (int y = hy - 8; y < hy + 9; y++)
                    for (int x = hx - 7; x < hx + 8; x++)
                    {
                        float dx = (x - hx + .5f) / 7.5f, dy = (y - hy + .5f) / 8.5f;
                        if (dx * dx + dy * dy > 1f) continue;
                        bool inCap = y < hy - 2;
                        if (d == 0 && !inCap) inCap = Math.Abs(x - hx + .5f) > 5.2f && y < hy + (longHair ? 6 : 1);
                        if (d == 2) inCap = true;
                        if (d == 1) inCap = y < hy - 2 || (x < hx - 1 && y < hy + (longHair ? 7 : 2));
                        if (!inCap) continue;
                        int col = hair;
                        if (dx > .45f) col = Mix(hair, -.2f);
                        else if (y < hy - 5) col = Mix(hair, .12f);
                        cv.Set(x, y, col);
                    }
            }
            else
            {
                for (int y = hy - 2; y < hy + 3; y++)
                {
                    if (d == 0) { cv.Set(hx - 7, y, hair); cv.Set(hx + 6, y, hair); }
                    if (d == 1) for (int x = hx - 7; x < hx - 2; x++) cv.Set(x, y, hair);
                }
            }
            if (longHair)
            {
                int ln = look.HairStyle == NpcHairStyle.Long ? 14 : 18;
                if (d == 2) cv.Rect(hx - 6, hy + 4, hx + 6, hy + ln, hair);
                else if (d == 1) cv.Rect(hx - 7, hy + 2, hx - 2, hy + ln - 2, hair);
                else
                {
                    cv.Rect(hx - 8, hy, hx - 6, hy + ln - 6, hair);
                    cv.Rect(hx + 6, hy, hx + 8, hy + ln - 6, hair);
                }
            }
            if (look.HairStyle == NpcHairStyle.Braid && d == 0)
            {
                cv.Rect(hx + 6, hy + 6, hx + 9, hy + 18, hair);
                cv.Rect(hx + 6, hy + 16, hx + 9, hy + 18, 0xB04040);
            }
            if (look.HairStyle == NpcHairStyle.Bun) cv.Ellipse(hx - (d == 1 ? 2 : 0), hy - 8, 3, 3, hair);

            // rosto
            if (d == 0)
            {
                cv.Rect(hx - 3, hy + 1, hx - 1, hy + 3, 0x2A1A14, false);
                cv.Rect(hx + 2, hy + 1, hx + 4, hy + 3, 0x2A1A14, false);
                cv.Rect(hx - 1, hy + 5, hx + 1, hy + 6, Mix(look.Skin, -.35f), false);
                cv.Rect(hx - 5, hy + 3, hx - 3, hy + 4, Mix(look.Skin, -.08f), false);
                cv.Rect(hx + 3, hy + 3, hx + 5, hy + 4, Mix(look.Skin, -.08f), false);
            }
            else if (d == 1)
            {
                cv.Rect(hx + 3, hy + 1, hx + 5, hy + 3, 0x2A1A14, false);
                cv.Rect(hx + 3, hy + 5, hx + 5, hy + 6, Mix(look.Skin, -.35f), false);
            }
            if (look.Beard && d != 2)
            {
                if (d == 0)
                {
                    for (int y = hy + 3; y < hy + 9; y++)
                        for (int x = hx - 6; x < hx + 6; x++)
                        {
                            float dx = (x - hx + .5f) / 7.5f, dy = (y - hy + .5f) / 8.5f;
                            if (dx * dx + dy * dy <= 1f && (y > hy + 4 || Math.Abs(x - hx + .5f) > 4f))
                                cv.Set(x, y, dx > .4f ? Mix(hair, -.12f) : hair);
                        }
                    cv.Rect(hx - 1, hy + 5, hx + 1, hy + 6, Mix(look.Skin, -.35f), false);
                }
                else
                {
                    for (int y = hy + 3; y < hy + 9; y++)
                        for (int x = hx - 1; x < hx + 6; x++)
                        {
                            float dx = (x - hx + .5f) / 7.5f, dy = (y - hy + .5f) / 8.5f;
                            if (dx * dx + dy * dy <= 1f && y > hy + 4) cv.Set(x, y, hair);
                        }
                }
            }

            // chapéu / lenço
            if (look.Headwear == NpcHeadwear.Kerchief)
            {
                for (int y = hy - 9; y < hy; y++)
                    for (int x = hx - 8; x < hx + 9; x++)
                    {
                        float dx = (x - hx + .5f) / 8.5f, dy = (y - hy + .5f) / 9f;
                        if (dx * dx + dy * dy > 1f || y >= hy - 3) continue;
                        if (d == 1 && x > hx + 3 && y > hy - 6) continue;
                        cv.Set(x, y, dx > .45f ? Mix(look.HeadColor, -.15f) : look.HeadColor);
                    }
                if (d == 2) cv.Rect(hx - 6, hy - 3, hx + 6, hy + 6, look.HeadColor);
                if (d == 1) cv.Rect(hx - 8, hy - 3, hx - 3, hy + 3, look.HeadColor);
                cv.Rect(d != 1 ? hx - 8 : hx - 9, hy - 3, d != 1 ? hx + 8 : hx + 5, hy - 2, Mix(look.HeadColor, -.22f), false);
            }
            if (look.Headwear == NpcHeadwear.StrawHat)
            {
                cv.Ellipse(hx, hy - 6, 11, 3, look.HeadColor);
                cv.Ellipse(hx, hy - 9, 6, 4, Mix(look.HeadColor, .05f));
                cv.Rect(hx - 6, hy - 8, hx + 6, hy - 6, 0x6A4A30, false);
            }
        }

        sealed class PixelCanvas
        {
            public readonly int[] Px = new int[FrameWidth * FrameHeight];

            public PixelCanvas()
            {
                for (int i = 0; i < Px.Length; i++) Px[i] = Clear;
            }

            public void Set(int x, int y, int c)
            {
                if (x >= 0 && x < FrameWidth && y >= 0 && y < FrameHeight) Px[y * FrameWidth + x] = c;
            }

            public void Rect(int x0, int y0, int x1, int y1, int c, bool shade = true)
            {
                for (int y = y0; y < y1; y++)
                    for (int x = x0; x < x1; x++)
                    {
                        int col = c;
                        if (shade)
                        {
                            if (x == x1 - 1 && x1 - x0 > 2) col = Mix(c, -.18f);
                            else if (x == x0 && x1 - x0 > 2) col = Mix(c, .10f);
                            if (y == y1 - 1 && y1 - y0 > 3) col = Mix(c, -.18f);
                        }
                        Set(x, y, col);
                    }
            }

            public void Ellipse(int cx, int cy, int rx, int ry, int c)
            {
                for (int y = cy - ry; y <= cy + ry; y++)
                    for (int x = cx - rx; x <= cx + rx; x++)
                    {
                        float dx = (x - cx + .5f) / (rx + .5f), dy = (y - cy + .5f) / (ry + .5f);
                        if (dx * dx + dy * dy > 1f) continue;
                        int col = c;
                        if (dx > .45f) col = Mix(c, -.18f);
                        else if (dx < -.5f) col = Mix(c, .08f);
                        Set(x, y, col);
                    }
            }

            /// <summary>Contorno de 1 px nos pixels transparentes vizinhos (4 direções) de pixels pintados.</summary>
            public void Outline()
            {
                var src = (int[])Px.Clone();
                for (int y = 0; y < FrameHeight; y++)
                    for (int x = 0; x < FrameWidth; x++)
                    {
                        if (src[y * FrameWidth + x] != Clear) continue;
                        if (Painted(src, x + 1, y) || Painted(src, x - 1, y) || Painted(src, x, y + 1) || Painted(src, x, y - 1))
                            Px[y * FrameWidth + x] = OutlineColor;
                    }
            }

            static bool Painted(int[] src, int x, int y)
            {
                if (x < 0 || x >= FrameWidth || y < 0 || y >= FrameHeight) return false;
                int v = src[y * FrameWidth + x];
                return v != Clear && v != OutlineColor;
            }

            public void Mirror()
            {
                for (int y = 0; y < FrameHeight; y++)
                    for (int x = 0; x < FrameWidth / 2; x++)
                    {
                        int a = y * FrameWidth + x, b = y * FrameWidth + (FrameWidth - 1 - x);
                        int t = Px[a]; Px[a] = Px[b]; Px[b] = t;
                    }
            }
        }
    }
}