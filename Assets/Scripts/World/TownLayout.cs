using System.Collections.Generic;

namespace Vadronia
{
    // Pure data, no Unity types: the .NET regression project compiles this file too.
    public struct TownProp
    {
        public string Name;
        public int Atlas;          // 0 = town.png (originals), 1 = town-extra.png
        public int Sprite;
        public float X, Y, Width, Depth;
        public bool Flip, Solid, Flat;
        public float Red, Green, Blue;
        public TownProp(string name, int atlas, int sprite, float x, float y, float width, float depth,
            bool flip = false, bool solid = true, bool flat = false)
        {
            Name = name; Atlas = atlas; Sprite = sprite; X = x; Y = y; Width = width; Depth = depth;
            Flip = flip; Solid = solid; Flat = flat; Red = Green = Blue = 1;
        }
        public TownProp Tinted(float r, float g, float b) { var copy = this; copy.Red = r; copy.Green = g; copy.Blue = b; return copy; }
        public bool IsTree => Atlas == 0 && (Sprite == 4 || Sprite == 5);
        // Collision follows the footprint, never the roof or tree canopy.
        public FootBlock Footprint => IsTree ? new FootBlock(X - .22f, Y, .44f, .35f)
            : Atlas == 0 ? new FootBlock(X - Width * .38f, Y, Width * .76f, Depth)
            : new FootBlock(X - Width * .42f, Y, Width * .84f, Depth);
    }
    public static class TownLayout
    {
        // Atlas 0: 0 casa · 1 estalagem · 2 guilda · 3 ferraria · 4 carvalho · 5 pinheiro · 6 poço · 7 banca
        static TownProp Town(string name, int sprite, float x, float y, float w, float d, bool flip = false)
        { return new TownProp(name, 0, sprite, x, y, w, d, flip); }
        // Atlas 1: 0 casa de costas · 1 estalagem de costas · 2 casa azul · 3 casa verde · 4 estalagem vermelha
        // 5 casa de costas azul · 6 estalagem de costas vermelha · 7 barril · 8 caixotes · 9 feno · 10 cerca
        // 11 banco · 12 mural · 13 canteiro · 14 carroça · 15 caminho de terra
        static TownProp Extra(string name, int sprite, float x, float y, float w, float d, bool flip = false)
        { return new TownProp(name, 1, sprite, x, y, w, d, flip); }
        static TownProp Soft(string name, int sprite, float x, float y, float w, bool flip = false)
        { return new TownProp(name, 1, sprite, x, y, w, 0, flip, false, true); }

