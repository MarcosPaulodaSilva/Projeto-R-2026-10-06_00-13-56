using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vadronia
{
    public sealed class VisualLibrary : IDisposable
    {
        readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        public readonly Sprite Disc, SoftDisc, Square;
        public VisualLibrary()
        {
            Disc = Circle(false); SoftDisc = Circle(true);
            var t = new Texture2D(1, 1); t.SetPixel(0, 0, Color.white); t.Apply();
            Square = Sprite.Create(t, new Rect(0, 0, 1, 1), Vector2.one * .5f, 1);
            owned.Add(t); owned.Add(Square);
        }
        Sprite Circle(bool soft)
        {
            var t = new Texture2D(64,64,TextureFormat.RGBA32,false); t.filterMode = FilterMode.Bilinear;
            var pixels = new Color[4096];
            for (int y=0;y<64;y++) for(int x=0;x<64;x++)
            {
                float r = new Vector2((x-31.5f)/31.5f,(y-31.5f)/31.5f).magnitude;
                float a = soft ? Mathf.Pow(Mathf.Clamp01(1-r),2) : Mathf.Clamp01((1-r)*16);
                pixels[y*64+x] = new Color(1,1,1,a);
            }
            t.SetPixels(pixels); t.Apply(false,true);
            var s = Sprite.Create(t,new Rect(0,0,64,64),Vector2.one*.5f,64);
            owned.Add(t); owned.Add(s); return s;
        }
        public SpriteRenderer Add(Transform parent, string name, Sprite sprite, Vector2 point, Vector2 size, Color color, int order)
        {
            var r = new GameObject(name).AddComponent<SpriteRenderer>();
            r.transform.SetParent(parent,false); r.transform.localPosition = point;
            r.transform.localScale = new Vector3(size.x,size.y,1);
            r.sprite = sprite; r.color = color; r.sortingOrder = order; return r;
        }
        public void Dispose() { foreach(var item in owned) UnityEngine.Object.Destroy(item); }
    }
}
