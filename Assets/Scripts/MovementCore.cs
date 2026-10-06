using System;
using System.Collections.Generic;

namespace Vadronia
{
    // Engine-independent movement and animation rules, shared by player, NPC and regression tests.
    public struct FootPoint
    {
        public float X, Y;
        public FootPoint(float x, float y) { X = x; Y = y; }
    }
    public struct FootBlock
    {
        public float Left, Bottom, Right, Top;
        public FootBlock(float x, float y, float width, float height)
        { Left = x; Bottom = y; Right = x + width; Top = y + height; }
        public bool Contains(float x, float y, float radius)
        {
            float nearestX = Math.Max(Left, Math.Min(Right, x));
            float nearestY = Math.Max(Bottom, Math.Min(Top, y));
            float dx = x - nearestX, dy = y - nearestY;
            return dx * dx + dy * dy < radius * radius;
        }
    }
    public static class MovementCore
    {
        public const float Radius = .22f;
        public const float MinX = -13.5f, MaxX = 13.5f, MinY = -10.5f, MaxY = 10.5f;
        public static FootPoint Move(FootPoint point, float dx, float dy, IList<FootBlock> blocks)
        {
            if (float.IsNaN(dx) || float.IsNaN(dy) || float.IsInfinity(dx) || float.IsInfinity(dy))
                return point;
            // Substeps prevent tunnelling even during a frame hitch. Resolve axes independently to slide.
            int steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Abs(dx), Math.Abs(dy)) / .08f));
            for (int i = 0; i < steps; i++)
            {
                float x = Clamp(point.X + dx / steps, MinX + Radius, MaxX - Radius);
                if (Clear(x, point.Y, blocks)) point.X = x;
                float y = Clamp(point.Y + dy / steps, MinY + Radius, MaxY - Radius);
                if (Clear(point.X, y, blocks)) point.Y = y;
            }
            return point;
        }
        public static bool Clear(float x, float y, IList<FootBlock> blocks)
        {
            foreach (var block in blocks) if (block.Contains(x, y, Radius)) return false;
            return true;
        }
        public static float Clamp(float n, float min, float max) { return Math.Max(min, Math.Min(max, n)); }
    }
    public static class EightDirection
    {
        // Clockwise from south: S, SE, E, NE, N, NW, W, SW.
        public static int Resolve(float x, float y)
        {
            return ((int)Math.Round(Math.Atan2(x, -y) * 4 / Math.PI) + 8) % 8;
        }
    }
    public sealed class WalkCycle
    {
        // Four poses: left contact, passing, right contact, passing. One cycle = two footsteps.
        public const float Stride = 1.8f;
        public int Facing { get; private set; }
        public int Frame { get; private set; } = 1;
        public bool Moving { get; private set; }
        public float Phase { get; private set; }
        public void Advance(float dx, float dy)
        {
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            Moving = distance > .00001f;
            if (!Moving) { Frame = 1; return; }
            Facing = Math.Abs(dx) > Math.Abs(dy) ? (dx > 0 ? 1 : 3) : (dy > 0 ? 2 : 0);
            Phase = (Phase + distance / Stride) % 1f;
            Frame = Math.Min(3, (int)(Phase * 4));
        }
    }
}
