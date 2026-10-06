using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vadronia
{
    // Whole frames: the player comes from Marcos's video, Konrad from the original atlas.
    public sealed class CharacterView : IDisposable
    {
        readonly GameObject root;
        readonly SpriteRenderer renderer;
        readonly List<Sprite> owned = new List<Sprite>();
        readonly VisualLibrary visuals = new VisualLibrary();
        readonly Sprite[][] frames;
        readonly int[] idleFrames;
        readonly bool fromVideo;
        float phase;
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
        public float AnimationPhase => phase;
        public int FrameCount => frames[ClipIndex].Length;
        int ClipIndex => fromVideo && direction == 7 ? 1 : direction;

        public CharacterView(string name, FootPoint position, Texture2D walking, Texture2D original, bool conrad)
        {
            fromVideo = !conrad;
            root = new GameObject(name);
            renderer = root.AddComponent<SpriteRenderer>();
            visuals.Add(root.transform, "Sombra dos pés", visuals.SoftDisc, Vector2.zero,
                new Vector2(.72f, .23f), new Color(.08f, .09f, .07f, .65f), -16000);
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
                            data.frameWidth, data.frameHeight), new Vector2(data.pivotX, data.pivotY), data.pixelsPerUnit);
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
            direction = fromVideo ? EightDirection.Resolve(delta.x, delta.y)
                : Mathf.Abs(delta.x) > Mathf.Abs(delta.y) ? (delta.x > 0 ? 1 : 3) : (delta.y > 0 ? 2 : 0);
        }

        public void Place(FootPoint next)
        {
            var delta = new Vector2(next.X - Position.X, next.Y - Position.Y);
            Cycle.Advance(delta.x, delta.y);
            if (Cycle.Moving)
            {
                Face(delta);
                // Actual collision-resolved distance, never elapsed time or requested velocity.
                phase = (phase + delta.magnitude / (fromVideo ? 1.25f : WalkCycle.Stride)) % 1f;
            }
            Position = next;
            root.transform.position = new Vector3(next.X, next.Y, 0);
        }

        public void Animate(float dt, bool dodging = false, bool sprinting = false)
        {
            int clip = ClipIndex;
            AnimationFrame = Cycle.Moving ? Mathf.Min(frames[clip].Length - 1, (int)(phase * frames[clip].Length)) : idleFrames[clip];
            renderer.sprite = frames[clip][AnimationFrame];
            renderer.flipX = fromVideo && direction == 7;
            renderer.sortingOrder = -Mathf.RoundToInt(Position.Y * 100) * 10 + 4;
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(root);
            foreach (var sprite in owned) UnityEngine.Object.Destroy(sprite);
            visuals.Dispose();
        }

        [Serializable] sealed class VideoManifest
        {
            public int frameWidth, frameHeight, columns;
            public float pivotX, pivotY, pixelsPerUnit;
            public VideoClip[] clips;
        }
        [Serializable] sealed class VideoClip { public string name; public int frameCount, idleFrame; }
    }
}
