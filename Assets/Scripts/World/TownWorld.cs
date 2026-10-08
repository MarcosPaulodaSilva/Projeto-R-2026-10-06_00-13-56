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
        readonly VillageSquare square;
        public Transform Root => root.transform;
        public readonly List<FootBlock> Blocks = TownLayout.Blocks();
        public TownWorld(Texture2D scenery)
        {
            root = new GameObject("Grünwald — ruas e praça");
            // terrain-v3 = terrain-v2 + paths, back lane and contact shade (Tools/build_terrain.py).
            var texture = Resources.Load<Texture2D>("Vadronia/terrain-blue");
            bool blueGround = texture != null;
            if(texture == null) texture = Resources.Load<Texture2D>("Vadronia/terrain-v3");
            if(texture == null) texture = Resources.Load<Texture2D>("Vadronia/terrain-v2");
            if(texture == null) { texture = PaintGround(); owned.Add(texture); }
            var ground = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f, texture.width/28f);
            owned.Add(ground);
            var terrain = Add("Chão", ground, Vector2.zero, 1, -20000);
            terrain.transform.localScale = new Vector3(1,22f/ground.bounds.size.y,1);
            // Calçamento coeso, independente dos recortes de terra do terreno anterior.
            if(!blueGround) square = new VillageSquare(root.transform);
            // Atlas 1 (casas de costas, cercas, barris...) is optional: without it the town still loads.
            var extra = Resources.Load<Texture2D>("Vadronia/town-extra");
            var blue = Resources.Load<Texture2D>("Vadronia/town-blue");
            var blueProps = Resources.Load<Texture2D>("Vadronia/town-blue-props");
            if(extra == null) Debug.LogWarning("Resources/Vadronia/town-extra.png não encontrado; adereços extras omitidos.");
            for (int i = 0; i < TownLayout.Props.Length; i++)
            {
                var prop = TownLayout.Props[i];
                var source = prop.Atlas == 0 ? scenery : extra;
                int replacement = BlueTownAtlas.Replacement(prop);
                int propReplacement = BlueTownAtlas.PropReplacement(prop);
                if(blue!=null && replacement>=0)source=blue;
                if(source == null) continue;
                Rect bounds = blue!=null && replacement>=0 ? BlueTownAtlas.Frames[replacement]
                    : (prop.Atlas == 0 ? TownAtlasLayout.Frames : TownAtlasLayout.Extra)[prop.Sprite];
                if(blueProps!=null && propReplacement>=0){source=blueProps;bounds=BlueTownAtlas.Props[propReplacement];}
                // Flat decals (flower beds, dirt) are centred on the ground; everything else stands on its feet.
                Sprite sprite = Sprite.Create(source, bounds, prop.Flat ? Vector2.one * .5f : new Vector2(.5f, 0), bounds.width / prop.Width, 0, SpriteMeshType.FullRect);
                owned.Add(sprite);
                // Sombras só em volumes grandes. Arbustos e barris não projetam discos desproporcionais.
                bool structure = (prop.Atlas == 0 && prop.Sprite <= 3) || (prop.Atlas == 1 && prop.Sprite <= 6);
                if (!prop.Flat && (structure || prop.IsTree))
                    visuals.Add(root.transform, "Sombra — " + prop.Name, visuals.SoftDisc,
                        new Vector2(prop.X, prop.Y - .09f),
                        new Vector2(prop.Width * (structure ? .92f : .64f), structure ? .32f : .24f),
                        new Color(.08f, .1f, .07f, .4f), -18600);
                var renderer = Add(prop.Name, sprite, new Vector2(prop.X, prop.Y), 1, prop.Flat ? -18000 : -Mathf.RoundToInt(prop.Y * 100)*10);
                renderer.flipX = prop.Flip;
                renderer.color = new Color(prop.Red, prop.Green, prop.Blue, 1f);
            }
            // A forest border adds depth without adding collision across the plaza patrol.
            // Kinds: 0 carvalho · 1 pinheiro · 2 carvalho dourado · 3 carvalho claro · 4 pinheiro azulado.
            int[] mix = { 1, 0, 4, 1, 2, 1, 3, 0, 4, 1 };
            for(int i=0;i<32;i++)
            {
                int kind=mix[(i*7+i/16)%mix.Length];
                bool ready=kind<2||extra!=null;
                if(!ready) kind=kind==4?1:0;
                var source=kind<2?scenery:extra;
                Rect bounds=kind<2?TownAtlasLayout.Frames[4+kind]:TownAtlasLayout.Extra[14+kind];
                var sprite=Sprite.Create(source,bounds,new Vector2(.5f,0),bounds.width/(2.2f+(i%4)*.25f));owned.Add(sprite);
                float x=i<16?-13.8f+(i%2)*.45f:13.8f-(i%2)*.45f;
                float y=-10.7f+(i%16)*1.45f;
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
            square?.Dispose();
            if (root != null) UnityEngine.Object.Destroy(root);
            foreach (var asset in owned) if (asset != null) UnityEngine.Object.Destroy(asset);
            visuals.Dispose();
        }
    }
}
