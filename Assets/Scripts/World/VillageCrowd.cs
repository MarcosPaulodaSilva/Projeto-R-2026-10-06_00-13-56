using System;
using UnityEngine;
using System.Collections.Generic;

namespace Vadronia
{
    /// <summary>
    /// Pequena circulação de moradores na feira, usando a mesma pixel art dos NPCs.
    /// Visitantes de ambientação; não substituem personagens ou diálogos das missões.
    /// </summary>
    public sealed class VillageCrowd : IDisposable
    {
        sealed class Visitor
        {
            public CharacterView View;
            public FootPoint[] Route;
            public int Next = 1;
            public float Wait;
            public float Speed;
        }

        readonly List<FootBlock> blocks;
        readonly Visitor[] visitors;

        public VillageCrowd(List<FootBlock> blocks)
        {
            this.blocks = blocks;
            visitors = new[]
            {
                Create("Moradora — feira oeste", new NpcLook
                {
                    Skin = 0xDFAC85, Hair = 0x5D3B26, Top = 0x7D5B49,
                    Trim = 0xD8C9A5, Bottom = 0x6A503B, Boots = 0x453325,
                    Belt = 0x533827, Dress = true, HairStyle = NpcHairStyle.Bun
                }, new[] {
                    new FootPoint(-5.1f, -3.15f), new FootPoint(-3.9f, -3.1f),
                    new FootPoint(-2.25f, -3.2f), new FootPoint(-3.9f, -3.1f)
                }, .44f),
                Create("Morador — feira leste", new NpcLook
                {
                    Skin = 0xC99268, Hair = 0x433020, Top = 0x536D61,
                    Trim = 0xC1B89D, Bottom = 0x54483B, Boots = 0x443123,
                    Belt = 0x3B2C21, HairStyle = NpcHairStyle.Short
                }, new[] {
                    new FootPoint(5.25f, -3.25f), new FootPoint(3.7f, -3.25f),
                    new FootPoint(2.25f, -3.25f), new FootPoint(3.7f, -3.25f)
                }, .5f)
            };
        }

        static Visitor Create(string name, NpcLook look, FootPoint[] route, float speed)
        {
            return new Visitor { View = new CharacterView(name, route[0], look), Route = route, Speed = speed };
        }

        public void Tick(float dt, bool paused)
        {
            foreach (var visitor in visitors)
            {
                var position = visitor.View.Position;
                if (paused || visitor.Wait > 0)
                {
                    if (!paused) visitor.Wait = Mathf.Max(0, visitor.Wait - dt);
                    visitor.View.Place(position);
                    visitor.View.Animate(paused ? 0 : dt);
                    continue;
                }

                var target = visitor.Route[visitor.Next];
                float dx = target.X - position.X, dy = target.Y - position.Y;
                var next = MovementCore.Move(position,
                    Mathf.Clamp(dx, -visitor.Speed * dt, visitor.Speed * dt),
                    Mathf.Clamp(dy, -visitor.Speed * dt, visitor.Speed * dt), blocks);
                visitor.View.Place(next);
                visitor.View.Animate(dt);
                float remX = target.X - next.X, remY = target.Y - next.Y;
                if (remX * remX + remY * remY < .0016f)
                {
                    visitor.Next = (visitor.Next + 1) % visitor.Route.Length;
                    visitor.Wait = 1.4f + visitor.Next * .55f;
                }
            }
        }

        public void Dispose()
        {
            foreach (var visitor in visitors) visitor.View.Dispose();
        }
    }
}
