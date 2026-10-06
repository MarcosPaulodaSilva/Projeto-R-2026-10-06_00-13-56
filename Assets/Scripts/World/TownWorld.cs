using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vadronia
{
    public sealed class TownWorld : IDisposable
    {
        readonly GameObject root;
        readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        readonly VisualLibrary visuals = new VisualLibrary();
        public Transform Root => root.transform;
        public readonly List<FootBlock> Blocks = TownLayout.Blocks();
        public TownWorld(Texture2D scenery)
        {
            root = new GameObject("Grünwald — ruas e praça");
            var texture = Resources.Load<Texture2D>("Vadronia/terrain-v2");
            if(texture == null) { texture = PaintGround(); owned.Add(texture); }
            var ground = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f, texture.width/28f);
            owned.Add(ground);
            var terrain = Add("Chão", ground, Vector2.zero, 1, -20000);
            terrain.transform.localScale = new Vector3(1,22f/ground.bounds.size.y,1);
            for (int i = 0; i < TownLayout.Props.Length; i++)
            {
                var prop = TownLayout.Props[i];
                Rect bounds = TownAtlasLayout.Frames[prop.Sprite];
                Sprite sprite = Sprite.Create(scenery, bounds, new Vector2(.5f, 0), bounds.width / prop.Width, 0, SpriteMeshType.FullRect);
                owned.Add(sprite);
                visuals.Add(root.transform,"Sombra — "+prop.Name,visuals.SoftDisc,new Vector2(prop.X+.3f,prop.Y-.12f),new Vector2(prop.Width*1.15f,prop.Width*.42f),new Color(.07f,.1f,.075f,.7f),-17000);
                Add(prop.Name, sprite, new Vector2(prop.X, prop.Y), 1, -Mathf.RoundToInt(prop.Y * 100)*10);
            }
            // A forest border adds depth without adding collision across the plaza patrol.
            for(int i=0;i<28;i++)
            {
                int type=i%3==0?4:5;Rect bounds=TownAtlasLayout.Frames[type];
                var sprite=Sprite.Create(scenery,bounds,new Vector2(.5f,0),bounds.width/(2.2f+(i%4)*.23f));owned.Add(sprite);
                float x=i<14?-13.8f+(i%2)*.4f:13.8f-(i%2)*.4f;
                float y=-10.7f+(i%14)*1.65f;
                Add("Bosque "+i,sprite,new Vector2(x,y),1,-Mathf.RoundToInt(y*100)*10);
            }
        }
        SpriteRenderer Add(string name, Sprite sprite, Vector2 position, float scale, int order)
        {
            var renderer = new GameObject(name).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(root.transform);
            renderer.transform.position = new Vector3(position.x, position.y, 0);
            renderer.transform.localScale = Vector3.one * scale;
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }
        static Texture2D PaintGround()
        {
            const int width = 896, height = 704;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "Grünwald terrain";
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color32[width * height];
            var random = new System.Random(317);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    float wx = x / 32f - 14, wy = y / 32f - 11;
                    bool plaza = Mathf.Abs(wx) < 4.3f && wy > -3.6f && wy < 4.3f;
                    bool road = Mathf.Abs(wx) < 1.3f || (Mathf.Abs(wy - 2.5f) < .8f && Mathf.Abs(wx) < 10)
                        || (Mathf.Abs(wy + 5) < .65f && wx > -9 && wx < 7);
                    int noise = random.Next(-5, 6);
                    bool edge = Mathf.Abs(wx) > 13.3f || Mathf.Abs(wy) > 10.3f;
                    Color32 color;
                    if (plaza)
                    {
                        bool seam = y % 10 == 0 || (x + (y / 10 % 2) * 8) % 18 == 0;
                        color = seam ? new Color32(112, 110, 86, 255) : Color(155 + noise, 151 + noise, 116 + noise);
                    }
                    else if (road) color = Color(165 + noise, 136 + noise, 86 + noise);
                    else color = Color((edge ? 61 : 82) + noise, (edge ? 98 : 124) + noise, 54 + noise);
                    if (!road && !plaza && !edge && random.Next(160) == 0) color = new Color32(132, 155, 73, 255);
                    pixels[y * width + x] = color;
                }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }
        static Color32 Color(int r, int g, int b) { return new Color32((byte)r, (byte)g, (byte)b, 255); }
        public void Dispose()
        {
            if (root != null) UnityEngine.Object.Destroy(root);
            foreach (var asset in owned) if (asset != null) UnityEngine.Object.Destroy(asset);
            visuals.Dispose();
        }
    }
}
