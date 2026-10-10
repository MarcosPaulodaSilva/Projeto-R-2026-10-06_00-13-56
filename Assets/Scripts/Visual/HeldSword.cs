using System;
using UnityEngine;
namespace Vadronia
{
    // Presentation only. Anatomical left hand, never the left side of the screen.
    public sealed class HeldSword : IDisposable
    {
        readonly CharacterView actor;
        readonly EquipmentArt art;
        readonly SpriteRenderer body;
        public SpriteRenderer Renderer { get; }
        public string ItemId { get; private set; }
        public Vector2 HandPosition { get; private set; }
        // Idle points measured in the eight reference rectangles, bottom-up, normalized.
        static readonly Vector2[] Idle = {
            new Vector2(.90f,.36f),new Vector2(.90f,.37f),new Vector2(.59f,.36f),new Vector2(.10f,.37f),
            new Vector2(.10f,.36f),new Vector2(.13f,.34f),new Vector2(.51f,.34f),new Vector2(.86f,.36f)
        };
        static readonly float[] Angles={-140,-125,-110,115,140,125,110,-115};
        // Tracked hand samples: frame index, X, Y in each 200x300 video cell (top-left).
        // Mirrored clips have their own anatomical-left track, not a mirrored left-hand track.
        static readonly Vector3[][] Walk={
            Track(0,137,190, 6,146,190, 12,133,194, 18,146,195, 24,138,193),
            Track(0,137,185, 7,151,186, 14,132,186, 20,121,187, 27,130,185),
            Track(0,89,185, 7,134,186, 13,137,184, 20,87,188, 26,88,185),
            Track(0,60,190, 7,62,190, 14,78,191, 20,87,190, 27,58,190),
            Track(0,58,200, 8,55,195, 16,57,195, 24,55,186, 31,59,186),
            Track(0,95,200, 7,70,190, 14,49,188, 21,80,194, 27,91,199),
            Track(0,129,200, 7,85,198, 14,72,190, 21,119,190, 27,130,200),
            Track(0,132,190, 7,146,192, 14,120,191, 20,101,190, 27,119,192)
        };
        static readonly Vector3[][] Run={
            Track(0,156,151, 4,158,162, 8,121,146, 10,107,133, 13,107,151, 16,139,175, 19,156,149, 22,154,157),
            Track(0,130,175, 3,151,157, 6,150,130, 9,150,152, 12,123,174, 15,111,171, 18,121,179, 21,153,169),
            Track(0,84,161, 3,76,158, 6,87,146, 9,167,159, 12,175,128, 15,174,146, 18,153,166, 21,84,161),
            Track(0,47,162, 3,69,149, 6,130,132, 9,132,133, 12,76,150, 15,49,165, 16,45,164),
            Track(0,62,147, 4,63,151, 7,52,166, 10,45,165, 13,44,173, 16,46,182, 19,62,151, 22,62,151),
            Track(0,42,129, 3,48,166, 6,79,165, 9,84,174, 12,60,174, 15,41,136, 16,44,129),
            Track(0,32,138, 3,28,128, 6,43,157, 9,86,174, 12,123,169, 15,120,178, 18,60,163, 21,32,130),
            Track(0,107,168, 3,150,170, 6,162,155, 9,161,173, 12,92,170, 15,79,137, 18,92,156, 21,131,177)
        };
        static Vector3[] Track(params int[] values)
        {
            var result=new Vector3[values.Length/3];
            for(int i=0;i<result.Length;i++)result[i]=new Vector3(values[i*3],values[i*3+1],values[i*3+2]);
            return result;
        }
        static Vector2 Sample(Vector3[] track,int frame)
        {
            for(int i=1;i<track.Length;i++)if(frame<=track[i].x){
                var a=track[i-1];var b=track[i];float t=Mathf.InverseLerp(a.x,b.x,frame);
                return Vector2.Lerp(new Vector2(a.y,a.z),new Vector2(b.y,b.z),t);
            }
            var last=track[track.Length-1];return new Vector2(last.y,last.z);
        }
        public HeldSword(CharacterView actor,EquipmentArt art)
        {
            this.actor=actor;this.art=art;body=actor.Transform.GetComponent<SpriteRenderer>();
            Renderer=new GameObject("Espada — mão esquerda").AddComponent<SpriteRenderer>();
            Renderer.transform.SetParent(actor.Transform,false);
            Renderer.gameObject.layer=actor.Transform.gameObject.layer;
        }
        public void Apply(InventoryData inventory)
        {
            var item=InventoryRules.Equipped(inventory,GearSlot.Weapon);
            ItemId=item?.Id;Renderer.enabled=item!=null&&body.sprite!=null;
            if(item==null||body.sprite==null)return;
            int d=actor.Facing;
            var sprite=body.sprite;
            if(actor.Cycle.Moving){
                var point=Sample((actor.IsRunningAnimation?Run:Walk)[d],actor.AnimationFrame);
                // Points already describe the final visible orientation, including mirrored clips.
                HandPosition=new Vector2((point.x-sprite.pivot.x)/sprite.pixelsPerUnit,
                    (sprite.rect.height-point.y-sprite.pivot.y)/sprite.pixelsPerUnit);
            }else{
                var p=Idle[d];
                HandPosition=new Vector2((p.x*sprite.rect.width-sprite.pivot.x)/sprite.pixelsPerUnit,
                    (p.y*sprite.rect.height-sprite.pivot.y)/sprite.pixelsPerUnit);
            }
            Renderer.sprite=art.Weapons[item.Icon];
            Renderer.transform.localPosition=HandPosition;
            Renderer.transform.localScale=Vector3.one*(item.Style==7?.70f:.57f);
            Renderer.transform.localRotation=Quaternion.Euler(0,0,Angles[d]);
            // East / northeast: left arm is on the far side of the torso.
            Renderer.sortingOrder=body.sortingOrder+((d==2||d==3)?-1:1);
        }
        public void Dispose(){UnityEngine.Object.Destroy(Renderer.gameObject);}
    }
}
