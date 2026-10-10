using System;
using System.Collections.Generic;
using UnityEngine;
namespace Vadronia
{
    public sealed class EquipmentArt:IDisposable
    {
        readonly List<Sprite> sprites=new List<Sprite>();
        public readonly Sprite[] Icons=new Sprite[40];
        public readonly Sprite[] Weapons=new Sprite[8];
        public EquipmentArt()
        {
            Load("swords",0);Load("armor",8);Load("accessories",32);
        }
        void Load(string file,int offset)
        {
            var texture=Resources.Load<Texture2D>("Vadronia/Equipment/"+file);
            var json=Resources.Load<TextAsset>("Vadronia/Equipment/"+file);
            if(texture==null||json==null)throw new InvalidOperationException("Arte de equipamento ausente: "+file);
            var data=JsonUtility.FromJson<Sheet>(json.text);
            for(int i=0;i<data.rects.Length;i++){
                var r=data.rects[i];var sprite=Sprite.Create(texture,new Rect(r.x,texture.height-r.y-r.h,r.w,r.h),new Vector2(.5f,.13f),r.h,0,SpriteMeshType.FullRect);
                Icons[offset+i]=sprite;sprites.Add(sprite);
                if(file=="swords"){
                    float[] handY={371,373,386,377,882,882,882,879};
                    float[] handX={194,576,960,1357,195,576,960,1355};
                    var held=Sprite.Create(texture,sprite.rect,new Vector2((handX[i]-r.x)/r.w,1-(handY[i]-r.y)/r.h),r.h,0,SpriteMeshType.FullRect);
                    Weapons[i]=held;sprites.Add(held);
                }
            }
        }
        public void Dispose(){foreach(var sprite in sprites)UnityEngine.Object.Destroy(sprite);}
        [Serializable] sealed class Sheet { public Cut[] rects; }
        [Serializable] sealed class Cut { public int x,y,w,h; }
    }
}
