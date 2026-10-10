using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace Vadronia.Editor
{
    public static class LeftHandSwordChecks
    {
        [MenuItem("Vadronia/Verificar espada na mão esquerda")]
        public static void Run()
        {
            if(!Application.isPlaying)throw new Exception("Entre em Play.");
            int count=0;
            Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception(label);count++;};
            var actors=new List<CharacterView>();var swords=new List<HeldSword>();
            var camera=new GameObject("Revisão da mão esquerda").AddComponent<Camera>();
            var target=new RenderTexture(1600,780,24);Texture2D output=null;
            using(var art=new EquipmentArt())
            try
            {
                camera.enabled=false;camera.orthographic=true;camera.orthographicSize=2.5f;
                camera.aspect=1600f/780;camera.transform.position=new Vector3(1000,999,-10);
                camera.cullingMask=1<<29;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.08f,.11f,.16f);camera.targetTexture=target;
                var data=InventoryRules.Starter();
                for(int row=0;row<3;row++)for(int d=0;d<8;d++){
                    var actor=new CharacterView("Espada "+row+" / "+d,new FootPoint(995.6f+d*1.25f,1000-row*1.5f),null,null,false);actors.Add(actor);
                    var held=new HeldSword(actor,art);swords.Add(held);
                    foreach(var t in actor.Transform.GetComponentsInChildren<Transform>())t.gameObject.layer=29;
                    var body=actor.Transform.GetComponent<SpriteRenderer>();var material=body.sharedMaterial;
                    float angle=d*Mathf.PI/4;var direction=new Vector2(Mathf.Sin(angle),-Mathf.Cos(angle));
                    if(row>0){
                        var start=actor.Position;
                        actor.Place(new FootPoint(start.X+direction.x*.4f,start.Y+direction.y*.4f));actor.Animate(0,false,row==2);
                        actor.Transform.position=new Vector3(start.X,start.Y,0);
                    }else {actor.Face(direction);actor.Place(actor.Position);actor.Animate(0);}
                    foreach(var item in ItemCatalog.All)if(item.Slot==GearSlot.Weapon){
                        InventoryRules.Equip(data,item.Id);held.Apply(data);
                        check(held.Renderer.sprite==art.Weapons[item.Icon],"Espada selecionada em todas as direções");
                        check(held.Renderer.enabled&&body.sharedMaterial==material,"Só espada altera apresentação");
                    }
                    InventoryRules.Equip(data,"sword-1");held.Apply(data);
                    if(row==0){
                        if(d==0||d==1||d==7)check(held.HandPosition.x>0,"Esquerda anatômica vista de frente");
                        if(d==3||d==4||d==5)check(held.HandPosition.x<0,"Esquerda anatômica vista de costas");
                    }
                    check((held.Renderer.sortingOrder<body.sortingOrder)==(d==2||d==3),"Oclusão do braço distante");
                    var hand=held.Renderer.transform.localPosition;
                    check(hand.y>.2f&&hand.y<.8f&&Mathf.Abs(hand.x)<.4f,"Empunhadura dentro da região do braço");
                    InventoryRules.Unequip(data,GearSlot.Weapon);held.Apply(data);
                    check(!held.Renderer.enabled,"Retirar arma esconde espada");
                    InventoryRules.Equip(data,"sword-1");held.Apply(data);
                }
                camera.Render();var previous=RenderTexture.active;
                try{RenderTexture.active=target;output=new Texture2D(1600,780,TextureFormat.RGBA32,false);output.ReadPixels(new Rect(0,0,1600,780),0,0);output.Apply();File.WriteAllBytes("docs/references/left-hand-swords.png",output.EncodeToPNG());}
                finally{RenderTexture.active=previous;}
                Debug.Log("LEFT HAND PASS: "+count+" verificações; linhas idle, caminhada e corrida; colunas S, SE, E, NE, N, NW, W, SW.");
            }
            finally{
                foreach(var held in swords)held.Dispose();foreach(var actor in actors)actor.Dispose();
                camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(camera.gameObject);
                if(output!=null)UnityEngine.Object.Destroy(output);
            }
        }
    }
}
