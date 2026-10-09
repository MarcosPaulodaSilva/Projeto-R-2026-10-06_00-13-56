using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vadronia
{
    // Video locomotion and reference-derived poses, preserving the existing NPC identities.
    public sealed class CharacterView : IDisposable
    {
        readonly GameObject root;
        readonly SpriteRenderer renderer;
        readonly List<Sprite> owned = new List<Sprite>();
        readonly List<Texture2D> ownedTextures = new List<Texture2D>();
        readonly VisualLibrary visuals = new VisualLibrary();
        Sprite[][] frames;
        int[] idleFrames;
        Sprite[][] runningFrames;
        bool[] runMirrors;
        Sprite[] neutralFrames;
        bool referenceNpc;
        float pendingDistance;
        public bool IsRunningAnimation { get; private set; }
        readonly bool fromVideo;
        float phase, baseScale = 1f;
        int direction;
        public readonly WalkCycle Cycle = new WalkCycle();
        public FootPoint Position { get; private set; }
        public Vector2 LeftFootOffset => Vector2.zero;
        public Vector2 RightFootOffset => Vector2.zero;
        public Transform Transform => root.transform;
        public int Facing => direction;
        public int DirectionCount => fromVideo ? 8 : 4;
        public int AnimationFrame { get; private set; }
        public bool UsesVideo => fromVideo;
        public bool UsesReference => referenceNpc;
        public float AnimationPhase => phase;
        public int FrameCount => frames[ClipIndex].Length;
        int ClipIndex => fromVideo && direction == 7 ? 1 : direction;

        public CharacterView(string name, FootPoint position, Texture2D walking, Texture2D original, bool conrad)
        {
            fromVideo = !conrad;
            root = new GameObject(name);
            renderer = root.AddComponent<SpriteRenderer>();
            visuals.Add(root.transform, "Sombra dos pés", visuals.SoftDisc, Vector2.zero,
                new Vector2(.42f, .14f), new Color(.08f, .09f, .07f, .65f), -16000);
            if (fromVideo)
            {
                var json = Resources.Load<TextAsset>("Vadronia/player-video/manifest");
                if (json == null) throw new InvalidOperationException("Quadros do vídeo ausentes.");
                var data = JsonUtility.FromJson<VideoManifest>(json.text);
                frames = new Sprite[data.clips.Length][];
                idleFrames = new int[data.clips.Length];
                for (int d = 0; d < data.clips.Length; d++)
                {
                    var clip = data.clips[d];
                    var texture = Resources.Load<Texture2D>("Vadronia/player-video/" + clip.name);
                    if (texture == null) throw new InvalidOperationException("Atlas do vídeo ausente: " + clip.name);
                    frames[d] = new Sprite[clip.frameCount];
                    idleFrames[d] = clip.idleFrame;
                    for (int i = 0; i < clip.frameCount; i++)
                        frames[d][i] = Slice(texture, new Rect(i % data.columns * data.frameWidth,
                            texture.height - (i / data.columns + 1) * data.frameHeight,
                            data.frameWidth, data.frameHeight), new Vector2(data.pivotX, data.pivotY), 260f);
                }
            }
            else
            {
                if (original == null) throw new InvalidOperationException("Atlas original de Konrad ausente.");
                frames = new Sprite[4][];
                idleFrames = new int[4];
                for (int d = 0; d < 4; d++)
                {
                    frames[d] = new Sprite[4];
                    idleFrames[d] = 1;
                    for (int i = 0; i < 4; i++)
                        frames[d][i] = Slice(original, AtlasLayout.Frames[d * 8 + 4 + i], new Vector2(.5f, 0), 160);
                }
            }
            if(fromVideo) LoadRunning();
            else LoadReference(name);
            Position = position;
            Place(position);
            Animate(0);
        }

        public CharacterView(string name, FootPoint position, NpcLook look)
        {
            if (look == null) throw new ArgumentNullException(nameof(look));
            fromVideo = false;
            root = new GameObject(name);
            renderer = root.AddComponent<SpriteRenderer>();
            visuals.Add(root.transform, "Sombra dos pés", visuals.SoftDisc, Vector2.zero,
                new Vector2(.42f, .14f), new Color(.08f, .09f, .07f, .65f), -16000);

            var texture = new Texture2D(NpcPixelArt.SheetWidth, NpcPixelArt.SheetHeight, TextureFormat.RGBA32, false);
            texture.name = name + " — pixel art procedural";
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.LoadRawTextureData(NpcPixelArt.Sheet(look));
            texture.Apply(false, true);
            ownedTextures.Add(texture);

            frames = new Sprite[NpcPixelArt.Directions][];
            idleFrames = new int[NpcPixelArt.Directions];
            for (int d = 0; d < NpcPixelArt.Directions; d++)
            {
                frames[d] = new Sprite[NpcPixelArt.FramesPerDirection];
                idleFrames[d] = 1;
                for (int i = 0; i < NpcPixelArt.FramesPerDirection; i++)
                {
                    var rect = new Rect(i * NpcPixelArt.FrameWidth,
                        NpcPixelArt.SheetHeight - (d + 1) * NpcPixelArt.FrameHeight,
                        NpcPixelArt.FrameWidth, NpcPixelArt.FrameHeight);
                    frames[d][i] = Slice(texture, rect, new Vector2(.5f, 0), NpcPixelArt.PixelsPerUnit);
                }
            }
            LoadReference(name);
            Position = position;
            Place(position);
            Animate(0);
        }

        Sprite Slice(Texture2D texture, Rect rect, Vector2 pivot, float ppu)
        {
            var sprite = Sprite.Create(texture, rect, pivot, ppu, 0, SpriteMeshType.FullRect);
            owned.Add(sprite);
            return sprite;
        }

        public void Face(Vector2 delta)
        {
            if (delta.sqrMagnitude < .000001f) return;
            if (fromVideo) { direction = EightDirection.Resolve(delta.x, delta.y); return; }
            // Four-direction atlas: hysteresis keeps the sprite from flickering between side and front/back
            // while walking along a diagonal.
            float ax = Mathf.Abs(delta.x), ay = Mathf.Abs(delta.y);
            bool horizontal = direction == 1 || direction == 3;
            bool wantHorizontal = horizontal ? ax * 1.25f >= ay : ax > ay * 1.25f;
            direction = wantHorizontal ? (delta.x > 0 ? 1 : 3) : (delta.y > 0 ? 2 : 0);
        }

        public void Place(FootPoint next)
        {
            var delta = new Vector2(next.X - Position.X, next.Y - Position.Y);
            Cycle.Advance(delta.x, delta.y);
            if (Cycle.Moving)
            {
                Face(delta);
                // Actual collision-resolved distance, never elapsed time or requested velocity.
                pendingDistance += delta.magnitude;
            }
            Position = next;
            root.transform.position = new Vector3(next.X, next.Y, 0);
        }

        public void Animate(float dt, bool dodging = false, bool sprinting = false)
        {
            IsRunningAnimation = fromVideo && Cycle.Moving && sprinting && !dodging && runningFrames != null;
            phase = (phase + pendingDistance / (fromVideo ? (IsRunningAnimation ? 1.65f : 1.25f) : .72f)) % 1f;
            pendingDistance = 0;
            int clip = IsRunningAnimation ? direction : ClipIndex;
            var active = IsRunningAnimation ? runningFrames : frames;
            AnimationFrame = Cycle.Moving ? Mathf.Min(active[clip].Length - 1, (int)(phase * active[clip].Length)) : idleFrames[ClipIndex];
            renderer.sprite = !Cycle.Moving && neutralFrames != null ? neutralFrames[direction] : active[clip][AnimationFrame];
            renderer.flipX = IsRunningAnimation ? runMirrors[direction] : fromVideo && direction == 7 && neutralFrames == null;
            if(fromVideo && Cycle.Moving && !IsRunningAnimation) renderer.flipX = direction == 7;
            renderer.sortingOrder = -Mathf.RoundToInt(Position.Y * 100) * 10 + 4;
            if (!fromVideo) root.transform.localScale = new Vector3(baseScale, baseScale, 1f);
        }

        void LoadRunning()
        {
            var json=Resources.Load<TextAsset>("Vadronia/player-run/manifest");
            if(json==null)return;
            var data=JsonUtility.FromJson<VideoManifest>(json.text);
            runningFrames=new Sprite[8][];runMirrors=new bool[8];
            for(int d=0;d<8;d++)
            {
                var clip=data.clips[d];var tex=Resources.Load<Texture2D>("Vadronia/player-run/"+clip.name);
                runningFrames[d]=new Sprite[clip.frameCount];runMirrors[d]=clip.mirror;
                for(int i=0;i<clip.frameCount;i++) runningFrames[d][i]=Slice(tex,new Rect(i%data.columns*data.frameWidth,
                    tex.height-(i/data.columns+1)*data.frameHeight,data.frameWidth,data.frameHeight),new Vector2(data.pivotX,data.pivotY),260f);
            }
            LoadNeutral();
        }
        void LoadNeutral()
        {
            var data=Resources.Load<TextAsset>("Vadronia/characters/player-idle");
            var tex=Resources.Load<Texture2D>("Vadronia/characters/player-idle");
            if(data==null||tex==null)return;
            var sheet=JsonUtility.FromJson<ReferenceSheet>(data.text);neutralFrames=new Sprite[8];
            for(int i=0;i<8;i++)neutralFrames[i]=ReferenceSlice(tex,sheet.rects[i],1.02f);
        }
        void LoadReference(string name)
        {
            string key;
            switch(name.ToUpperInvariant())
            {
                case "KONRAD":case "CONRAD":key="conrad";break;
                case "HELGA":key="eliza";break;
                case "BRUNO":key="henning";break;
                case "MAREN":key="marta";break;
                case "LÚCIA":key="nicasia";break;
                case "TOMÁS":key="bran";break;
                default:key=name.StartsWith("Morador —")?"lukas":"eliza";break;
            }
            var data=Resources.Load<TextAsset>("Vadronia/characters/"+key);
            var tex=Resources.Load<Texture2D>("Vadronia/characters/"+key);
            if(data==null||tex==null)return;
            var sheet=JsonUtility.FromJson<ReferenceSheet>(data.text);
            frames=new Sprite[4][];idleFrames=new int[4];neutralFrames=new Sprite[4];referenceNpc=true;
            for(int d=0;d<4;d++)
            {
                neutralFrames[d]=ReferenceSlice(tex,sheet.rects[d*4],1.02f);
                frames[d]=new Sprite[4];int[] sequence={1,2,3,2};
                for(int i=0;i<4;i++)frames[d][i]=ReferenceSlice(tex,sheet.rects[d*4+sequence[i]],1.02f);
            }
        }
        Sprite ReferenceSlice(Texture2D tex,ReferenceRect rect,float height)
        {return Slice(tex,new Rect(rect.x,tex.height-rect.y-rect.h,rect.w,rect.h),new Vector2(.5f,0),rect.h/height);}
        [Serializable] sealed class ReferenceSheet { public ReferenceRect[] rects; }
        [Serializable] sealed class ReferenceRect { public int x,y,w,h; }

        public void Tint(Color color, float scale)
        {
            renderer.color = color;
            baseScale = scale;
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(root);
            foreach (var sprite in owned) UnityEngine.Object.Destroy(sprite);
            foreach (var texture in ownedTextures) UnityEngine.Object.Destroy(texture);
            visuals.Dispose();
        }

        [Serializable] sealed class VideoManifest
        {
            public int frameWidth, frameHeight, columns;
            public float pivotX, pivotY, pixelsPerUnit;
            public VideoClip[] clips;
        }
        [Serializable] sealed class VideoClip { public string name; public int frameCount, idleFrame; public bool mirror; }
    }
}