        public static readonly TownProp[] Props =
        {
            // Rua norte: fachadas voltadas para o sul, de frente para quem passa.
            Town("Estalagem", 1, -3, 4.8f, 5.2f, 2.1f),
            Town("Guilda — fachada", 2, 4.3f, 4.8f, 4.8f, 2.1f),
            Town("Casa vermelha norte", 0, -7.4f, 6, 3.2f, 1.5f),
            Extra("Estalagem menor nordeste", 4, 12.3f, 6.1f, 3.2f, 1.4f),
            // Entre as ruas: casas voltadas para a rua norte, vistas de costas.
            Extra("Casa de costas oeste", 0, -8, .3f, 2.9f, 1.4f),
            Extra("Casa de costas azul", 5, -11.8f, .5f, 2.9f, 1.4f, true),
            Town("Ferraria — fachada", 3, 9, 2.2f, 4.3f, 1.8f),
            Extra("Estalagem de costas leste", 6, 12.4f, .4f, 3.2f, 1.4f),
            // Ao sul da rua sul: também de costas para quem vem da praça.
            Extra("Casa de costas sul oeste", 0, -8.2f, -6.6f, 3.2f, 1.5f, true),
            Extra("Estalagem de costas sudoeste", 1, -12, -6.3f, 3.2f, 1.4f),
            Extra("Casa de costas sudeste", 5, 11.9f, -6.2f, 3.2f, 1.5f),
            // Praça: mercado, descanso e avisos em volta do poço.
            Town("Poço da praça", 6, 0, 0, 1.65f, .85f),
            Town("Banca oeste", 7, -3.3f, -1.2f, 2.5f, .8f),
            Town("Banca leste", 7, 3.3f, -1.2f, 2.5f, .8f, true),
            Town("Banca da praça", 7, 5.8f, -4.7f, 2.8f, 1),
            Extra("Banco oeste", 11, -2.1f, 2.1f, 1.33f, .4f),
            Extra("Banco leste", 11, 2.2f, 2.1f, 1.33f, .4f),
            Extra("Mural de avisos", 12, 2.6f, 3.2f, 1.23f, .4f),
            Extra("Carroça de feno", 14, 5.4f, -9.3f, 1.8f, .8f),
            Extra("Barris da banca oeste", 7, -4.5f, -.7f, .7f, .4f),
            Extra("Caixotes da banca leste", 8, 4.6f, -.8f, 1.2f, .5f),
            Soft("Canteiro oeste do poço", 13, -1.8f, -.5f, 1.5f),
            Soft("Canteiro leste do poço", 13, 1.8f, -.5f, 1.5f),
            // Jardim noroeste (ervas em -10.2, 7.2): cerca ao sul com uma entrada, canteiros e feno.
            Extra("Cerca do jardim oeste", 10, -12.2f, 5.7f, 1.5f, .25f),
            Extra("Cerca do jardim leste", 10, -9.9f, 5.8f, 1.5f, .25f),
            Soft("Canteiro do jardim 1", 13, -11.5f, 7.5f, 1.5f),
            Soft("Canteiro do jardim 2", 13, -11.5f, 8.8f, 1.5f),
            Soft("Canteiro do jardim 3", 13, -9.7f, 9.3f, 1.5f),
            Extra("Feno do jardim", 9, -12.2f, 6.6f, 1.08f, .5f),
            Extra("Barril do jardim", 7, -9.2f, 7.9f, .7f, .4f),
            Extra("Feno do quintal", 9, -5.9f, 1.2f, 1.08f, .5f),
            Extra("Barril do quintal", 7, -9.7f, -.4f, .7f, .4f),
            Extra("Caixotes do quintal", 8, -5.3f, -.9f, 1.2f, .5f, true),
            Extra("Feno do sudoeste", 9, -4.4f, -8.2f, 1.08f, .5f),
            Extra("Carroça do sudoeste", 14, -2.9f, -9.2f, 1.8f, .8f, true),
            Extra("Barris da casa sudeste", 7, 9.5f, -7.5f, .7f, .4f),
            Extra("Cerca da banca", 10, 7.9f, -3.5f, 1.5f, .25f),
            Soft("Canteiro da banca", 13, 9.7f, -4.3f, 1.5f),
            // Árvores isoladas
            Town("Carvalho oeste", 4, -6.3f, 3.2f, 2.8f, .4f),
            Town("Carvalho leste", 4, 11.8f, -3f, 2.8f, .4f),
            Town("Pinheiro oeste", 5, -11.7f, 3.4f, 2.2f, .4f),
            Town("Pinheiro leste", 5, 6.9f, 2.4f, 2.2f, .4f),
            Town("Carvalho sul", 4, -3.8f, -8.9f, 2.6f, .4f),
            Town("Pinheiro sul", 5, 8.5f, -8.9f, 2.3f, .4f),
        };
        // Ronda de Conrad: praça, bancas, rua sul e quintal oeste. A espera em cada ponto vem de PatrolPause.
        public static readonly FootPoint[] Patrol =
        {
            new FootPoint(-1.5f, -3), new FootPoint(-1.5f, 1.3f), new FootPoint(-.2f, 3),
            new FootPoint(1.4f, 1.3f), new FootPoint(1.5f, -3), new FootPoint(6.3f, -2.8f),
            new FootPoint(1.5f, -2.3f), new FootPoint(-6.5f, -2.6f), new FootPoint(-9.5f, -1.6f),
            new FootPoint(-6.5f, -1.4f)
        };
        public static readonly float[] PatrolPause = { 2, 1.2f, 2.4f, 1.2f, 1.6f, 3, 1, 2, 3, .8f };
        public static List<FootBlock> Blocks()
        {
            var result = new List<FootBlock>();
            foreach (var prop in Props)
                if (prop.Solid) result.Add(prop.Footprint);
            return result;
        }
    }
}
