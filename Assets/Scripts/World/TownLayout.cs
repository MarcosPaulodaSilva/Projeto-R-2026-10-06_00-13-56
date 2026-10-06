using System.Collections.Generic;

namespace Vadronia
{
    public struct TownProp
    {
        public string Name;
        public int Sprite;
        public float X, Y, Width, Depth;
        public TownProp(string name, int sprite, float x, float y, float width, float depth)
        { Name = name; Sprite = sprite; X = x; Y = y; Width = width; Depth = depth; }
        // Collision follows the footprint, never the roof or tree canopy.
        public FootBlock Footprint => new FootBlock(X - Width * .38f, Y, Width * .76f, Depth);
    }
    public static class TownLayout
    {
        public static readonly TownProp[] Props =
        {
            new TownProp("Casa oeste", 0, -8, 3.4f, 4.2f, 1.8f),
            new TownProp("Estalagem", 1, -3, 4.8f, 5.2f, 2.1f),
            new TownProp("Guilda — fachada", 2, 4.3f, 4.8f, 4.8f, 2.1f),
            new TownProp("Ferraria — fachada", 3, 9, 2.2f, 4.3f, 1.8f),
            new TownProp("Casa sul", 0, -8, -6.1f, 4, 1.7f),
            new TownProp("Banca da praça", 7, 5.8f, -4.7f, 2.8f, 1),
            new TownProp("Poço da praça", 6, 0, 0, 1.65f, .85f),
            new TownProp("Carvalho oeste", 4, -11.6f, .3f, 2.8f, .4f),
            new TownProp("Carvalho leste", 4, 11.8f, -5.8f, 2.8f, .4f),
            new TownProp("Pinheiro oeste", 5, -11.7f, 7.6f, 2.2f, .4f),
            new TownProp("Pinheiro leste", 5, 11.4f, 7.4f, 2.2f, .4f),
            new TownProp("Carvalho sul", 4, -3.8f, -8.9f, 2.6f, .4f),
            new TownProp("Pinheiro sul", 5, 8.5f, -8.9f, 2.3f, .4f),
        };
        public static readonly FootPoint[] Patrol =
        {
            new FootPoint(-3, -2.7f), new FootPoint(3, -2.7f),
            new FootPoint(3, 3), new FootPoint(-3, 3)
        };
        public static List<FootBlock> Blocks()
        {
            var result = new List<FootBlock>();
            foreach (var prop in Props)
            {
                if (prop.Sprite == 4 || prop.Sprite == 5)
                    result.Add(new FootBlock(prop.X - .22f, prop.Y, .44f, .35f));
                else result.Add(prop.Footprint);
            }
            return result;
        }
    }
}
