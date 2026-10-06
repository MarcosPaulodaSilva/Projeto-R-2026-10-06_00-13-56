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
        // Oak/pine of the original atlas plus the recoloured trees of town-extra (16-18): trunk-only collision.
        public bool IsTree => (Atlas == 0 && (Sprite == 4 || Sprite == 5)) || (Atlas == 1 && Sprite >= 16 && Sprite <= 18);
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
        // Standing decoration the player walks through (bushes, stones).
        static TownProp Deco(string name, int sprite, float x, float y, float w, bool flip = false)
        { return new TownProp(name, 1, sprite, x, y, w, 0, flip, false, false); }
        // Trees: 0 carvalho · 1 pinheiro · 2 carvalho dourado · 3 carvalho claro · 4 pinheiro azulado.
        static TownProp Tree(string name, int kind, float x, float y, float w, bool flip = false)
        {
            return kind == 0 ? Town(name, 4, x, y, w, .4f, flip) : kind == 1 ? Town(name, 5, x, y, w, .4f, flip)
                : Extra(name, 14 + kind, x, y, w, .4f, flip);
        }

        public static readonly TownProp[] Props =
        {
            // Rua norte: fachadas voltadas para o sul, de frente para quem passa.
            Town("Estalagem", 1, -3, 4.8f, 5.2f, 2.1f),
            Town("Guilda — fachada", 2, 4.3f, 4.8f, 4.8f, 2.1f),
            Town("Casa vermelha norte", 0, -7.4f, 5.8f, 3.2f, 1.5f),
            Extra("Estalagem menor nordeste", 4, 12.3f, 6, 3.2f, 1.4f),
            // Entre as ruas: casas de costas (a porta fica do lado da rua norte) e a ferraria de frente para a rua sul.
            Extra("Casa de costas oeste", 0, -8, .3f, 2.9f, 1.4f),
            Extra("Casa de costas azul", 5, -11.8f, .5f, 2.9f, 1.4f, true),
            Town("Ferraria — fachada", 3, 9, 2.2f, 4.3f, 1.8f),
            Extra("Estalagem de costas leste", 6, 12.4f, .4f, 3.2f, 1.4f),
            // Rua dos fundos (sul): casas de frente para a viela, com o caminho até a porta.
            Extra("Casa verde sudoeste", 3, -8.2f, -7.5f, 3.2f, 1.1f),
            Extra("Casa azul sudoeste", 2, -12, -7.6f, 3, 1.1f, true),
            Extra("Casa azul sudeste", 2, 3.2f, -7.5f, 3, 1.1f),
            Extra("Casa verde sudeste", 3, 11.9f, -7.6f, 3.2f, 1.1f, true),
            // Praça: mercado, descanso e avisos em volta do poço.
            Town("Poço da praça", 6, 0, 0, 1.65f, .85f),
            Town("Banca oeste", 7, -3.3f, -1.2f, 2.5f, .8f),
            Town("Banca leste", 7, 3.3f, -1.2f, 2.5f, .8f, true),
            Town("Banca da praça", 7, 6.4f, -4.7f, 2.8f, 1),
            Extra("Banco oeste", 11, -2.1f, 2.1f, 1.33f, .4f),
            Extra("Banco leste", 11, 2.2f, 2.1f, 1.33f, .4f),
            Extra("Mural de avisos", 12, 2.6f, 3.2f, 1.23f, .4f),
            Extra("Barris da banca oeste", 7, -4.5f, -.7f, .7f, .4f),
            Extra("Caixotes da banca leste", 8, 4.6f, -.8f, 1.2f, .5f),
            Soft("Canteiro oeste do poço", 13, -1.8f, -.5f, 1.5f),
            Soft("Canteiro leste do poço", 13, 1.8f, -.5f, 1.5f),
            // Jardim noroeste (ervas em -10.2, 7.2): cerca ao sul com uma entrada em x≈-11, canteiros e feno.
            Extra("Cerca do jardim oeste", 10, -12.2f, 5.7f, 1.5f, .25f),
            Extra("Cerca do jardim leste", 10, -9.9f, 5.7f, 1.5f, .25f),
            Soft("Canteiro do jardim 1", 13, -11.5f, 7.5f, 1.5f),
            Soft("Canteiro do jardim 2", 13, -11.5f, 8.8f, 1.5f),
            Soft("Canteiro do jardim 3", 13, -9.7f, 9.3f, 1.5f),
            Extra("Feno do jardim", 9, -12.2f, 6.6f, 1.08f, .5f),
            Extra("Barril do jardim", 7, -9.2f, 7.9f, .7f, .4f),
            // Quintais
            Extra("Feno do quintal", 9, -5.9f, 1.2f, 1.08f, .5f),
            Extra("Barril do quintal", 7, -9.7f, -.4f, .7f, .4f),
            Extra("Caixotes do quintal", 8, -5.3f, -.9f, 1.2f, .5f, true),
            Extra("Feno da ferraria", 9, 6.3f, 1.2f, 1.08f, .5f),
            Extra("Carroça da viela", 14, 6.2f, -9.5f, 1.8f, .8f),
            Extra("Feno da viela", 9, -4.6f, -6.9f, 1.08f, .5f),
            Extra("Barris da casa sudeste", 7, 9.9f, -7.2f, .7f, .4f),
            Extra("Cerca das ervas sudeste", 10, 9.6f, -5.3f, 1.5f, .25f),
            Soft("Canteiro da banca", 13, 9.7f, -6.4f, 1.5f),
            // Arbustos e pedras: assentam as casas no chão e quebram as linhas retas.
            Deco("Arbusto da casa vermelha", 20, -9.2f, 5.55f, 1.0f),
            Deco("Arbusto da casa vermelha 2", 19, -5.7f, 5.55f, .95f, true),
            Deco("Arbusto da estalagem nordeste", 20, 10.5f, 5.8f, 1.0f),
            Deco("Arbusto da estalagem nordeste 2", 19, 14, 5.8f, .9f, true),
            Deco("Arbusto da ferraria", 19, 6.8f, 1.9f, .95f),
            Deco("Arbusto da guilda", 20, 7.1f, 4.4f, 1.0f),
            Deco("Arbusto da estalagem", 19, -5.9f, 4.4f, 1.0f, true),
            Deco("Arbusto sudoeste", 20, -10.2f, -7.3f, 1.0f),
            Deco("Arbusto sudoeste 2", 19, -5.9f, -7.2f, .95f, true),
            Deco("Arbusto sudeste", 19, 5.0f, -7.2f, .95f),
            Deco("Arbusto sudeste 2", 20, 13.9f, -7.3f, .95f),
            Deco("Arbusto da banca", 19, 8.2f, -4.4f, .9f),
            Deco("Arbusto do quintal oeste", 20, -13.1f, -.5f, 1.0f),
            Deco("Arbusto do quintal leste", 19, 13.9f, -.8f, .95f, true),
            Deco("Pedras do caminho oeste", 21, -3.4f, -2.2f, .9f),
            Deco("Pedras do caminho leste", 21, 8.9f, 3.9f, .9f, true),
            Deco("Pedras da viela", 21, -3.2f, -9.2f, .9f),
            // Árvores isoladas e bosquetes (as bordas do mapa são desenhadas em TownWorld).
            Tree("Carvalho oeste", 0, -6.3f, 3.2f, 2.8f),
            Tree("Pinheiro oeste", 1, -12.9f, 3.1f, 2.2f),
            Tree("Pinheiro leste", 1, 6.9f, 2.4f, 2.2f),
            Tree("Carvalho leste", 0, 11.8f, -3f, 2.8f),
            Tree("Bosque nordeste 1", 2, 8.4f, 8.4f, 2.9f),
            Tree("Bosque nordeste 2", 1, 10.3f, 9.7f, 2.2f),
            Tree("Bosque nordeste 3", 3, 7.0f, 9.5f, 2.6f),
            Tree("Bosque leste", 4, 13.0f, 3.6f, 2.3f),
            Tree("Bosque sul-oeste 1", 2, -3.6f, -5.4f, 2.8f),
            Tree("Bosque sul-oeste 2", 1, -5.6f, -4.3f, 2.2f),
            Tree("Bosque sul-leste 2", 4, 11.0f, -4.6f, 2.3f),
            Tree("Bosque sul-leste 3", 2, 13.2f, -5.0f, 2.7f),
            Tree("Bosque sul 1", 4, -11.2f, -9.8f, 2.3f),
            Tree("Bosque sul 2", 0, -6.6f, -9.9f, 2.7f),
            Tree("Bosque sul 3", 4, 3.8f, -9.9f, 2.3f),
            Tree("Bosque sul 4", 2, 9.4f, -9.9f, 2.8f),
            Tree("Bosque sul 5", 1, 12.7f, -9.5f, 2.2f),
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
